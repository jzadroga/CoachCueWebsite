using System.Collections.Generic;
using System.Net.Mail;
using CoachCue.ViewModels;
using Mvc.Mailer;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using CoachCue.Models;
using CoachCue.Service;

namespace CoachCue.Mailers
{ 
    public class UserMailer : MailerBase, IUserMailer 	
	{
        private string fromAddress = "info@coachcue.com";
		
        public UserMailer()
		{
			MasterName="_Layout";
		}
		
		public virtual MvcMailMessage Invite(string emailTo, string label, string inviteMessage)
		{
			ViewData["Title"] = "Join me at CoachCue.";
            ViewData["InviteLink"] = "http://coachcue.com?gu=xyyyy-567-0gtr";
            ViewData["fullName"] = label;
            ViewData["InviteMessage"] = inviteMessage;
			return Populate(x =>
			{
                x.Subject = "You have been invited to join CoachCue!";
				x.ViewName = "Invite";
                x.To.Add(emailTo);
                x.From = new MailAddress(fromAddress, label);
			});
		}

        public virtual MvcMailMessage Notifications(Notification notification, User userFrom, User userTo, Message message)
		{
            LinkData link = new LinkData()
            {
                Message = message.Text,
                ID = notification.Message,
                Guid = notification.UserFrom
            };

            UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string msgLink = helper.Action("Index", "Coach", new { name = userFrom.Link });
            msgLink = ConfigurationManager.AppSettings["MvcMailer.BaseURL"] + msgLink;

            ViewData["Title"] = notification.Text;
            ViewData["UserGuid"] = notification.UserFrom;

            MailMentionViewModel voteVM = new MailMentionViewModel();
            voteVM.FullName = userFrom.Name;
            voteVM.MessageLink = link;
            voteVM.UserMessageLink = msgLink;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + userFrom.Profile.Image;
            ViewData.Model = voteVM;
            return Populate(x =>
            {
                x.Subject = notification.Text;
                x.ViewName = "MentionNotice";
                x.To.Add(userTo.Email);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }

        public virtual MvcMailMessage MatchupNotifications(Notification notification, User userFrom, User userTo, Matchup matchup)
        {
            string msg = matchup.Type + " ";
            for (int i = 0; i < matchup.Players.Count; i++)
            {
                if (matchup.Players.Count > 2 && i < matchup.Players.Count - 2)
                    msg += matchup.Players[i].Name + ", ";
                else
                    msg += ((i + 1) != matchup.Players.Count) ? matchup.Players[i].Name + " or " : matchup.Players[i].Name;
            }

            LinkData link = new LinkData()
            {
                Message = msg,
                ID = notification.Matchup,
                Guid = notification.UserFrom
            };

            MailRequestVoteViewModel voteVM = new MailRequestVoteViewModel();
            voteVM.FullName = userFrom.Name;
            voteVM.MatchupLink = link;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + userFrom.Profile.Image;
            voteVM.FullLink = "http://coachcue.com/" + matchup.Link;

            ViewData.Model = voteVM;

            ViewData["Title"] = notification.Text;
            ViewData["UserGuid"] = string.Empty;

            return Populate(x =>
            {
                x.Subject = notification.Text;
                x.ViewName = "MatchupVote";
                x.To.Add(userTo.Email);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }

        public virtual MvcMailMessage RequestVote(Notification notification, User userTo, User userFrom, Matchup matchup)
        {
            ViewData["Title"] = notification.Text;
            ViewData["UserGuid"] = notification.UserFrom;

            string msg = matchup.Type + " ";
            for (int i = 0; i < matchup.Players.Count; i++)
            {
                if (matchup.Players.Count > 2 && i < matchup.Players.Count - 2)
                    msg += matchup.Players[i].Name + ", ";
                else
                    msg += ((i + 1) != matchup.Players.Count) ? matchup.Players[i].Name + " or " : matchup.Players[i].Name;
            }

            LinkData link = new LinkData()
            {
                Message = msg,
                ID = notification.Matchup,
                Guid = notification.UserFrom
            };

            MailRequestVoteViewModel voteVM = new MailRequestVoteViewModel();
            voteVM.FullName = userFrom.Name;
            voteVM.MatchupLink = link;
            voteVM.FullLink = ConfigurationManager.AppSettings["MvcMailer.BaseURL"] + "/" + matchup.Link;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + userFrom.Profile.Image;
            ViewData.Model = voteVM;
            return Populate(x =>
            {
                x.Subject = notification.Text;
                x.ViewName = "RequestVote";
                x.To.Add(userTo.Email);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }

        public virtual MvcMailMessage MatchupVoted(Notification notification, User userTo, User userFrom, Matchup matchup)
        {
            string msg = matchup.Type + " ";
            for( int i=0; i < matchup.Players.Count; i++ )
            {
                if (matchup.Players.Count > 2 && i < matchup.Players.Count - 2)
                    msg += matchup.Players[i].Name + ", ";
                else
                    msg += ((i + 1) != matchup.Players.Count) ? matchup.Players[i].Name + " or " : matchup.Players[i].Name;
            }

            LinkData link = new LinkData()
            {
                Message = msg, 
                ID = notification.Matchup,
                Guid = notification.UserFrom
            };

            MailRequestVoteViewModel voteVM = new MailRequestVoteViewModel();
            voteVM.FullName = userFrom.Name;
            voteVM.MatchupLink = link;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + userFrom.Profile.Image;
            voteVM.FullLink = "http://coachcue.com/" + matchup.Link;

            ViewData.Model = voteVM;

            ViewData["Title"] = userFrom.Name + " voted on your matchup<br/>";
            ViewData["UserGuid"] = string.Empty;

            return Populate(x =>
            {
                x.Subject = userFrom.Name + "  voted on your CoachCue matchup";
                x.ViewName = "MatchupVote";
                x.To.Add(userTo.Email);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }
 	}
}