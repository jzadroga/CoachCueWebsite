using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;

namespace CoachCue.Controllers
{
    public class FollowersController : BaseController
    {
        public ActionResult Index(int id, string type, string name)
        {
            CoachCue.ViewModels.FollowersModel fvm = new ViewModels.FollowersModel();
            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;

            fvm.FollowCoaches = new List<Model.user>();
            if(! string.IsNullOrEmpty(type) ){
                fvm.FollowCoaches = user.GetFollowers(type, id);
            }

            if (type == "players")
            {
                nflplayer player = nflplayer.Get(id);
                fvm.FollowItem = player.fullName;
                fvm.FollowImg = player.profilePic;
                fvm.FollowLink = Url.Action("Index", "Player", new { id = player.playerID, name = player.linkFullName });
            }
            else
            {
                user userItem = user.Get(id);
                fvm.FollowItem = userItem.fullName;
                fvm.FollowImg = Url.Content("~/assets/img/avatar/" + userItem.avatar.imageName);
                fvm.FollowLink = Url.Action("Index", "Coach", new { id = userItem.userID, name = userItem.fullName });
            }

            return View(fvm);
        }
    }
}
