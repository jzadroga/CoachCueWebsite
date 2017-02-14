using CoachCue.Model;
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

        public static void SetUserData(string id, string name, string userName, string profileImage)
        {
            HttpContext.Current.Session["UserId"] = id;
            HttpContext.Current.Session["Name"] = name;
            HttpContext.Current.Session["UserName"] = userName;
            HttpContext.Current.Session["ProfileImage"] = profileImage;
        }

        public static CoachCueUserData GetUserData(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                SetUserData(string.Empty, string.Empty, string.Empty, "sm_profile.jpg");
            }
            else
            {
                if (HttpContext.Current.Session["UserId"] == null ||
                        HttpContext.Current.Session["Name"] == null ||
                         HttpContext.Current.Session["UserName"] == null)
                {
                
               
                        var currentUser = user.GetUserByEmail(email);
                        SetUserData(currentUser.userID.ToString(), currentUser.fullName, currentUser.userName, currentUser.avatar.imageName);
                    
                    /*return new CoachCueUserData()
                    {
                        UserId = currentUser.userID.ToString(),
                        Name = currentUser.fullName,
                        UserName = currentUser.userName,
                        ProfileImage = currentUser.avatar.imageName
                    };*/
                }
            }

            return new CoachCueUserData()
            {
                UserId = HttpContext.Current.Session["UserId"].ToString(),
                Name = HttpContext.Current.Session["Name"].ToString(),
                UserName = HttpContext.Current.Session["UserName"].ToString(),
                ProfileImage = HttpContext.Current.Session["ProfileImage"].ToString()
            };
        }
    }
}
