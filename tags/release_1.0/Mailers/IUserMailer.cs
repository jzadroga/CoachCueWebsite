using System.Collections.Generic;
using CoachCue.Model;
using Mvc.Mailer;

namespace CoachCue.Mailers
{ 
    public interface IUserMailer
    {
        MvcMailMessage Invite(string emailTo, string label, string inviteMessage);
        MvcMailMessage Notifications(string emailTo, string guid, string name, List<MatchupNotification> info);
        MvcMailMessage RequestVote(string emailTo, string guid, string fromName, LinkData link);
	}
}