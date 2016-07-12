using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CoachCue.Model
{
    public partial class message
    {
        public bool IsParent { get; set; }
        public bool IsReply 
        {
            get
            {
                return (this.messageContextID.HasValue) ? true : false;
            }
        }

        public bool HasConversation
        {
            get
            {
                bool hasConversaton = false;

                try
                {
                    CoachCueDataContext db = new CoachCueDataContext();
                    var replies = db.messages.Where(msg => msg.messageContextID == this.messageID);
                    if (replies.Count() > 0)
                        hasConversaton = true;
                }
                catch (Exception) { }

                return hasConversaton;
            }
        }

        public user MessageUser
        {
            get
            {
                return user.Get(this.userID);
            }
        }

        public static bool ShowPlayerHeader( List<StreamContent> stream, int? messageContextID ){
            bool show = true;

            if (messageContextID.HasValue)
            {
                foreach (StreamContent streamItem in stream)
                {
                    if (streamItem.MessageItem.messageID == messageContextID)
                        show = false;
                }
            }

            return show;
        }

        public static message Save(int userID, int playerID, string message, int? parentID = null )
        {
            message msg = new message();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                if (message.Count() > 140) //don't save if too big
                    return msg;

                FormattedMessage messageItem = FormatMessage(message);
                msg.userID = userID;
                msg.messageText = messageItem.messageText;
                msg.playerID = playerID;

                msg.messageContextTypeID = ( messageItem.userIncluded ) ? messagecontexttype.GetID("users") : messagecontexttype.GetID("general");
                if (messageItem.userIncluded || parentID != null)
                    msg.messageContextID = parentID;
                
                DateTime messageCreated = DateTime.Now;
                msg.dateCreated = messageCreated;

                db.messages.InsertOnSubmit(msg);
                db.SubmitChanges();

                //also add a notification here///
                if (messageItem.userIncluded)
                    notification.Add("messageMention", msg.messageID, userID, messageItem.userReference.userID, messageCreated);
            }
            catch (Exception) { }

            return msg;
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

        public static List<message> GetRecentList(int playerID, DateTime? fromDate = null)
        {
            List<message> msgs = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where(msg => msg.playerID == playerID && !msg.messageContextID.HasValue );
                if (fromDate.HasValue)
                    msgQuery = msgQuery.Where(msg => msg.dateCreated > fromDate);

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

        public static List<message> GetConversation(int messageID)
        {
            List<message> msgs = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where(msg => msg.messageContextID == messageID);
                if (msgQuery.Count() > 0)
                    msgs = msgQuery.OrderByDescending(msg => msg.dateCreated).ToList();
            }
            catch (Exception) { }

            return msgs;
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
                var msgQuery = db.messages.Where(msg => msg.messageContextID == userID || msg.userID == userID );
                if (fromDate.HasValue)
                    msgQuery = msgQuery.Where(msg => msg.dateCreated > fromDate);

                //only grab the top 50 messages
                List<message> messageList = msgQuery.OrderByDescending(msg => msg.dateCreated).Take(50).ToList();
                if (messageList.Count() > 0)
                    msgs = messageList;
            }
            catch (Exception) { }

            return msgs;
        }

        public static List<message> GetRecentList( int userID,  List<int> followIDs, DateTime? fromDate = null )
        {
            List<message> msgs = new List<message>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var msgQuery = db.messages.Where(msg => msg.messageContextID == userID || msg.userID == userID || followIDs.Contains(msg.userID));
                if (fromDate.HasValue)
                    msgQuery = msgQuery.Where(msg => msg.dateCreated > fromDate);

                //only grab the top 50 messages
                List<message> messageList =  msgQuery.OrderByDescending( msg => msg.dateCreated).Take(50).ToList();
                if (messageList.Count() > 0)
                    msgs = messageList;
            }
            catch( Exception){}
            
            return msgs;
        }

        public static List<FollowData> GetRecentMessagePlayerList(int userID)
        {
            List<FollowData> players = new List<FollowData>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                List<int> followIDs = user.GetFollowUsers(userID);
                foreach (int followID in followIDs)
                {
                    //check for messages in the last two days
                    List<message> msgs = GetUserRecentList(followID, DateTime.Now.AddDays(-2));
                    if (msgs.Count() > 0)
                    {
                        //get distinct list of messages by player
                        var distinctPlyrs = msgs.Select(msg => new { msg.playerID }).Distinct();
                        foreach( var playerID in distinctPlyrs.ToList() )
                        {
                            //now get the player stream and check if the user message is in the top 5
                            int player = playerID.playerID;
                            List<StreamContent> streamItems = stream.GetPlayerStream(player, userID, true);
                            if (streamItems.Count() > 0)
                            {
                                StreamContent lastItem = streamItems.Where(st => st.ContentType != "empty-news" && st.ContentType != "empty-matchup" && st.ContentType != "empty-messages").LastOrDefault();
                                if (lastItem != null)
                                {
                                    List<message> playerMsgs = msgs.Where(msg => msg.playerID == player && msg.dateCreated > lastItem.DateCreated).ToList();
                                    if (playerMsgs.Count() > 0)
                                        players.Add(new FollowData { player = nflplayer.Get(player) });
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception) { }

            return players;
        }

        public static FormattedMessage FormatMessage(string txt)
        {
            FormattedMessage messageItem = new FormattedMessage();
            //spilt the message into words
            List<string> words = txt.Split(' ').ToList();
            List<string> domains = new string[]{".com", ".org", ".net", ".mil", ".edu"}.ToList();
            string message = string.Empty;

            foreach (string word in words)
            {
                //check for link format
                if (domains.Any(w => word.ToLower().Contains(w)))
                    message += (word.ToLower().StartsWith("http://")) ? word : "http://" + word;
                else if (word.StartsWith("@")) //check for user
                {
                    user userItem = user.GetByAccountUsername(word.Substring(1));
                    if (userItem != null)
                    {
                        messageItem.userIncluded = true;
                        messageItem.userReference = userItem;
                        if (userItem.userID != 0)
                            message += "<a href='/coach/" + userItem.userID + "/" + userItem.fullName + "'>" + word + "</a>";
                    }
                }
                else
                    message += word;

                message += " ";
            }

            message = message.Trim();

            //build urls if links are inlcuded
            Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            MatchCollection mactches = regx.Matches(message);

            foreach (Match match in mactches)
            {
                string googleUrl = GoogleAPI.GetShortUrl(match.Value);

                string showUrl = (match.Value.Length > 25 ) ? googleUrl : match.Value;
                message = message.Replace(match.Value, "<a target='_blank' href='" + googleUrl + "'>" + showUrl + "</a>");
            }

            messageItem.messageText = message;
            return messageItem;
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

    public class FormattedMessage
    {
        public string messageText { get; set; }
        public bool userIncluded { get; set; }
        public user userReference { get; set; }
    }
}
