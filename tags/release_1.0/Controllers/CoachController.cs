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
    public class CoachController : BaseController
    {
        public ActionResult Index(int id, string name, [DefaultValue(0)]int mt)
        {
            UserViewModel userVM = new UserViewModel();

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            userVM.TrendingItems = user.GetAccountsFromPlayers(nflplayer.GetTrending(15), (userID != 0) ? (int?)userID : null);
            userVM.UserDetail = user.Get(id);

            userVM.UserStream = (mt == 0) ? stream.GetUserStream(id, false, userID) : stream.GetDetails(mt);
            userVM.MessageDetails = (mt == 0) ? false : true;
            userVM.Followers = user.FollowersCount(id);

            return View(userVM);
        }

        public ActionResult LeaderBoard([DefaultValue(0)]int week)
        {
            LeaderBoardModel leaderVM = new LeaderBoardModel();
            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            leaderVM.TrendingItems = user.GetAccountsFromPlayers(nflplayer.GetTrending(15), (userID != 0) ? (int?)userID : null);
            List<GameWeek> weeks = new List<GameWeek>();

            int currentWeek =  gameschedule.GetCurrentWeekID();

            //set up the past weeks
            for (int i = 1; i <= currentWeek - 1; i++)
            {
                weeks.Add(new GameWeek { ID = i, Label = i.ToString() });
            }

            leaderVM.Weeks = weeks;
            leaderVM.SelectedWeek = week;
            leaderVM.LeaderCoaches = user.GetTopCoachesByWeek(50, week);

            return View(leaderVM);
        }
    }
}
