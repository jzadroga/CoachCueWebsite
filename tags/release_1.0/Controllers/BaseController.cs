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

namespace CoachCue.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var model = filterContext.Controller.ViewData.Model as BaseViewModel;
            if (model != null)
            {
                int? userID = null;

                TopCoaches coaches = user.GetTopCoaches(20);
                model.TopCoaches = coaches.OverallTopCoaches.Take(15).ToList();
                model.WeeklyTopCoaches = coaches.WeeklyTopCoaches;
                
                //have to figure out a way to get random coach list
                model.AskCoaches = model.TopCoaches.Take(4).ToList();
                model.InvitedCoaches = new List<user>();
                model.WeekNumber = coaches.WeekNumber;

                model.Avatar = "sm_profile.jpg";
                user userItem = user.GetByUsername(User.Identity.Name);
                if (userItem != null)
                {
                    if (userID != 0)
                    {
                        userID = userItem.userID;
                        model.Name = (string.IsNullOrEmpty(userItem.fullName)) ? userItem.userName : userItem.fullName;
                        model.Players = user.GetFollowingPlayerCount(userItem.userID);
                        model.Avatar = userItem.avatar.imageName;
                        // model.TotalStarters = ( userItem.users_matchups != null ) ? userItem.users_matchups.Count() : 0;
                        // model.CorrectStarters = (userItem.users_matchups != null) ? userItem.users_matchups.Where(um => um.correctMatchup == true).Count() : 0;
                        model.AccountID = userItem.userID;
                        // model.TotalCreatedMatchups = (userItem.matchups != null) ? userItem.matchups.Count() : 0;
                        model.NoticeCount = userItem.NotificationCount;
                    }
                }

                //model.Matchups = matchup.GetUserMatchups(userID);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
