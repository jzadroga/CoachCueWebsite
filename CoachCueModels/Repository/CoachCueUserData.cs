using CoachCue.Model;
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

        public static void SetUserData(string id, string name, string userName, string profileImage, string email)
        {
            HttpContext.Current.Session["UserId"] = id;
            HttpContext.Current.Session["Name"] = name;
            HttpContext.Current.Session["UserName"] = userName;
            HttpContext.Current.Session["ProfileImage"] = profileImage;
            HttpContext.Current.Session["Email"] = email;
        }

        public async static Task<CoachCueUserData> GetUserData(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                SetUserData(string.Empty, string.Empty, string.Empty, "sm_profile.jpg", email);
            }
            else
            {
                if (HttpContext.Current.Session["UserId"] == null ||
                        HttpContext.Current.Session["Name"] == null ||
                         HttpContext.Current.Session["UserName"] == null)
                {         
                    var currentUser = await UserService.GetByEmail(email);
                    SetUserData(currentUser.Id, currentUser.Name, currentUser.UserName, currentUser.Profile.Image, currentUser.Email);
                }
            }

            return new CoachCueUserData()
            {
                UserId = HttpContext.Current.Session["UserId"].ToString(),
                Name = HttpContext.Current.Session["Name"].ToString(),
                UserName = HttpContext.Current.Session["UserName"].ToString(),
                ProfileImage = HttpContext.Current.Session["ProfileImage"].ToString(),
                Email = HttpContext.Current.Session["Email"].ToString()
            };
        }
    }
}
