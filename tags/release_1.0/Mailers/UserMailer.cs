using System.Collections.Generic;
using System.Net.Mail;
using CoachCue.Model;
using CoachCue.ViewModels;
using Mvc.Mailer;

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

        public virtual MvcMailMessage Notifications(string emailTo, string guid, string name, List<MatchupNotification> info)
		{
            ViewData["Title"] = name + ", you have some notifications";
            ViewData["UserGuid"] = guid;

            MailNotificationsViewModel notVM = new MailNotificationsViewModel();
            notVM.FullName = name;
            notVM.Notices = info;
            ViewData.Model = notVM;
			return Populate(x =>
			{
                x.Subject = "Here's some activity you may have missed on CoachCue";
				x.ViewName = "Notifications";
                x.To.Add(emailTo);
                x.From = new MailAddress(fromAddress, "CoachCue");
			});
		}

        public virtual MvcMailMessage RequestVote(string emailTo, string guid, string fromName, LinkData link)
        {
            ViewData["Title"] = fromName + " asked you to answer the matchup<br/>" + link.Message;
            ViewData["UserGuid"] = guid;

            MailRequestVoteViewModel voteVM = new MailRequestVoteViewModel();
            voteVM.FullName = fromName;
            voteVM.MatchupLink = link;
            ViewData.Model = voteVM;
            return Populate(x =>
            {
                x.Subject = fromName + " asked you to answer the matchup " + link.Message;
                x.ViewName = "RequestVote";
                x.To.Add(emailTo);
                x.From = new MailAddress(fromAddress, "CoachCue");
            });
        }
 	}
}