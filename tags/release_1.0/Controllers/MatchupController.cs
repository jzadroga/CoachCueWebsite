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
using CoachCue.Utility;

namespace CoachCue.Controllers
{
    public class MatchupController : BaseController
    {
        public ActionResult Index([DefaultValue(0)] int mt, [DefaultValue("")] string gud)
        {
            MyMatchupViewModel myMatchVM = new MyMatchupViewModel();
            myMatchVM.MyMatchup = new WeeklyMatchups();
            myMatchVM.MyMatchup.ShowFollow = false;
            myMatchVM.MyMatchup.AllowVote = false;
            myMatchVM.LoggedIn = false;

            int? userID = null;
            if (User.Identity.IsAuthenticated)
            {
                userID = user.GetUserID(User.Identity.Name);
                user.SaveLogin((int)userID);
                myMatchVM.LoggedIn = true;
            }
            else if (!string.IsNullOrEmpty(gud))
            {
                //redirect to the login page so that the user gets automatically logged in 
                return RedirectToAction("LoginByNotice", "Account", new { mt = mt, guid = gud });
            }

            myMatchVM.TrendingItems = user.GetAccountsFromPlayers(nflplayer.GetTrending(15), (userID != 0) ? (int?)userID : null);

            if (mt != 0)
            {
                myMatchVM.MyMatchup = matchup.GetWeeklyMatchupByID(mt, userID);

                myMatchVM.MyMatchup.AllowVote = false;
                myMatchVM.MyMatchup.AllowInvite = false;
                if (!string.IsNullOrEmpty(gud))
                {
                    if (notification.GetByGuid(gud).user1.userID == userID && !matchup.HasVoted((int)userID, mt))
                    {
                        //one final check to make sure the matchup hasn't expired
                        if (myMatchVM.MyMatchup.WeekNumber == gameschedule.GetCurrentWeekID())
                            myMatchVM.MyMatchup.AllowVote = true;
                    }
                }
                else
                {
                    if (userID.HasValue)
                    {
                        if (!matchup.HasVoted((int)userID, mt))
                        {
                            //one final check to make sure the matchup hasn't expired
                            int weekNumber = gameschedule.GetCurrentWeekID();
                            if (myMatchVM.MyMatchup.WeekNumber == weekNumber || (myMatchVM.MyMatchup.WeekNumber == 1 && weekNumber == 0))
                                myMatchVM.MyMatchup.AllowVote = true;
                        }
                    }
                }

                //only allow for either follow or vote - so if vote is set hide the follow
                if (myMatchVM.MyMatchup.AllowVote == true && myMatchVM.MyMatchup.ShowFollow == true)
                    myMatchVM.MyMatchup.ShowFollow = false;
            }
 
            return View(myMatchVM);
        }

        public ActionResult List([DefaultValue(0)] int week)
        {
            MatchupsListViewModel myMatchsVM = new MatchupsListViewModel();

            user userItem = user.GetByUsername(User.Identity.Name);
            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            List<GameWeek> weeks = new List<GameWeek>();
            myMatchsVM.ShowMatchupAdd = false;

            int currentWeek =  gameschedule.GetCurrentWeekID();
            if (week == 0)
            {
                week = currentWeek;
                myMatchsVM.ShowMatchupAdd = true;
            }
            else if (week == currentWeek)
                myMatchsVM.ShowMatchupAdd = true;

            //set up the past weeks
            for (int i = 1; i <= currentWeek; i++)
            {
                if (currentWeek == i)
                    weeks.Add(new GameWeek { ID = i, Label = "Current Week" });
                else
                    weeks.Add(new GameWeek { ID = i, Label = i.ToString() });
            }

            myMatchsVM.Weeks = weeks;
            myMatchsVM.SelectedWeek = week;
            myMatchsVM.AllMatchups = matchup.GetMatchupsByWeek(userID, week);
            myMatchsVM.MyMatchups = myMatchsVM.AllMatchups.Where( mtch => mtch.HasVoted == false && mtch.AllowVote == true ).ToList();
            myMatchsVM.TrendingItems = user.GetAccountsFromPlayers(nflplayer.GetTrending(10), (userID != 0) ? (int?)userID : null);

            return View(myMatchsVM);
        }
    }
}
