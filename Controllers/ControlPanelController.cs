using System;
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
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using CoachCue.Service;
using CoachCue.Models;

namespace CoachCue.Controllers
{
    [ControlPanelAuthorization]
    public class ControlPanelController : Controller
    {
        private ICredentialStore credentials = new SessionStateCredentialStore();
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

        public ActionResult MatchupDelete(int id)
        {
            matchup.Delete(id);

            return RedirectToAction("Matchups");
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

        public async Task<ActionResult> JournalistAccounts([DefaultValue("PHI")]string slug)
        {
            TeamJournalistModel teamJournalist = new TeamJournalistModel();
            
            teamJournalist.SelectedTeam = slug;
            teamJournalist.Teams = Team.GetList();
            teamJournalist.Journalists = await PlayerService.ListJournalistsByTeam(slug);
            
            return View(teamJournalist);
        }

        //main action to update players and their twitter information
        public async Task<ActionResult> TeamRoster([DefaultValue("PHI")]string slug)
        {
            TeamRosterModel roster = new TeamRosterModel();

            roster.Teams = Team.GetList();
            roster.SelectedTeam = slug;
            roster.Players = await PlayerService.GetByTeam(slug);

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

        public async Task<ActionResult> ImportRoster(string slug)
        {
            await PlayerService.ImportRoster(slug);
            return RedirectToAction("TeamRoster", new { slug=slug });
        }

        public ActionResult ImportSchedule()
        {
            gameschedule.ImportSchedule(5);
            return RedirectToAction("Index");
        }

        public ActionResult AccountDelete(int id, int playerID, int teamID)
        {
            nflplayer.RemoveTwitterAccount(playerID, id);
            twitteraccount.Delete(id);
            
            return RedirectToAction("TeamRoster", new { team = teamID });
        }

        //deletes a journalist account
        public  async Task<ActionResult> DeleteTwitterAccount(string account, string slug)
        {
            await PlayerService.DeleteBeatWriter(slug, account);

            return RedirectToAction("JournalistAccounts", new { slug = slug });
        }

        [HttpPost]
        public async Task<ActionResult> AddTwitterAccount(string slug, string username)
        {
            //adds a journalist twitter account to a team
            await PlayerService.AddBeatWriter(slug, username);

            return RedirectToAction("JournalistAccounts", new { slug = slug });
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

        public async Task<ActionResult> UpdatePlayerTwitter(int teamID, int playerID, [DefaultValue(false)] bool login)
        {
            //set the twitter information
            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
                credentials.ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
            }

            auth = new MvcAuthorizer
            {
                CredentialStore = credentials
            };

            if (string.IsNullOrEmpty(auth.CredentialStore.ScreenName))
            {
                if (!login)
                {
                    string twitterCallbackUrl = Request.Url.ToString() + "&login=true";
                    return await auth.BeginAuthorizationAsync(new Uri(twitterCallbackUrl));
                }
                else
                {
                    Uri specialUri = new Uri(Request.Url.ToString());
                    await auth.CompleteAuthorizeAsync(specialUri);
                }
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

        public ActionResult DeletePlayerTwitter(int teamID, int playerID)
        {
            int accountID = CoachCue.Model.nflplayer.DeleteTwitterAccount(playerID);
            twitteraccount.Delete(accountID);
            return RedirectToAction("TeamRoster", new { team = teamID });
        }

        [HttpPost]
        public ActionResult SelectPlayerTwitter(int teamID, int playerID, int accountID, string twitterID, string username, string name, string image, string description)
        {
            twitteraccount twtAccount = twitteraccount.Save(accountID, twitterID, username, name, image, 1, "Active", description);
            if (accountID == 0) //add a reference to the player since it is new
            {
                nflplayer.UpdateTwitterAccount(playerID, twtAccount.twitterAccountID);
                string cacheID = "nflplayer" + playerID.ToString();
                HttpContext.Cache.Remove(cacheID);
            }

            return RedirectToAction("TeamRoster", new { team = teamID });
        }

        public async Task<ActionResult> BuildPlayerJson()
        {
            using (FileStream fs = System.IO.File.Open(Server.MapPath("~/assets/data/players.json"), FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;

                JsonSerializer serializer = new JsonSerializer();
                var players = await PlayerService.GetJsonList();
                serializer.Serialize(jw, players);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> BuildUserJson()
        {
            using (FileStream fs = System.IO.File.Open(Server.MapPath("~/assets/data/users.json"), FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;

                JsonSerializer serializer = new JsonSerializer();
                var users = await UserService.GetList();
                var jsonUser = users.Select( us => new {
                    name = us.Name,
                    username =  us.UserName,
                    image = "/assets/img/avatar/" + us.Profile.Image,
                    value = us.Name,
                    userID = us.Id,
                    link = us.Link 
                }).ToList();

                serializer.Serialize(jw, jsonUser);
            }

            return RedirectToAction("Index");
        }
    }
}
