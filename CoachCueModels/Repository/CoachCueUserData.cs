using CoachCue.Model;
using CoachCue.Models;
using CoachCue.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CoachCue.Repository
{
    public class CoachCueUserData
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string ProfileImage { get; set; }
        public string Email { get; set; }
        public int NotificationCount { get; set; }
        public string Link { get; set; }
        public UserStatistics Stats { get; set; }
        public List<Badge> Badges { get; set; }

        public static void Reset()
        {
            //clear out a session variable so next call will reload from database
            HttpContext.Current.Session["UserId"] = null;
        }

        public static void SetUserData(string id, string name, string userName, string profileImage, string email, int notificationCount, string link, UserStatistics stats, List<Badge> badges)
        {
            HttpContext.Current.Session["UserId"] = id;
            HttpContext.Current.Session["Name"] = name;
            HttpContext.Current.Session["UserName"] = userName;
            HttpContext.Current.Session["ProfileImage"] = profileImage;
            HttpContext.Current.Session["Email"] = email;
            HttpContext.Current.Session["NotificationCount"] = notificationCount;
            HttpContext.Current.Session["Link"] = link;
            HttpContext.Current.Session["Stats"] = stats;
            HttpContext.Current.Session["Badges"] = badges;
        }

        public async static Task<CoachCueUserData> GetUserData(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                SetUserData(string.Empty, string.Empty, string.Empty, "sm_profile.jpg", email, 0, string.Empty, null, new List<Badge>());
            }
            else
            {
                if (HttpContext.Current.Session["UserId"] == null ||
                        HttpContext.Current.Session["Name"] == null ||
                         HttpContext.Current.Session["UserName"] == null)
                {         
                    var currentUser = await UserService.GetByEmail(email);
                    var notifications = await NotificationService.GetList(currentUser.Id);
                    int count = (notifications.Count() > 0) ? notifications.Where(n => n.Read == false).Count() : 0;
                    SetUserData(currentUser.Id, currentUser.Name, currentUser.UserName, currentUser.Profile.Image, currentUser.Email, count, currentUser.Link, currentUser.Statistics, currentUser.Badges);
                }
            }

            return new CoachCueUserData()
            {
                UserId = HttpContext.Current.Session["UserId"].ToString(),
                Name = HttpContext.Current.Session["Name"].ToString(),
                UserName = HttpContext.Current.Session["UserName"].ToString(),
                ProfileImage = HttpContext.Current.Session["ProfileImage"].ToString(),
                Email = HttpContext.Current.Session["Email"].ToString(),
                NotificationCount = (int)HttpContext.Current.Session["NotificationCount"],
                Link = HttpContext.Current.Session["Link"].ToString(),
                Stats = (UserStatistics)HttpContext.Current.Session["Stats"],
                Badges = (List<Badge>)HttpContext.Current.Session["Badges"]
            };
        }
    }
}
