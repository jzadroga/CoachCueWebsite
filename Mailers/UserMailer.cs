using System.Collections.Generic;
using System.Net.Mail;
using CoachCue.Model;
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

        public virtual MvcMailMessage Notifications(Notification notification)
		{
            LinkData link = new LinkData()
            {
                Message = notification.Message.Text,
                ID = notification.Message.Id,
                Guid = notification.UserFrom.Id
            };

            UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string msgLink = helper.Action("Index", "Coach", new { mt = link.ID, id = notification.UserFrom.Id, name = notification.UserFrom.Name });
            msgLink = ConfigurationManager.AppSettings["MvcMailer.BaseURL"] + msgLink;

            ViewData["Title"] = notification.Text;
            ViewData["UserGuid"] = notification.UserFrom.Id;

            MailMentionViewModel voteVM = new MailMentionViewModel();
            voteVM.FullName = notification.UserFrom.Name;
            voteVM.MessageLink = link;
            voteVM.UserMessageLink = msgLink;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + notification.UserFrom.Profile.Image;
            ViewData.Model = voteVM;
            return Populate(x =>
            {
                x.Subject = notification.Text;
                x.ViewName = "MentionNotice";
                x.To.Add(notification.UserTo.Email);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }

        public virtual MvcMailMessage RequestVote(string emailTo, string guid, int fromUserID, LinkData link)
        {
            user fromUser = user.Get(fromUserID);
            string fromName = fromUser.fullName;

            ViewData["Title"] = fromName + " asked you to answer the matchup<br/>" + link.Message;
            ViewData["UserGuid"] = guid;

            MailRequestVoteViewModel voteVM = new MailRequestVoteViewModel();
            voteVM.FullName = fromName;
            voteVM.MatchupLink = link;
            voteVM.FullLink = ConfigurationManager.AppSettings["MvcMailer.BaseURL"] + "/matchup?mt=" + link.ID + "&gud=" + link.Guid;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + fromUser.avatar.imageName;
            ViewData.Model = voteVM;
            return Populate(x =>
            {
                x.Subject = fromName + " asked you to answer the matchup " + link.Message;
                x.ViewName = "RequestVote";
                x.To.Add(emailTo);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }

        public virtual MvcMailMessage Follow(int userID, int followUserID)
        {
            user fromUser = user.Get(userID);
            user followUser = user.Get(followUserID);
            string fromName = fromUser.fullName;

            ViewData["Title"] = followUser.fullName + ", you have a new follower on CoachCue.<br/>";
            ViewData["UserGuid"] = string.Empty;

            MailFollowViewModel followVM = new MailFollowViewModel();
            followVM.NewFollowName = fromName;
            followVM.NewFollowLink = ConfigurationManager.AppSettings["MvcMailer.BaseURL"] + "/coach/" + userID + "/" + fromUser.fullName;
            followVM.NewFollowAvatarSrc = "http://coachcue.com/assets/img/avatar/" + fromUser.avatar.imageName;
            ViewData.Model = followVM;
            return Populate(x =>
            {
                x.Subject = fromName + " is now following you on CoachCue!";
                x.ViewName = "NewFollow";
                x.To.Add(followUser.email);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }

        public virtual MvcMailMessage MatchupVoted(Notification notification)
        {
            string msg = notification.Matchup.Type + " ";
            for( int i=0; i < notification.Matchup.Players.Count; i++ )
            {
                if (notification.Matchup.Players.Count > 2 && i < notification.Matchup.Players.Count - 2)
                    msg += notification.Matchup.Players[i].Name + ", ";
                else
                    msg += ((i + 1) != notification.Matchup.Players.Count) ? notification.Matchup.Players[i].Name + " or " : notification.Matchup.Players[i].Name;
            }

            LinkData link = new LinkData()
            {
                Message = msg, 
                ID = notification.Matchup.Id,
                Guid = notification.UserFrom.Id
            };

            MailRequestVoteViewModel voteVM = new MailRequestVoteViewModel();
            voteVM.FullName = notification.UserFrom.Name;
            voteVM.MatchupLink = link;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + notification.UserFrom.Profile.Image;

            ViewData.Model = voteVM;

            ViewData["Title"] = notification.UserFrom.Name + " voted on your matchup<br/>";
            ViewData["UserGuid"] = string.Empty;

            return Populate(x =>
            {
                x.Subject = notification.UserFrom.Name + "  voted on your CoachCue matchup";
                x.ViewName = "MatchupVote";
                x.To.Add(notification.UserTo.Email);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }

        public virtual MvcMailMessage MatchupMessage(user toUser, int fromUserID, LinkData link)
        {
            user fromUser = user.Get(fromUserID);

            MailRequestVoteViewModel voteVM = new MailRequestVoteViewModel();
            voteVM.FullName = fromUser.fullName;
            voteVM.MatchupLink = link;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + fromUser.avatar.imageName;

            ViewData.Model = voteVM;

            ViewData["Title"] = fromUser.fullName + " commented on your matchup<br/>";
            ViewData["UserGuid"] = string.Empty;

            return Populate(x =>
            {
                x.Subject = fromUser.fullName + "  commented on your CoachCue matchup";
                x.ViewName = "MatchupVote";
                x.To.Add(toUser.email);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }

        public virtual MvcMailMessage UserMention(string emailTo, int fromUserID, string guid, string fromName, LinkData link)
        {
            UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string msgLink = helper.Action("Index", "Coach", new {mt = link.ID, id = fromUserID, name = fromName});
            msgLink = ConfigurationManager.AppSettings["MvcMailer.BaseURL"] + msgLink;

            ViewData["Title"] = fromName + ", mentioned you in a <a href='" + msgLink + "'>Message</a> on CoachCue<br/>";
            ViewData["UserGuid"] = guid;

            user fromUser = user.Get(fromUserID);

            MailMentionViewModel voteVM = new MailMentionViewModel();
            voteVM.FullName = fromName;
            voteVM.MessageLink = link;
            voteVM.UserMessageLink = msgLink;
            voteVM.FromAvatarSrc = "http://coachcue.com/assets/img/avatar/" + fromUser.avatar.imageName;
            ViewData.Model = voteVM;
            return Populate(x =>
            {
                x.Subject = fromName + "  mentioned you on CoachCue!";
                x.ViewName = "MentionNotice";
                x.To.Add(emailTo);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }
 	}
}