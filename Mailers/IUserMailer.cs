using System.Collections.Generic;
using CoachCue.Model;
using Mvc.Mailer;

namespace CoachCue.Mailers
{ 
    public interface IUserMailer
    {
        MvcMailMessage Invite(string emailTo, string label, string inviteMessage);
        MvcMailMessage Notifications(string emailTo, string guid, string name, List<MatchupNotification> info);
        MvcMailMessage RequestVote(string emailTo, string guid, int fromUserID, LinkData link);
        MvcMailMessage UserMention(string emailTo, int fromUserID, string guid, string fromName, LinkData link);
        MvcMailMessage MatchupVoted(user toUser, int fromUserID, LinkData link);
        MvcMailMessage MatchupMessage(user toUser, int fromUserID, LinkData link);
        MvcMailMessage Follow(int userID, int followUserID);
    }
}