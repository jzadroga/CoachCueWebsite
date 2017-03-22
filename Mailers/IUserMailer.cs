using System.Collections.Generic;
using CoachCue.Model;
using Mvc.Mailer;
using CoachCue.Models;
using CoachCue.Service;

namespace CoachCue.Mailers
{ 
    public interface IUserMailer
    {
        MvcMailMessage Invite(string emailTo, string label, string inviteMessage);
        MvcMailMessage Notifications(Notification notification);
        MvcMailMessage RequestVote(Notification notification);
        MvcMailMessage MatchupVoted(Notification notification);
        MvcMailMessage MatchupMessage(user toUser, int fromUserID, LinkData link);
    }
}