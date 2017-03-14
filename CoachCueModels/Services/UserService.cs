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

        public static async Task<User> Get(string id)
        {
            return await DocumentDBRepository<User>.GetItemAsync(id, "Users");
        }

        public static async Task<User> GetByUsername(string username)
        {
            var user = await DocumentDBRepository<User>.GetItemsAsync(us => us.UserName.ToLower() == username.ToLower(), "Users");

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
    }
}
