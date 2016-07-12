using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoachCue.Model
{
    public partial class notification 
    {
        public user noticeUser
        {
            get
            {
                user userItem = new user();
                switch (this.notification_type.type)
                {
                    case "voteMatchup":
                        userItem = this.user;
                        break;
                    case "voteRequested":
                        userItem = (this.user.userID == user.GetUserID(HttpContext.Current.User.Identity.Name)) ? this.user : this.user1;                            
                        break;
                    case "messageMention":
                        userItem = this.user;
                        break;
                }

                return user;
            }
        }

        public LinkData linkData
        {
            get
            {
                LinkData link = new LinkData();

                switch (this.notification_type.type)
                {
                    case "voteMatchup":
                        //get the sentTo users matchups and the sentFrom user_matchups than match the matchup ids to get the matchup
                        List<int> matchups = this.user1.matchups.Where( mt => mt.createdBy == this.sentTo ).Select( mt => mt.matchupID).ToList();
                        users_matchup userMatch = this.user.users_matchups.Where(usm => matchups.Contains(usm.matchupID) && usm.dateCreated == this.dateCreated).FirstOrDefault();
                        if (userMatch != null)
                        {
                            link.Message = userMatch.matchup.nflplayer.fullName + " vs " + userMatch.matchup.nflplayer1.fullName;
                            link.ID = userMatch.matchupID;
                        }

                        break;
                    case "voteRequested":
                        link = GetMatchupLink((int)this.entityID, this.notificationGUID);
                        break;
                }

                return link;
            }
        }

        public string noticeMessage
        {
            get
            {
                string msg = string.Empty;

                switch (this.notification_type.type)
                {
                    case "voteMatchup":
                        msg = " has voted on your matchup ";
                        break;
                    case "voteRequested":
                        msg = (this.user.userID == user.GetUserID(HttpContext.Current.User.Identity.Name)) ? " was invited to vote on your matchup " : " invited you to vote on the matchup ";
                        break;
                    case "messageMention":
                        msg = " mentioned you ";
                        break;
                }

                return msg;
            }
        }

        public static LinkData GetMatchupLink(int matchupID, string guid)
        {
            LinkData link = new LinkData();

            matchup matchupItem = matchup.Get(matchupID);
            if (matchupItem != null)
            {
                if (matchupItem.matchupID != 0)
                {
                    link.Message = matchupItem.nflplayer.fullName + " vs " + matchupItem.nflplayer1.fullName;
                    link.ID = matchupItem.matchupID;
                    link.Guid = guid;
                }
            }

            return link;
        }

        public static notification GetByGuid(string guid)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            notification notice = new notification();

            try
            {
                var not = from mt in db.notifications
                          where mt.notificationGUID == guid
                          select mt;

                if (not.Count() > 0)
                    notice = not.FirstOrDefault();
            }
            catch (Exception) { }

            return notice;
        }

        public static notification Add(string type, int entityID, int from, int to, DateTime dateCreated)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            notification notice = new notification();
            try
            {
                //don't add if its from and to the same user
                if (from != to)
                {
                    int typeID = GetTypeID(type);

                    //now make sure the notification does not already exist before adding
                    notification noticeCheck = db.notifications.Where(not => not.status.statusName == "Active" && not.entityID == entityID && not.sentFrom == from && not.sentTo == to && not.typeID == typeID).FirstOrDefault();
                    if (noticeCheck == null)
                    {
                        notice.dateCreated = dateCreated;
                        notice.sentFrom = from;
                        notice.sentTo = to;
                        notice.notificationGUID = Guid.NewGuid().ToString();
                        notice.statusID = status.GetID("Active", "notifications");
                        notice.typeID = typeID;
                        notice.entityID = entityID;

                        db.notifications.InsertOnSubmit(notice);
                        db.SubmitChanges();

                        //send the invite email
                       // if (type == "voteRequested")
                          //  EmailHelper.SendVoteRequestEmail(from, to, entityID, notice.notificationGUID);
                    }
                }
            }
            catch (Exception ex) 
            { 
                string s = ex.Message; 
            }

            return notice;
        }

        //returns a list of users who have been asked to vote on a matchup
        public static List<UserVoteData> GetInvitedToAnswer(int matchupID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<UserVoteData> users = new List <UserVoteData>();

            try
            {
                List<notification> notices = GetByMatchupID(matchupID).Where(notice => notice.notification_type.type == "voteRequested").ToList();

                //now see if user has responded yet
                foreach( notification notice in notices )
                {
                    notification voted = db.notifications.Where(not => not.entityID == notice.entityID && not.sentTo == notice.sentFrom && not.sentFrom == notice.sentTo).FirstOrDefault();
                    if( voted == null )
                    {
                        user userItem = user.Get( notice.sentTo );
                        users.Add(new UserVoteData
                        {
                            email = userItem.email,
                            fullName = userItem.fullName,
                            correctPercentage = (userItem.CorrectPercentage != 0) ? userItem.CorrectPercentage + "%" : string.Empty,
                            profileImg = "/assets/img/avatar/" + userItem.avatar.imageName,
                            userID = userItem.userID,
                            username = userItem.userName
                        });
                    }
                }
            }
            catch (Exception) { }

            return users;
        }

        public static List<notification> GetByMatchupID(int matchupID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<notification> notifications = new List<notification>();

            try
            {
                var not = from mt in db.notifications
                          where mt.entityID == matchupID && mt.status.statusName == "Active"
                          orderby mt.dateCreated descending
                          select mt;

                if (not.Count() > 0)
                    notifications = not.ToList();
            }
            catch (Exception) { }

            return notifications;
        }

        public static List<notification> GetByUserID(int userID, bool changeViewed)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<notification> notifications = new List<notification>();

            try
            {
                var not = from mt in db.notifications
                          where mt.sentTo == userID && mt.status.statusName != "Deleted" orderby mt.dateCreated descending
                          select mt;

                if (not.Count() > 0)
                {
                    notifications = not.ToList();
                    if (changeViewed)
                    {
                        foreach (notification notice in notifications)
                        {
                            if (notice.status.statusName != "Viewed")
                            {
                                CoachCueDataContext dbUpdate = new CoachCueDataContext();
                                notification updateNotice = dbUpdate.notifications.Where(nt => nt.notificationID == notice.notificationID).FirstOrDefault();
                                updateNotice.statusID = status.GetID("Viewed", "notifications");
                                updateNotice.dateUpdated = DateTime.Now;
                                dbUpdate.SubmitChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { string mes = ex.Message; }

            return notifications;
        }

        public static int GetTypeID(string type)
        {
            int typeID = 0;
            CoachCueDataContext db = new CoachCueDataContext();
            
            try
            {
                var notType = db.notification_types.Where(nt => nt.type.ToLower() == type.ToLower()).FirstOrDefault();
                if (notType != null)
                    typeID = notType.notificationTypeID;

            }
            catch (Exception) { }

            return typeID;
        }
    }

    public class LinkData
    {
        public string Message { get; set; }
        public int ID { get; set;}
        public string Guid { get; set; }
    }

    public class MatchupNotification
    {
        public int ID { get; set; }
        public int VoteCount { get; set; }
        public string Match { get; set; }
    }
}