using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Helpers;
using CoachCue.Model;
using CoachCue.ViewModels;
using System.ComponentModel;

namespace CoachCue.Controllers
{
    public class PostController : Controller
    {
        [NoCacheAttribute]
        public ActionResult NewMessage( [DefaultValue("")] string ply)
        {
            MessageViewModel msgVM = new MessageViewModel();

            msgVM.User = user.GetByUsername(User.Identity.Name);
            msgVM.Type = "general";
            msgVM.ParentID = 0;

            if (!string.IsNullOrEmpty(ply))
            {
                List<nflplayer> players = new List<nflplayer>();
                players.Add(nflplayer.Get( Convert.ToInt32(ply) ));
                msgVM.MessagePlayers = players;
            }

            return View(msgVM);
        }

        [NoCacheAttribute]
        public ActionResult ReplyMessage(int msgID)
        {
            MessageViewModel msgVM = new MessageViewModel();

            msgVM.User = user.GetByUsername(User.Identity.Name);
            msgVM.ParentMessage = message.Get(msgID);
            msgVM.MessagePlayers = msgVM.ParentMessage.MessagePlayers;
            msgVM.Type = "general";
            msgVM.ParentID = msgVM.ParentMessage.messageID;

            return View(msgVM);
        }

        [NoCacheAttribute]
        public ActionResult NewMatchupMessage(int mtchpID)
        {
            MessageViewModel msgVM = new MessageViewModel();

            msgVM.User = user.GetByUsername(User.Identity.Name);
            matchup currentMatch = matchup.Get(mtchpID);
            msgVM.Matchup = currentMatch;

            List<nflplayer> players = new List<nflplayer>();
            players.Add(currentMatch.nflplayer);
            players.Add(currentMatch.nflplayer1);
            msgVM.MessagePlayers = players;
            msgVM.Type = "matchup";
            msgVM.ParentID = currentMatch.matchupID;

            return View(msgVM);
        }

        [NoCacheAttribute]
        public ActionResult NewMatchup([DefaultValue(0)] int ply1)
        {
            MessageViewModel msgVM = new MessageViewModel();

            msgVM.User = user.GetByUsername(User.Identity.Name);
            
            //get list of potential people to invite to answer the matchup
            List<user> askUsers = new List<user>();

            if (Request.Browser.IsMobileDevice)
                msgVM.MatchupInvites = new List<user>();
            else
                msgVM.MatchupInvites = user.GetInviteUser(msgVM.User.userID, 4);

            if (ply1 != 0)
            {
                List<nflplayer> players = new List<nflplayer>();
                players.Add(nflplayer.Get(ply1));
                msgVM.MessagePlayers = players;
            }

            return View(msgVM);
        }

        [NoCacheAttribute]
        public ActionResult Invite(int mtchpID)
        {
            MessageViewModel msgVM = new MessageViewModel();

            msgVM.User = user.GetByUsername(User.Identity.Name);
            msgVM.Matchup = matchup.Get(mtchpID);

            int userCount = (Request.Browser.IsMobileDevice) ? 1 : 4;

            //get list of potential people to invite to answer the matchup
            List<user> askUsers = new List<user>();
            msgVM.MatchupInvites = user.GetInviteUser(msgVM.User.userID, msgVM.Matchup, userCount);

            return View(msgVM);
        }

        [NoCacheAttribute]
        public ActionResult LoginRegister()
        {
            return View();
        }
    }
}
