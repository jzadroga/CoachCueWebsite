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
    public class LeaderBoardController : BaseController
    {
        public ActionResult Index([DefaultValue(50)]int week, [DefaultValue("")]string sort, [DefaultValue("")]string dr)
        {
            LeaderBoardModel leaderVM = new LeaderBoardModel();
            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            List<GameWeek> weeks = new List<GameWeek>();

            //return current week and overall which is week 0

            int currentWeek = gameschedule.GetCurrentWeekID();
            if (week == 50)
                week = 0;//;currentWeek - 1;

            //set up the past weeks
            for (int i = 1; i <= currentWeek; i++)
            {
                weeks.Add(new GameWeek { ID = i, Label = i.ToString() });
            }

            leaderVM.Weeks = weeks;
            leaderVM.SelectedWeek = week;
            leaderVM.Sort = (string.IsNullOrEmpty(sort)) ? "correct" : sort.ToLower();
            leaderVM.Direction = (string.IsNullOrEmpty(dr)) ? "asc" : dr.ToLower();
            leaderVM.LeaderCoaches = user.GetTopCoachesByWeek(100, week, 5);

            switch( leaderVM.Sort )
            {
                case "correct":
                    if (leaderVM.Direction == "asc")
                        leaderVM.LeaderCoaches = leaderVM.LeaderCoaches.OrderByDescending(ldr => ldr.Correct).ToList();
                    else
                        leaderVM.LeaderCoaches = leaderVM.LeaderCoaches.OrderBy(ldr => ldr.Correct).ToList();
                break;
                case "wrong":
                if (leaderVM.Direction == "asc")
                    leaderVM.LeaderCoaches = leaderVM.LeaderCoaches.OrderByDescending(ldr => ldr.Wrong).ToList();
                else
                    leaderVM.LeaderCoaches = leaderVM.LeaderCoaches.OrderBy(ldr => ldr.Wrong).ToList();
                break;
                case "percent":
                if (leaderVM.Direction == "asc")
                    leaderVM.LeaderCoaches = leaderVM.LeaderCoaches.OrderByDescending(ldr => ldr.Percent).ToList();
                else
                    leaderVM.LeaderCoaches = leaderVM.LeaderCoaches.OrderBy(ldr => ldr.Percent).ToList();
                break;
            }


            leaderVM.UserIncluded = false;

            if( userID != 0)
            {

            }

            return View(leaderVM);
        }
    }
}
