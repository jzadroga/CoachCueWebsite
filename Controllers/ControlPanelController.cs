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
using CoachCue.Helpers;

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

        public async Task<ActionResult> Matchups()
        {
            MatchupsViewModel matchupsVM = new MatchupsViewModel();
            var matchups = await MatchupService.GetListByWeek(1);
            matchupsVM.Matchups = matchups.ToList();

            return View(matchupsVM);
        }

        [HttpPost]
        public async Task<ActionResult> UpdatePoints(List<Matchup> matchups)
        {
            foreach(var matchup in matchups)
            {
                var allPoints = matchup.Players.Where(pl => !pl.Points.HasValue).FirstOrDefault();
                if(allPoints == null)
                    await MatchupService.UpdatePoints(matchup);
            }

            return RedirectToAction("Matchups");
        }

        public ActionResult MatchupDelete(int id)
        {
            matchup.Delete(id);

            return RedirectToAction("Matchups");
        }

        public async Task<ActionResult> Messages()
        {
            MessagesViewModel messagesVM = new MessagesViewModel();

            DateTime endDate = DateTime.Now.AddDays(-200);
            messagesVM.Messages = await MessageService.GetList(endDate);

            return View(messagesVM);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMessage(string messageId)
        {
            await MessageService.Delete(messageId);
            return RedirectToAction("Messages");
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

        public async Task<ActionResult> Users([DefaultValue(0)]int page, [DefaultValue("")]string srt, [DefaultValue("")]string search)
        {
            UsersModel users = new UsersModel();
            users.Page = page;
            users.Users = await UserService.ListByPage(page, srt, search);
            users.PageCount = await UserService.UserPageCount() + 1;
            users.Total = await UserService.GetCount();
            users.Search = search;

            return View(users);
        }

        public async Task<ActionResult> AddBadge(string id)
        {
            var user = await UserService.Get(id);
            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> AddBadge(string id, string title, string image)
        {
            var user = await UserService.AddBadge(id, title, image);

            var fromUser = "2706adc2-34ad-404e-9e0c-5b34ea1c5172"; //hardcoded to info@coachcue.com

            var notification = await NotificationService.Save(fromUser, id, "You have earned a new Trophy!", "trophy", title);
            await EmailHelper.SendTrophyNotificationEmail(notification);

            return View(user);
        }

        public async Task<ActionResult> DeleteBadge(string id, string image)
        {
            var user = await UserService.RemoveBadge(id, image);
            return RedirectToAction("AddBadge", new { id = id });
        }

        public async Task<ActionResult> ImportRoster(string slug)
        {
            await PlayerService.ImportRoster(slug);
            return RedirectToAction("TeamRoster", new { slug=slug });
        }

        public ActionResult ImportSchedule()
        {
            GameScheduleService.ImportSchedule(2016);
            return RedirectToAction("Index");
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

        public async Task<ActionResult> PlayerDelete(string id, string teamID)
        {
            var player = await PlayerService.Get(id);
            await PlayerService.Delete(player);

            return RedirectToAction("TeamRoster", new { slug = teamID });
        }

        public async Task<ActionResult> UpdatePlayerTwitter(string teamID, string playerID, [DefaultValue(false)] bool login)
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
            List<TwitterAccount> accounts = await PlayerService.GetTwitterAccounts(playerID, twitterCtx );
            PlayerTwitterModel playerTwitter = new PlayerTwitterModel();
            playerTwitter.PlayerID = playerID;
            playerTwitter.TeamSlug = teamID;
            playerTwitter.TwitterAccounts = accounts;

            return View(playerTwitter);
        }

        public async Task<ActionResult> DeletePlayerTwitter(string teamID, string playerID)
        {
            await PlayerService.UpdateTwitterAccount(playerID, string.Empty, string.Empty);
            return RedirectToAction("TeamRoster", new { slug = teamID });
        }

        [HttpPost]
        public async Task<ActionResult> SelectPlayerTwitter(string slug, string playerID, string name, string image)
        {
            if (!string.IsNullOrEmpty(name)) //add a reference to the player since it is new
                await PlayerService.UpdateTwitterAccount(playerID, name, image);

            return RedirectToAction("TeamRoster", new { slug = slug });
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

            return RedirectToAction("Users");
        }
    }
}
