using CoachCue.Model;
using CoachCue.Models;
using CoachCue.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace CoachCue.Service
{
    public static class UserService
    {
        public static async Task<List<string>> ImportUsers()
        {
            List<string> foundPlayers = new List<string>();
            CoachCueDataContext db = new CoachCueDataContext();

            var usrs = db.users.Where(us => us.status.statusName == "Active").ToList();
            var userDB = await DocumentDBRepository<User>.GetItemsAsync(d => d.Active, "Users");

            var addedUsers = userDB.ToList();

           /* foreach (user usr in usrs)
            {
                if (addedUsers.Where(u => u.Email == usr.email).Count() == 0)
                {
                    await SaveUser(usr, usr.fullName, usr.email, usr.password);
                }
            }
            */
            return foundPlayers;
        }

        public static async Task<User> Create(string name, string email, string password, string openID)
        {
            User user = new User();

            try
            {
                DateTime now = DateTime.UtcNow.GetEasternTime();

                user.Email = email;
                user.Name = name;
                user.UserName =email.Substring(0, email.IndexOf('@'));
                user.Password = (string.IsNullOrEmpty(password)) ? Guid.NewGuid().ToString().Take(10).ToString() : password;
                user.Active = true;
                user.DateCreated = now;
                user.Admin = false;
                user.Verified = false;

                if (HttpContext.Current.Session["media"] != null)
                {
                    if (!string.IsNullOrEmpty(HttpContext.Current.Session["media"].ToString()))
                        user.Referrer = HttpContext.Current.Session["media"].ToString();
                }

                //add profile
                UserProfile profile = new UserProfile();
                profile.Image = "profile.jpg";
                user.Profile = profile;

                //add the settings
                UserSettings settings = new UserSettings();
                settings.EmailNotifications = true;
                user.Settings = settings;

                //add the stats
                UserStatistics stats = new UserStatistics();
                stats.LastLogin = now;
                stats.LoginCount = 1;
                user.Statistics = stats;

                await DocumentDBRepository<User>.CreateItemAsync(user, "Users");
            }
            catch (Exception) { }

            return user;
        }


        //save a user document to the Users collection
        //todo update code for true new user
        public static async Task<User> SaveUser(user usr, string name, string email, string password)
        {
            User user = new User();

            try
            {
                DateTime now = DateTime.UtcNow.GetEasternTime();

                user.Email = email;
                user.Name = name;
                user.UserName = usr.userName; //email.Substring(0, email.IndexOf('@'));
                user.Password = (string.IsNullOrEmpty(password)) ? Guid.NewGuid().ToString().Take(10).ToString() : password;
                user.Active = true;
                user.DateCreated = usr.dateCreated.Value; //now;
                user.Admin = usr.isAdmin; //false;
                user.Verified = false;

                user.Referrer = usr.referrer;
               /* if (HttpContext.Current.Session["media"] != null)
                {
                    if (!string.IsNullOrEmpty(HttpContext.Current.Session["media"].ToString()))
                        user.Referrer = HttpContext.Current.Session["media"].ToString();
                }
                */

                //add profile
                UserProfile profile = new UserProfile();
                profile.Image = usr.avatar.imageName; //"profile.jpg";
                user.Profile = profile;

                //add the settings
                UserSettings settings = new UserSettings();
                settings.EmailNotifications = usr.users_settings.First().emailNotifications.Value; //true;
                user.Settings = settings;

                //add the stats
                UserStatistics stats = new UserStatistics();
                stats.LastLogin = usr.LastLoginDate.Value; //now;
                stats.LoginCount = usr.LoginCount; //1;
                user.Statistics = stats;

                await DocumentDBRepository<User>.CreateItemAsync(user, "Users");
            }
            catch (Exception) { }

            return user;
        }

        public static async Task<User> UpdateLoginStats(string id)
        {
            var user = await Get(id);
            user.Statistics.LoginCount = user.Statistics.LoginCount + 1;
            user.Statistics.LastLogin = DateTime.UtcNow.GetEasternTime();
            await DocumentDBRepository<User>.UpdateItemAsync(id, user, "Users");

            return user;
        }

        public static async Task<User> UpdateVoteCount(string id)
        {
            var user = await Get(id);
            user.Statistics.VoteCount = user.Statistics.VoteCount + 1;

            await DocumentDBRepository<User>.UpdateItemAsync(id, user, "Users");

            return user;
        }

        
        public static async Task<User> UpdateMessageCount(string id)
        {
            var user = await Get(id);
            user.Statistics.MessageCount = user.Statistics.MessageCount + 1;

            await DocumentDBRepository<User>.UpdateItemAsync(id, user, "Users");

            return user;
        }

        public static async Task<User> UpdateMatchupCount(string id)
        {
            var user = await Get(id);
            user.Statistics.MatchupCount = user.Statistics.MatchupCount + 1;

            await DocumentDBRepository<User>.UpdateItemAsync(id, user, "Users");

            return user;
        }

        public static async Task<User> Get(string id)
        {
            return await DocumentDBRepository<User>.GetItemAsync(id, "Users");
        }

        public static async Task<IEnumerable<User>> GetRandomTopVotes(int total)
        {
            var users = await DocumentDBRepository<User>.GetItemsAsync(us => us.Active == true, "Users");

            Random rnd = new Random();
            int start = rnd.Next(0, 25);

            start = 0; //remove this when going live
            users = users.OrderBy(us => us.Statistics.VoteCount).Skip(start).Take(total);

            return users;
        }

        public static async Task<User> GetByUsername(string username)
        {
            var user = await DocumentDBRepository<User>.GetItemsAsync(us => us.UserName.ToLower() == username.ToLower(), "Users");

            return user.FirstOrDefault();
        }

        public static async Task<User> GetByLink(string link)
        {
            var user = await DocumentDBRepository<User>.GetItemsAsync(us => us.Link.ToLower() == link.ToLower(), "Users");

            return user.FirstOrDefault();
        }

        public static async Task<User> GetByEmail(string email)
        {
            var user = await DocumentDBRepository<User>.GetItemsAsync(us => us.Email.ToLower() == email.ToLower(), "Users");

            return user.FirstOrDefault();
        }

        public static async Task<IEnumerable<User>> GetListByIds(List<string> userIds)
        {
            return await DocumentDBRepository<User>.GetItemsAsync(d => userIds.Contains(d.Id), "Users");
        }

        public static async Task<IEnumerable<User>> GetList()
        {
            return await DocumentDBRepository<User>.GetItemsAsync(d => d.Active == true, "Users");
        }

        public static async Task<bool> IsValidUser(string username, string password)
        {
            bool valid = false;
            string COACHCUE_AUTH_COOKIE = "coachcue_auth";

            try
            {
                var user = await DocumentDBRepository<User>.GetItemsAsync(us => us.Email.ToLower() == username.ToLower()
                    && us.Password == password && us.Active == true, "Users");

                var userItem = user.FirstOrDefault();

                if (userItem != null)
                {
                    valid = true;

                    //also set the cookie
                    HttpCookie coachCueCookie = new HttpCookie(COACHCUE_AUTH_COOKIE);
                    coachCueCookie.HttpOnly = false; // Not accessible by JS.
                    coachCueCookie.Values["userGUID"] = userItem.Id;
                    coachCueCookie.Expires = DateTime.UtcNow.GetEasternTime().AddYears(3);

                    HttpContext.Current.Response.Cookies.Add(coachCueCookie);
                }
            }
            catch (Exception)
            {
                valid = false;
            }

            return valid;
        }
    }

    #region Services
    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        Task<bool> ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
    }

    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public async Task<bool> ValidateUser(string userName, string password)
        {
            //if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            //if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return false;

            return await UserService.IsValidUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }
    }

    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session["CurrentUser"] = null;
            HttpContext.Current.Session["UserID"] = null;
        }
    }
    #endregion
}
