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
    public class WhoDoIStartController : BaseController
    {
        public ActionResult Index(int id, string name)
        {
            MyMatchupViewModel myMatchVM = new MyMatchupViewModel();
            myMatchVM.MyMatchup = new WeeklyMatchups();
            myMatchVM.MyMatchup.ShowFollow = false;
            myMatchVM.MyMatchup.AllowVote = false;
            myMatchVM.MyMatchup.HideDetails = true;

            myMatchVM.LoggedIn = false;

            int? userID = null;
            if (User.Identity.IsAuthenticated)
            {
                userID = user.GetUserID(User.Identity.Name);
                user.SaveLogin((int)userID);
                myMatchVM.LoggedIn = true;
            }

            if (id != 0)
            {
                myMatchVM.MyMatchup = matchup.GetWeeklyMatchupByID(id, userID);
                myMatchVM.MyMatchup.HideDetails = true;
                myMatchVM.MyMatchup.AllowVote = false;
                myMatchVM.MyMatchup.AllowInvite = false;
              
                if (userID.HasValue)
                {
                    if (!matchup.HasVoted((int)userID, id))
                    {
                        //one final check to make sure the matchup hasn't expired
                        int weekNumber = gameschedule.GetCurrentWeekID();
                        if (myMatchVM.MyMatchup.WeekNumber >= weekNumber)
                            myMatchVM.MyMatchup.AllowVote = true;
                    }
                }
                else //allowing guest votes
                {
                    int weekNumber = gameschedule.GetCurrentWeekID();
                    if (myMatchVM.MyMatchup.WeekNumber >= weekNumber)
                        myMatchVM.MyMatchup.AllowVote = true;
                }
                
                //only allow for either follow or vote - so if vote is set hide the follow
                if (myMatchVM.MyMatchup.AllowVote == true && myMatchVM.MyMatchup.ShowFollow == true)
                    myMatchVM.MyMatchup.ShowFollow = false;
            }
            else
                return RedirectToAction("Index", "Error");


            myMatchVM.Matchup = stream.ConvertToStream(myMatchVM.MyMatchup, 0, true, (userID.HasValue) ? (int)userID : 0);
            myMatchVM.RelatedMatchups = matchup.GetRelated((userID.HasValue) ? userID.Value : 0, myMatchVM.MyMatchup);

            return View(myMatchVM);
        }
    }
}
