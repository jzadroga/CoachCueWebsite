using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;
using System.ComponentModel;
using CoachCue.ViewModels;
using System.Net;
using System.IO;
using System.Web.Routing;
using CoachCue.Helpers;
using CoachCue.Utility;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using CoachCue.Repository;
using CoachCue.Service;
using System.Threading.Tasks;
using CoachCue.Models;

namespace CoachCue.Controllers
{
    public class AccountController : BaseController
    {
        public string COACHCUE_AUTH_COOKIE = "coachcue_auth";

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }
        public OpenIdRelyingParty openId { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (openId == null) { openId = new OpenIdRelyingParty(); }
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Login()
        {
            ViewData["valid"] = "";
            return RedirectToAction("Index", "Home");
        }

        public JsonResult EmailExists(string email)
        {
            bool exist = user.EmailExists(email);

            return Json(new { EmailExists = exist });
        }

        [HttpPost]
        public async Task<ActionResult> CreateAccount(string regname, string regemail, string regwrd)
        {
            //check the email again to avoid dups
            if (user.EmailExists(regemail))
                return RedirectToAction("Index", "Home");

            var acntUser = await UserService.Create(regname, regemail, regwrd, string.Empty);

            if (await MembershipService.ValidateUser(acntUser.Email, acntUser.Password))
            {
                CoachCueUserData.SetUserData(acntUser.Id, acntUser.Name, acntUser.UserName, acntUser.Profile.Image, acntUser.Email, 0, acntUser.Link, acntUser.Statistics, acntUser.Badges);
                FormsService.SignIn(acntUser.Email, true);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> LoginByCookie(string usr, string url)
        {
            user userItem = user.GetByGuid(usr);

            if (await MembershipService.ValidateUser(userItem.email, userItem.password))
            {
                FormsService.SignIn(userItem.email, true);
                return Redirect(url);
            }

            return RedirectToAction("Login", "Account");
        }

        public async Task<ActionResult> LoginBySettings(string usr)
        {
            user userItem = user.GetByGuid(usr);

            if (await MembershipService.ValidateUser(userItem.email, userItem.password))
            {
                FormsService.SignIn(userItem.email, true);
                return RedirectToAction("Settings", "Account", new { usr = usr });
            }

            return RedirectToAction("Login", "Account");
        }

        public ActionResult LoginByOpenID(string redirectUrl, string pvdr)
        {
            IAuthenticationResponse response = openId.GetResponse();

            if (response == null)
            {
                Identifier idProvider = (pvdr == "g") ? WellKnownProviders.Google : WellKnownProviders.Yahoo;
                IAuthenticationRequest request = openId.CreateRequest(Identifier.Parse(idProvider));

                var fr = new FetchRequest();
                fr.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
                fr.Attributes.AddRequired(WellKnownAttributes.Name.First);
                fr.Attributes.AddRequired(WellKnownAttributes.Name.Last);
                request.AddExtension(fr);

                // Require some additional data
                request.AddExtension(new ClaimsRequest
                {
                    Email = DemandLevel.Require,
                    FullName = DemandLevel.Require,
                    Nickname = DemandLevel.Require
                });

                return request.RedirectingResponse.AsActionResult();
            }
            else // response != null
            {
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        string userOpenID = response.ClaimedIdentifier.ToString();
                        user userItem = user.GetUserByOpenID(userOpenID);

                        //already have account so just login
                        if (userItem != null)
                            FormsService.SignIn(userItem.email, true);
                        else
                        {
                            //create new account
                            var fetch = response.GetExtension<FetchResponse>();
                            string name = fetch.GetAttributeValue(WellKnownAttributes.Name.First);
                            name += " " + fetch.GetAttributeValue(WellKnownAttributes.Name.Last);
                            string email = fetch.GetAttributeValue(WellKnownAttributes.Contact.Email);

                            user acntUser = user.Create(name, email, string.Empty, userOpenID);
                            if( acntUser != null )
                                FormsService.SignIn(acntUser.email, true);
                        }
                        if (!string.IsNullOrEmpty(redirectUrl))
                            return Redirect(redirectUrl);
                        break;
                    case AuthenticationStatus.Canceled:
                        ModelState.AddModelError("loginIdentifier",
                            "Login was cancelled at the provider");
                        break;
                    case AuthenticationStatus.Failed:
                        ModelState.AddModelError("loginIdentifier",
                            "Login failed using the provided OpenID identifier");
                        break;
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> LoginByNotice(string guid, int mt)
        {
            notification notice = notification.GetByGuid(guid);

            user userItem = user.Get(notice.sentTo);
            if (await MembershipService.ValidateUser(userItem.email, userItem.password))
            {
                FormsService.SignIn(userItem.email, true);
                return RedirectToAction("Index", "Matchup", new { mt = mt, gud = guid });
            }

            return RedirectToAction("Index", "Matchup", new { mt = mt });
        }

        [HttpPost]
        public async Task<ActionResult> Login(string username, string passwrd, string rememberMe, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (await MembershipService.ValidateUser(username, passwrd))
                {
                    FormsService.SignIn(username, (rememberMe == "on") ? true : false);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                       && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            return RedirectToAction("Index", "Signup", new { vld = true });
        }

        [HttpPost]
        public async Task<JsonResult> LogOnAjax(string username, string password, string rememberMe)
        {
            bool success = false;
            if (ModelState.IsValid)
            {
                if (await MembershipService.ValidateUser(username, password))
                {
                    var usr = await UserService.GetByEmail(username);
                    var notifications = await NotificationService.GetList(usr.Id);
                    int count = (notifications.Count() > 0) ? notifications.Where(n => n.Read == false).Count() : 0;

                    CoachCueUserData.SetUserData(usr.Id, usr.Name, usr.UserName, usr.Profile.Image, usr.Email, count, usr.Link, usr.Statistics, usr.Badges); 

                    //always have the rememberme cookie set
                    FormsService.SignIn(username, true);

                    success = true;
                }
            }

            // If we got this far, something failed, redisplay form
            return Json(new { Success = success });
        }

        public ActionResult LogOff()
        {
            FormsService.SignOut();

            HttpCookie coachCueCookie = new HttpCookie(COACHCUE_AUTH_COOKIE);
            coachCueCookie.HttpOnly = false; // Not accessible by JS.
            coachCueCookie.Values["userGUID"] = "";
            coachCueCookie.Expires = DateTime.Now.AddHours(3);

            HttpContext.Response.Cookies.Add(coachCueCookie);

            HttpContext.Session["UserId"] = string.Empty;
            HttpContext.Session["Name"] = string.Empty;
            HttpContext.Session["UserName"] = string.Empty;

            return RedirectToAction("Index", "Home");
        }

        [NoCacheAttribute]
        [SiteAuthorization]
        public JsonResult Invite(string email, string msg)
        {
            bool sent = false;

            user userItem = user.GetByUsername(User.Identity.Name);
            if (!string.IsNullOrEmpty(email))
            {
                EmailHelper.Send(email, userItem.fullName, msg);
                sent = true;
            }

            return Json(new { Sent = sent }, JsonRequestBehavior.AllowGet);
        }
   
        [NoCacheAttribute]
        public JsonResult InviteRequest(string email)
        {
            bool sent = false;

            EmailHelper.Send("info@coachcue.com", "The following email has requested an invite to CoachCue: " + email, "CoachCue Invite Request", "CoachCue");
            sent = true;

            return Json(new { Sent = sent }, JsonRequestBehavior.AllowGet);
        }


        [NoCacheAttribute]
        public JsonResult SendPassword(string email)
        {
            bool sent = false;
            user account = user.GetByEmail(email);
            string msg = "get message";

            if( !string.IsNullOrEmpty( account.password ))
            {
                string bodyMsg = "Here is your password to CoachCue.com as requested: " + account.password;
                msg = EmailHelper.Send(email, bodyMsg, "CoachCue Password Reminder", "CoachCue");
                sent = true;
            }

            return Json(new { Sent = sent }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        public async Task<ActionResult> Profile()
        {
            var userData = await CoachCueUserData.GetUserData(User.Identity.Name);
            ProfileViewModel pVM = new ProfileViewModel(userData);
            await LoadBaseViewModel(pVM);

            return View(pVM);
        }

        [SiteAuthorization]
        public async Task<ActionResult> Settings()
        {
            SettingsViewModel settingsVM = new SettingsViewModel();

            await LoadBaseViewModel(settingsVM);

            settingsVM.DisplayMessage = false;
            settingsVM.CurrentTab = "emailnotices";

            settingsVM.RecieveNotificationEmailOptions = new[]
            {
                new SelectListItem { Value = "1", Text = "Yes" },
                new SelectListItem { Value = "0", Text = "No thanks" },
            };

            var user = await UserService.GetByEmail(User.Identity.Name);
            bool notices = user.Settings.EmailNotifications;

            settingsVM.RecieveNotificationEmail = (notices) ? "1" : "0";

            return View(settingsVM);
        }

        [HttpPost]
        [SiteAuthorization]
        public async Task<ActionResult> UpdateSettingsEmailNotices(SettingsViewModel emailSettings)
        {
            SettingsViewModel settingsVM = new SettingsViewModel();
            await LoadBaseViewModel(settingsVM);

            if (ModelState.IsValid)
            { 
                await UserService.UpdateSettings( settingsVM.UserData.UserId, (emailSettings.RecieveNotificationEmail == "1") ? true : false );
                settingsVM.DisplayMessage = true;
                settingsVM.Message = "Thanks, your settings have been updated";
            }

            settingsVM.CurrentTab = "emailnotices";
            settingsVM.RecieveNotificationEmailOptions = new[]
            {
                new SelectListItem { Value = "1", Text = "Yes" },
                new SelectListItem { Value = "0", Text = "No thanks" },
            };
            settingsVM.RecieveNotificationEmail = emailSettings.RecieveNotificationEmail;

            return View("Settings", settingsVM);
        }

        [SiteAuthorization]
        public async Task<ActionResult> Notifications()
        {
            var userData = await CoachCueUserData.GetUserData(User.Identity.Name);
            userData.NotificationCount = 0;

            NotificationsViewModel notVM = new NotificationsViewModel();
            await LoadBaseViewModel(notVM);

            var notifications = await NotificationService.GetList(userData.UserId);
            notifications = notifications.OrderByDescending(nt => nt.DateCreated).Take(100);

            var notices = new List<NotificationNotice>();
            //mark all as read
            foreach (var notification in notifications)
            {
                if (notification.Read == false)
                {
                    notification.Read = true;
                    await NotificationService.Update(notification);
                }

                var userFrom = await UserService.Get(notification.UserFrom);
                var message = (string.IsNullOrEmpty(notification.Message)) ? null : await MessageService.Get(notification.Message);
                var matchup = (string.IsNullOrEmpty(notification.Matchup)) ? null : await MatchupService.Get(notification.Matchup);
                notices.Add(new NotificationNotice()
                {
                    UserFrom = userFrom,
                    Message = message,
                    Matchup = matchup,
                    Notification = notification
                });
            }

            notVM.Notifications = notices;

            //reset the session
            CoachCueUserData.Reset();
            return View(notVM);
        }

        [HttpPost]
        [SiteAuthorization]
        public async Task <ActionResult> UpdateProfileBasic(ProfileViewModel basicProfile)
        {
            var userData = await CoachCueUserData.GetUserData(User.Identity.Name);
            ProfileViewModel pVM = new ProfileViewModel(userData);
            await LoadBaseViewModel(pVM);

            if (ModelState.IsValid)
            {
                await UserService.UpdateProfile(userData.UserId, basicProfile.FullName, basicProfile.Email, basicProfile.AccountUserName);
                pVM.DisplayMessage = true;
                pVM.Message = "Thanks, your profile has been updated";
            }
            
            pVM.CurrentTab = "profile";

            //reset the session
            CoachCueUserData.Reset();
            return View("Profile", pVM);
        }

        [HttpPost]
        [SiteAuthorization]
        public async  Task<ActionResult> UpdatePassword(ProfileViewModel basicProfile)
        {
            var userData = await CoachCueUserData.GetUserData(User.Identity.Name);
            ProfileViewModel pVM = new ProfileViewModel(userData);
            await LoadBaseViewModel(pVM);

            if (ModelState.IsValid)
            {
                await UserService.UpdatePassword(userData.UserId, basicProfile.Password);
                pVM.DisplayMessage = true;
                pVM.Message = "Thanks, your password has been updated";
            }

            pVM.CurrentTab = "password";

            //reset the session
            CoachCueUserData.Reset();
            return View("Profile", pVM);
        }

        [HttpPost]
        [SiteAuthorization]
        public async Task<ActionResult> UploadAvatar(HttpPostedFileBase avatar)
        {
            string fileName = string.Empty;
            var userData = await CoachCueUserData.GetUserData(User.Identity.Name);
            ProfileViewModel pVM = new ProfileViewModel(userData);
            await LoadBaseViewModel(pVM);

            if (avatar != null && !string.IsNullOrEmpty(avatar.FileName))
            {
                fileName = await UserService.UpdateAvatar(userData.UserId, avatar);
                userData.ProfileImage = fileName;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                pVM.DisplayMessage = true;
                pVM.Message = "Thanks, your picture has been updated";
            }

            pVM.CurrentTab = "picture";

            //reset the session
            CoachCueUserData.Reset();
            return View("Profile", pVM);
        }

        public ThumbnailImageResult GetThumbnail(string fileName, [DefaultValue(40)] int size)
        {
            string imagePath =  Request.PhysicalApplicationPath + "assets\\img\\avatar\\"+ fileName;
            return new ThumbnailImageResult(imagePath, size);
        }
    }
}
