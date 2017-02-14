using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using CoachCue;
using System.Configuration;
using CoachCue.Service;

namespace CoachCue.Model
{
    public partial class message
    {
        private const string _messageCacheID = "generalMessagesCache";

        public string CssClass { get; set; }

        public int ParentID
        {
            get
            {
                int parentID = 0;

                if (this.messageContextID.HasValue)
                    parentID = this.messageContextID.Value;

                return parentID;
            }
        }

        public bool IsParent { get; set; }
        public bool IsReply 
        {
            get
            {
                return (this.messageContextID.HasValue) ? true : false;
            }
        }

        public string UserReplies
        {
            get
            {
                string replies = string.Empty;
                int userID = user.GetUserID(HttpContext.Current.User.Identity.Name);

                //get any users mentioned plus the person who posted the message
                if (this.message_users != null)
                {
                    if (this.message_users.Count() > 0)
                    {
                        foreach (message_user replyUser in this.message_users.ToList())
                        {
                            if (replyUser.userID != userID)  //don't add the user if its the same person
                                replies += ( string.IsNullOrEmpty( replies )) ? "@" + replyUser.user.userName : " @" + replyUser.user.userName;
                        }
                    }
                }

                if (this.user != null)
                {
                    if (!string.IsNullOrEmpty(this.user.userName))
                    {
                        if (this.user.userID != userID)  //don't add the user if its the same person
                            replies += (string.IsNullOrEmpty(replies)) ? "@" + this.user.userName : " @" + this.user.userName;
                    }
                }

                if (!string.IsNullOrEmpty(replies))
                    replies = replies + " ";

                return replies;
            }
        }

        public string MessagePlayerIDs
        {
            get
            {
                string playerIDs = string.Empty;

                if (this.message_players != null)
                {
                    if (this.message_players.Count() > 0)
                    {
                        foreach (message_player msgPlayer in this.message_players.ToList())
                        {
                            playerIDs += (string.IsNullOrEmpty(playerIDs)) ? msgPlayer.playerID.ToString() : "," + msgPlayer.playerID.ToString(); 
                        }
                    }
                }

                return playerIDs;
            }
        }

        public List<nflplayer> MessagePlayers
        {
            get
            {
                List<nflplayer> players = new List<nflplayer>();

                if (this.message_players != null)
                {
                    if (this.message_players.Count() > 0)
                    {
                        foreach (message_player msgPlayer in this.message_players.ToList())
                        {
                            players.Add(msgPlayer.nflplayer);                        
                        }
                    }
                }

                return players;
            }
        }

        public user MessageUser
        {
            get
            {
                return user.Get(this.userID);
            }
        }

        public string TimeAgo
        {
            get
            {
                return twitter.GetRelativeTime(this.dateCreated);
            }
        }

        private List<message> conversationMessages = new List<message>();
        public List<message> ConversationMessages
        {
            get
            {
                return conversationMessages;
            }
            set
            {
                if(value != null)
                {
                    conversationMessages = value;
                }
            }
        }

        public static bool ShowPlayerHeader( List<StreamContent> stream, int? messageContextID ){
            bool show = true;

            if (messageContextID.HasValue)
            {
                foreach (StreamContent streamItem in stream)
                {
                    //if (streamItem.MessageItem.messageID == messageContextID)
                        show = false;
                }
            }

            return show;
        }

        public static SavedMessage Save(int userID, string playerIDs, string message, string type, int? parentID = null)
        {
            message msg = new message();
            SavedMessage savedMsg = new SavedMessage();

           /* try
            {
                //clear the message cache
                HttpContext.Current.Cache.Remove(_messageCacheID);

                CoachCueDataContext db = new CoachCueDataContext();

                if (string.IsNullOrEmpty(message)) //don't save if too big or empty
                    return savedMsg;

                FormattedMessage messageItem = FormatMessage(message);
                msg.userID = userID;
                msg.messageText = messageItem.messageText;
                msg.messageContextTypeID = messagecontexttype.GetID(type);
                
                //if a reply set the parent ID
                if (messageItem.userIncluded || parentID != null)
                    msg.messageContextID = parentID;

                DateTime messageCreated = DateTime.UtcNow.GetEasternTime();
                msg.dateCreated = messageCreated;

                //check for opengraph info
                if( messageItem.openGraph.IsOpenGraph )
                {
                    msg.mediaTitle = messageItem.openGraph.Title;
                    msg.mediaUrl = messageItem.openGraph.URL;
                    if (messageItem.openGraph.MediaTypeID == 1)
                    {
                        msg.mediaTypeID = messageItem.openGraph.MediaTypeID;
                        msg.mediaObjectUrl = messageItem.openGraph.Video;
                    }
                    else if (messageItem.openGraph.MediaTypeID == 2)
                    {
                        msg.mediaTypeID = messageItem.openGraph.MediaTypeID;
                        msg.mediaObjectUrl = messageItem.openGraph.Image;
                    }
                }

                db.messages.InsertOnSubmit(msg);
                db.SubmitChanges();

                //add any player mentions
                foreach (string playerID in playerIDs.Split(',').ToList())
                {
                    if (!string.IsNullOrEmpty(playerID))
                    {
                         msg.attach_message_players( AddPlayerMention(msg.messageID, Convert.ToInt32(playerID) ));                   
                    }
                }

                //add a notifications and user mention joins
                if (messageItem.userIncluded)
                {
                    foreach (user mention in messageItem.userList)
                    {
                        AddUserMention(msg.messageID, mention.userID);
                        notification mentionNotice = notification.Add("messageMention", msg.messageID, userID, mention.userID, messageCreated);
                        savedMsg.MentionNotices.Add(new MentionNotice { fromUser = userID, toUser = mention.userID, messageID = msg.messageID, noticeGuid = mentionNotice.notificationGUID });
                    }
                }

                //also add notification if its a message about the user created matchup
                if (type == "matchup" && parentID.HasValue )
                {
                    matchup matchupItem = matchup.Get(parentID.Value);
                    notification matchupNotice = notification.Add("matchupMessage", matchupItem.matchupID, userID, matchupItem.createdBy, messageCreated);
                    savedMsg.MentionNotices.Add(new MentionNotice { fromUser = userID, toUser = matchupItem.createdBy, messageID = matchupItem.matchupID, noticeGuid = matchupNotice.notificationGUID });
                }

                savedMsg.UserMessage = msg;
            }
            catch (Exception) { }
            */
            return savedMsg;
        }

        public static message_player AddPlayerMention(int messageID, int playerID)
        {
            message_player mention = new message_player();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                mention.playerID = playerID;
                mention.messageID = messageID;

                db.message_players.InsertOnSubmit(mention);
                db.SubmitChanges();
            }
            catch (Exception)
            {
            }

            return mention;
        }
        
        public static void AddUserMention( int messageID, int userID ) 
        {
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                message_user mention = new message_user();

                mention.userID = userID;
                mention.messageID = messageID;

                db.message_users.InsertOnSubmit(mention);
                db.SubmitChanges();
            }
            catch (Exception)
            {
            }
        }

        public static List<int> DirectMessages(int userID)
        {
            List<int> playerIDs = new List<int>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
            }
            catch(Exception){}

            return playerIDs;
        }

        public static List<message> GetRecentList(int playerID,  bool futureTimeline, DateTime? fromDate = null)
        {
            List<message> msgs = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = from msg in db.messages
                               join playermsgs in db.message_players on
                               msg.messageID equals playermsgs.messageID
                               where playermsgs.playerID == playerID && !msg.messageContextID.HasValue
                               select msg;
                                
                if (fromDate.HasValue)
                    msgQuery = (futureTimeline) ? msgQuery.Where(msg => msg.dateCreated > fromDate) : msgQuery.Where(msg => msg.dateCreated < fromDate);

                //only grab the top 50 messages
                List<message> messageList = msgQuery.OrderByDescending(msg => msg.dateCreated).Take(50).ToList();
                if (messageList.Count() > 0)
                    msgs = messageList;
            }
            catch (Exception) { }

            return msgs;
        }

        public static message Get(int messageID)
        {
            message msg = new message();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where(ms => ms.messageID == messageID);
                if (msgQuery.Count() > 0)
                    msg = msgQuery.FirstOrDefault();
            }
            catch (Exception) { }

            return msg;
        }

        public static List<message> GetConversation(int id, int parentID, string type)
        {
            List<message> msgs = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                switch (type)
                {
                    case "general":

                        if (parentID != 0)
                        {
                            //if message is not the top parent but somewhere in the chain get all the messages above it in the chain

                            //first get the current message and the parent
                            message currentMsg = message.Get(id);
                            msgs.Add(currentMsg);
                            message parentMsg = message.Get(parentID);
                            msgs.Add(parentMsg);

                            //now continue up the chain till contextID is null
                            while (parentMsg.messageContextID.HasValue)
                            {
                                parentMsg = message.Get(parentMsg.messageContextID.Value);
                                msgs.Add(parentMsg);
                            }
                        }
                        else
                        {
                            //top message so get all the children in the chain
                            message childMsg = message.GetChild(id);
                            while ( childMsg.messageID != 0 )
                            {
                                msgs.Add(childMsg);
                                childMsg = message.GetChild(childMsg.messageID);
                            }
                        }
                        
                        if (msgs.Count() > 0)
                        {
                            msgs = msgs.OrderBy(msg => msg.dateCreated).ToList();
                            if (parentID != 0)
                                msgs[0].CssClass = "first-message";
                            
                            msgs[msgs.Count - 1].CssClass = "last-message";
                        }

                        break;
                    case "matchup":
                        var matchupMsgs = db.messages.Where(msg => msg.messageContextID == id && msg.messagecontexttype.messageContextTypeName == type);
                        if (matchupMsgs.Count() > 0)
                            msgs = matchupMsgs.OrderBy(msg => msg.dateCreated).ToList();

                        break;
                }
            }
            catch (Exception) { }

            return msgs;
        }

        public static List<message> GetByParents(List<int> messageIDList, string type)
        {
            List<message> msgs = new List<message>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var allMsgs = db.messages.Where(msg => messageIDList.Contains( msg.messageContextID.Value ) && msg.messagecontexttype.messageContextTypeName == type);
                if (allMsgs.Count() > 0)
                    msgs = allMsgs.ToList();

            }
            catch (Exception) { }

            return msgs;
        }

        public static message GetParent(int messageContextID)
        {
            message parentMsg = new message();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where(msg => msg.messageID == messageContextID);
                if (msgQuery.Count() > 0)
                    parentMsg = msgQuery.FirstOrDefault();

            }
            catch (Exception) { }

            return parentMsg;
        }

        public static message GetChild(int messageID)
        {
            message childMsg = new message();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where(msg => msg.messageContextID == messageID);
                if (msgQuery.Count() > 0)
                    childMsg = msgQuery.FirstOrDefault();

            }
            catch (Exception) { }

            return childMsg;
        }

        public static List<message> GetDetails(int messageID)
        {
            List<message> msgs = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where(msg => msg.messageID == messageID );
                if (msgQuery.Count() > 0)
                {
                    message msgItem = msgQuery.FirstOrDefault();
                    if (msgItem.messageContextID.HasValue)
                    {
                        //also get the parent
                        message msgParent = db.messages.Where(msg => msg.messageID == msgItem.messageContextID).FirstOrDefault();
                        msgParent.IsParent = true;
                        msgs.Add(msgParent);
                    }

                    msgs.Add(msgItem);
                    msgs = msgs.OrderBy(msg => msg.dateCreated).ToList();
                }
            }
            catch (Exception) { }

            return msgs;
        }

        public static List<message> GetAll(DateTime? fromDate = null)
        {
            List<message> msgs = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where( msg => msg.messageText.Length > 0 );
                if (fromDate.HasValue)
                    msgQuery = msgQuery.Where(msg => msg.dateCreated > fromDate);

                //only grab the top 50 messages
                List<message> messageList = msgQuery.OrderByDescending(msg => msg.dateCreated).Take(250).ToList();
                if (messageList.Count() > 0)
                    msgs = messageList;
            }
            catch (Exception) { }

            return msgs;
        }

        public static List<message> GetUserRecentList(int userID, DateTime? fromDate = null)
        {
            List<message> msgs = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where(msg => msg.messageContextID.HasValue == false && msg.messagecontexttype.messageContextTypeName == "general" && msg.userID == userID );
                if (fromDate.HasValue)
                    msgQuery = msgQuery.Where(msg => msg.dateCreated > fromDate);

                //only grab the top 50 messages
                List<message> messageList = msgQuery.OrderByDescending(msg => msg.dateCreated).Take(25).ToList();
                if (messageList.Count() > 0)
                    msgs = messageList;

                foreach (message msgItem in msgs)
                {
                    msgItem.ConversationMessages = new List<message>();
                    var convo = db.messages.Where(msg => msg.messageContextID.Value == msgItem.messageID && msg.messagecontexttype.messageContextTypeName == "general").OrderBy(msg => msg.dateCreated);
                    if (convo.Count() > 0)
                        msgItem.ConversationMessages = convo.ToList();
                }
            }
            catch (Exception) { }

            return msgs;
        }

        public static List<message> GetRecentList( bool futureTimeline, DateTime fromDate )
        {
            List<message> messages = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                if (HttpContext.Current.Cache[_messageCacheID] != null)
                    messages = (List<message>)HttpContext.Current.Cache[_messageCacheID];
                else
                {
                    List<int> messageIDs = new List<int>();

                    //don't filter messages
                    var msgQueryAll = from msg in db.messages
                                      where msg.messagecontexttype.messageContextTypeName == "general"
                                      select msg;

                    if (msgQueryAll.Count() > 0)
                    {
                        var msgQueryDate = (futureTimeline) ? msgQueryAll.Where(msgs => msgs.dateCreated >= fromDate) : msgQueryAll.Where(msgs => msgs.dateCreated < fromDate);
                        messages = msgQueryDate.Where(msg => msg.messageContextID.HasValue == false).OrderByDescending(msg => msg.dateCreated).Take(20).ToList();
                        foreach (message msgItem in messages)
                        {
                            msgItem.ConversationMessages = new List<message>();
                            var convo = msgQueryAll.Where(msg => msg.messageContextID.Value == msgItem.messageID).OrderBy(msg => msg.dateCreated);
                            if (convo.Count() > 0)
                                msgItem.ConversationMessages = convo.ToList();
                        }
                    }

                    HttpContext.Current.Cache.Insert(_messageCacheID, messages, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(24, 0, 0));
                }
            }
            catch( Exception){}

            return messages;
        }
    }

    public partial class messagecontexttype
    {
        public static int GetID(string contextName)
        {
            int contextTypeID = 0;

            CoachCueDataContext db = new CoachCueDataContext();
            var contype = db.messagecontexttypes.Where(type => type.messageContextTypeName.ToLower() == contextName.ToLower());

            if (contype.Count() > 0)
                contextTypeID = contype.FirstOrDefault().messageContextTypeID;

            return contextTypeID;
        }

    }

    public class MentionNotice
    {
        public int fromUser { get; set; }
        public int toUser { get; set; }
        public int messageID { get; set; }
        public string noticeGuid { get; set; }
    }

    public class SavedMessage
    {
        public message UserMessage { get; set; }
        public List<MentionNotice> MentionNotices { get; set; }
    
        public SavedMessage()
        {
            this.UserMessage = new message();
            this.MentionNotices = new List<MentionNotice>();
        }
    }
}
