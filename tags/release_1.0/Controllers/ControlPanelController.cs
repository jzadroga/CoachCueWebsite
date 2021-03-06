﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Utility;
using CoachCue.ViewModels;
using System.ComponentModel;
using LinqToTwitter;
using System.Configuration;
using CoachCue.Model;

namespace CoachCue.Controllers
{
    [ControlPanelAuthorization]
    public class ControlPanelController : Controller
    {
        private IOAuthCredentials credentials = new SessionStateCredentials();
        private MvcAuthorizer auth;
        private TwitterContext twitterCtx;

        public ActionResult Index()
        {
            return RedirectToAction("TeamRoster");
        }

        public JsonResult SetMatchupPoints(int pts1, int pts2, int id)
        {
            matchup.UpdatePoints(id, pts1, pts2);
            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Matchups()
        {
            MatchupsViewModel matchupsVM = new MatchupsViewModel();
            matchupsVM.Players = nflplayer.ListFantasyOffense();
            matchupsVM.Matchups = matchup.List(true);

            return View(matchupsVM);
        }

        public ActionResult Messages()
        {
            MessagesViewModel messagesVM = new MessagesViewModel();
            messagesVM.Messages = message.GetAll();

            return View(messagesVM);
        }

        [HttpPost]
        public ActionResult AddMatchup(int player1, int player2)
        {
            matchup.Add(player1, player2, user.GetUserID(User.Identity.Name));
            return RedirectToAction("Matchups");
        }

        public ActionResult JournalistAccounts([DefaultValue(0)]int team)
        {
            TeamJournalistModel teamJournalist = new TeamJournalistModel();
            
            teamJournalist.SelectedTeamID = (team == 0) ? nflteam.List()[0].teamID : team;
            teamJournalist.Teams = nflteam.List();
            teamJournalist.Journalists = twitteraccount.ListJournalistsByTeam(teamJournalist.SelectedTeamID);
            
            return View(teamJournalist);
        }

        //main action to update players and their twitter information
        public ActionResult TeamRoster([DefaultValue(0)]int team)
        {
            TeamRosterModel roster = new TeamRosterModel();

            int teamID = (team == 0) ? nflteam.List()[0].teamID : team;
            roster.Teams = nflteam.List();
            roster.SelectedTeamID = teamID;
            roster.Players = nflplayer.GetRoster(teamID);

            return View(roster);
        }

        public ActionResult Users([DefaultValue(0)]int page, [DefaultValue("")]string srt)
        {
            UsersModel users = new UsersModel();
            users.Page = page;
            users.Users = user.ListByPage(page, srt);
            users.PageCount = user.UserPageCount() + 1;
            users.Total = user.GetCount();

            return View(users);
        }

        public ActionResult ImportRoster(int teamID)
        {
            CoachCue.Model.nflplayer.ImportRoster(CoachCue.Model.nflteam.Get(teamID).teamSlug);
            return RedirectToAction("TeamRoster", new { team=teamID });
        }

        public ActionResult AccountDelete(int id, int playerID, int teamID)
        {
            nflplayer.RemoveTwitterAccount(playerID, id);
            twitteraccount.Delete(id);
            
            return RedirectToAction("TeamRoster", new { team = teamID });
        }

        //deletes a journalist account
        public ActionResult DeleteTwitterAccount(int id, int teamID)
        {
            nflteam.DeleteTwitterAccount(id);
            twitteraccount.Delete(id);
            return RedirectToAction("JournalistAccounts", new { team = teamID });
        }

        [HttpPost]
        public ActionResult AddTwitterAccount(int teamID, string name, string username)
        {
            //adds a journalist twitter account to a team
            twitteraccount account = twitteraccount.Save(null, string.Empty, username, name, string.Empty, twitteraccounttype.GetTypeID("Journalist"), "Active", string.Empty);
            nflteam.AddTwitterAccount(account.twitterAccountID, teamID);

            return RedirectToAction("JournalistAccounts", new { team = teamID });
        }

        public ActionResult PlayerDelete(int id, int teamID)
        {
            CoachCue.Model.nflplayer.Delete(id);

            return RedirectToAction("TeamRoster", new { team = teamID });
        }

        public ActionResult GetTeamPlayers( string slug )
        {
            List<nflplayer> players = nflplayer.ListByTeam(slug);
            return PartialView("_TeamPlayerRow", players);
        }

        public ActionResult UpdateUsers()
        {
            /*coachCueEntities db = new coachCueEntities();
           
            var ret = from mt in db.users
                        select mt;

            foreach (user usr in ret.ToList())
            {
                usr.userGuid = Guid.NewGuid().ToString();

                users_settings usrset = new users_settings();
                usrset.userID = usr.userID;
                usrset.emailNotifications = true;
                db.users_settings.AddObject(usrset);

                db.SaveChanges();
            }
            */
            return View();
        }

        public ActionResult SendNotificationEmail()
        {
            //ViewData["sent"] = EmailHelper.SendNotificationEmail();
            return View();
        }

        public ActionResult UpdatePlayerTwitter(int teamID, int playerID)
        {
            //set the twitter information
            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
                credentials.ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
            }

            auth = new MvcAuthorizer
            {
                Credentials = credentials
            };

            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                Uri specialUri = new Uri(Request.Url.ToString());
                return auth.BeginAuthorization(specialUri);
            }

            twitterCtx = new TwitterContext(auth);

            //get the list of possible twitter accounts for a player
            List<twitteraccount> accounts = twitteraccount.GetTwitterAccounts( playerID, twitterCtx );
            PlayerTwitterModel playerTwitter = new PlayerTwitterModel();
            playerTwitter.PlayerID = playerID;
            playerTwitter.TeamID = teamID;
            playerTwitter.TwitterAccounts = accounts;

            return View(playerTwitter);
        }

        [HttpPost]
        public ActionResult SelectPlayerTwitter(int teamID, int playerID, int accountID, string twitterID, string username, string name, string image, string description)
        {
            twitteraccount twtAccount = twitteraccount.Save(accountID, twitterID, username, name, image, 1, "Active", description);
            if (accountID == 0) //add a reference to the player since it is new
                nflplayer.UpdateTwitterAccount(playerID, twtAccount.twitterAccountID);

            return RedirectToAction("TeamRoster", new { team = teamID });
        }

        public ActionResult BuildTeamTwitter(int teamID)
        {           
            //set the twitter information
            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
                credentials.ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
            }

            auth = new MvcAuthorizer
            {
                Credentials = credentials
            };

            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                Uri specialUri = new Uri(Request.Url.ToString());
                return auth.BeginAuthorization(specialUri);
            }

            twitterCtx = new TwitterContext(auth);

            foreach (nflplayer player in nflplayer.GetRoster(teamID))
            {     
                //now check the players twitter account
                twitteraccount.BuildPlayerTwitterAccount(player, twitterCtx);
            }

            return RedirectToAction("TeamRoster", new { team = teamID });
        }
    }
}
