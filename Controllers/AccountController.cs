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

        public ActionResult Register([DefaultValue("")] string code)
        {
            return View();
        }

        public JsonResult EmailExists(string email)
        {
            bool exist = user.EmailExists(email);

            return Json(new { EmailExists = exist });
        }

        [HttpPost]
        public ActionResult CreateAccount(string regname, string regemail, string regwrd)
        {
            //check the email again to avoid dups
            if (user.EmailExists(regemail))
                return RedirectToAction("Index", "Home");

            user acntUser = user.Create(regname, regemail, regwrd, string.Empty);

            if (MembershipService.ValidateUser(acntUser.email, acntUser.password))
                FormsService.SignIn(acntUser.email, false);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult LoginByCookie(string usr, string url)
        {
            user userItem = user.GetByGuid(usr);

            if (MembershipService.ValidateUser(userItem.email, userItem.password))
            {
                FormsService.SignIn(userItem.email, true);
                return Redirect(url);
            }

            return RedirectToAction("Login", "Account");
        }

        public ActionResult LoginBySettings(string usr)
        {
            user userItem = user.GetByGuid(usr);

            if (MembershipService.ValidateUser(userItem.email, userItem.password))
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

        public ActionResult LoginByNotice(string guid, int mt)
        {
            notification notice = notification.GetByGuid(guid);

            user userItem = user.Get(notice.sentTo);
            if (MembershipService.ValidateUser(userItem.email, userItem.password))
            {
                FormsService.SignIn(userItem.email, true);
                return RedirectToAction("Index", "Matchup", new { mt = mt, gud = guid });
            }

            return RedirectToAction("Index", "Matchup", new { mt = mt });
        }

        [HttpPost]
        public ActionResult Login(string username, string passwrd, string rememberMe, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(username, passwrd))
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
                if (MembershipService.ValidateUser(username, password))
                {
                    var usr = await UserService.GetByEmail(username);
                    CoachCueUserData.SetUserData(usr.Id, usr.Name, usr.UserName, usr.Profile.Image, usr.Email); 

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
        public ActionResult Following([DefaultValue(0)] int id)
        {
            int userID = (id == 0 ) ? user.GetUserID(User.Identity.Name) : id;
            FollowingModel fvm = new ViewModels.FollowingModel();
            fvm.CurrentUserID = user.GetUserID(User.Identity.Name);

            if (userID != 0)
            {
                fvm.FollowingCoaches = user.GetFollowingUsers(userID);
                fvm.FollowingPlayers = user.GetFollowingPlayers(userID);
                fvm.UserDetail = user.Get(userID);

                fvm.FollowingCount = fvm.FollowingCoaches.Count() + fvm.FollowingPlayers.Count();
            }

            return View(fvm);
        }

        [SiteAuthorization]
        public ActionResult Profile()
        {
            ProfileViewModel pVM = new ProfileViewModel(user.GetByUsername(User.Identity.Name));
            return View(pVM);
        }

        public ActionResult Settings([DefaultValue("")] string usr)
        {
            if (!User.Identity.IsAuthenticated)
                //redirect to the login page so that the user gets automatically logged in 
                return RedirectToAction("LoginBySettings", "Account", new {usr = usr });

            SettingsViewModel settingsVM = new SettingsViewModel();
            settingsVM.DisplayMessage = false;
            settingsVM.CurrentTab = "emailnotices";

            settingsVM.RecieveNotificationEmailOptions = new[]
            {
                new SelectListItem { Value = "1", Text = "Yes" },
                new SelectListItem { Value = "0", Text = "No thanks" },
            };

            user userItem = user.GetByUsername(User.Identity.Name);
            bool? notices = user.GetSettings( userItem.userID ).emailNotifications;

            settingsVM.RecieveNotificationEmail = (notices.Value == true) ? "1" : "0";

            return View(settingsVM);
        }

        [HttpPost]
        [SiteAuthorization]
        public ActionResult UpdateSettingsEmailNotices(SettingsViewModel emailSettings)
        {
            bool success = false;
            user userItem = user.GetByUsername(User.Identity.Name);
            if (ModelState.IsValid)
            {
                success = user.UpdateEmailSettings(userItem.userID, emailSettings.RecieveNotificationEmail);
            }

            SettingsViewModel settingsVM = new SettingsViewModel();
            if (success)
            {
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
        public ActionResult Notifications()
        {
            NotificationsViewModel notVM = new NotificationsViewModel();

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            notVM.Notifications = notification.GetByUserID(userID, true);
           
            return View(notVM);
        }

        [HttpPost]
        [SiteAuthorization]
        public ActionResult UpdateProfileBasic(ProfileViewModel basicProfile)
        {
            bool success = false;
            user userItem = user.GetByUsername(User.Identity.Name);
            if (ModelState.IsValid)
            {
                success = user.UpdateProfile(userItem.userID, basicProfile.FullName, basicProfile.Email, basicProfile.AccountUserName);
            }

            ProfileViewModel pVM = new ProfileViewModel(userItem);
            if (success)
            {
                pVM.DisplayMessage = true;
                pVM.Message = "Thanks, your profile has been updated";
            }
            pVM.CurrentTab = "profile";
            return View("Profile", pVM);
        }

        [HttpPost]
        [SiteAuthorization]
        public ActionResult UpdatePassword(ProfileViewModel basicProfile)
        {
            bool success = false;

            user userItem = user.GetByUsername(User.Identity.Name);
            if (ModelState.IsValid)
            {
                success = user.UpdatePassword(userItem.userID, basicProfile.Password);
            }

            ProfileViewModel pVM = new ProfileViewModel(userItem);
            if( success )
            {
                pVM.DisplayMessage = true;
                pVM.Message = "Thanks, your password has been updated";
            }
            pVM.CurrentTab = "password";
            return View("Profile", pVM);
        }

        [HttpPost]
        [SiteAuthorization]
        public ActionResult UploadAvatar(HttpPostedFileBase avatar)
        {
            string fileName = string.Empty;
            user userItem = user.GetByUsername(User.Identity.Name);

            if (avatar != null && !string.IsNullOrEmpty(avatar.FileName))
            {
                fileName = user.UpdateAvatar(userItem.userID, avatar);
            }

            ProfileViewModel pVM = new ProfileViewModel(userItem);
            if (!string.IsNullOrEmpty(fileName))
            {
//                pVM.Avatar = fileName;
                pVM.DisplayMessage = true;
                pVM.Message = "Thanks, your picture has been updated";
            }

            pVM.CurrentTab = "picture";
            return View("Profile", pVM);
        }

        public ThumbnailImageResult GetThumbnail(string fileName, [DefaultValue(40)] int size)
        {
            string imagePath =  Request.PhysicalApplicationPath + "assets\\img\\avatar\\"+ fileName;
            return new ThumbnailImageResult(imagePath, size);
        }
    }
}
