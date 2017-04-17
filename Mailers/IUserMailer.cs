using System.Collections.Generic;
using Mvc.Mailer;
using CoachCue.Models;
using CoachCue.Service;

namespace CoachCue.Mailers
{ 
    public interface IUserMailer
    {
        MvcMailMessage Invite(string emailTo, string label, string inviteMessage);
        MvcMailMessage Notifications(Notification notification, User userFrom, User userTo, Message message);
        MvcMailMessage RequestVote(Notification notification, User userTo, User userFrom, Matchup matchup);
        MvcMailMessage MatchupVoted(Notification notification, User userTo, User userFrom, Matchup matchup);
        MvcMailMessage MatchupNotifications(Notification notification, User userFrom, User userTo, Matchup matchup);
    }
}