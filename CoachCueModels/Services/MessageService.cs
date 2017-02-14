using CoachCue.Model;
using CoachCue.Models;
using CoachCue.Repository;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoachCue.Service
{
    public static class MessageService
    { 
        //save a message document to the Messages collection
        public static async Task<Message> Save(CoachCueUserData userData, string playerIDs, string text, string type, string parentID)
        {
            Message message = new Message();

            try
            {
                if (string.IsNullOrEmpty(text)) //don't save if too big or empty
                    return message;

                FormattedMessage messageItem = await FormatMessage(text);
                message.CreatedBy = userData.UserId;
                message.Text = messageItem.messageText;

                DateTime messageCreated = DateTime.UtcNow.GetEasternTime();
                message.DateCreated = messageCreated;
                message.UserName = userData.UserName;
                message.Name = userData.Name;
                message.ProfileImage = userData.ProfileImage;

                //check for opengraph info
                Media media = new Media();
                if (messageItem.openGraph.IsOpenGraph)
                {
                    media.Title = messageItem.openGraph.Title;
                    media.Url = messageItem.openGraph.URL;
                    if (messageItem.openGraph.MediaTypeID == 1)
                    {
                        media.ObjectUrl = messageItem.openGraph.Video;
                    }
                    else if (messageItem.openGraph.MediaTypeID == 2)
                    {
                        media.ObjectUrl = messageItem.openGraph.Image;
                    }
                }
                message.Media = media;

                //add any player mentions
                List<string> players = new List<string>();
                foreach (string playerID in playerIDs.Split(',').ToList())
                {
                    if (!string.IsNullOrEmpty(playerID))
                    {
                        players.Add(playerID);
                    }
                }
                message.PlayerMentions = new List<Player>();

                message.Reply = new List<Message>();
                message.UserMentions = new List<User>();
                //add a notifications and user mention joins
                /* if (messageItem.userIncluded)
                 {
                     foreach (user mention in messageItem.userList)
                     {
                         AddUserMention(msg.messageID, mention.userID);
                         notification mentionNotice = notification.Add("messageMention", msg.messageID, userID, mention.userID, messageCreated);
                         savedMsg.MentionNotices.Add(new MentionNotice { fromUser = userID, toUser = mention.userID, messageID = msg.messageID, noticeGuid = mentionNotice.notificationGUID });
                     }
                 }

                 //also add notification if its a message about the user created matchup
                 if (type == "matchup" && parentID.HasValue)
                 {
                     matchup matchupItem = matchup.Get(parentID.Value);
                     notification matchupNotice = notification.Add("matchupMessage", matchupItem.matchupID, userID, matchupItem.createdBy, messageCreated);
                     savedMsg.MentionNotices.Add(new MentionNotice { fromUser = userID, toUser = matchupItem.createdBy, messageID = matchupItem.matchupID, noticeGuid = matchupNotice.notificationGUID });
                 }
                 */

                //it has a parent (matchup or message) so update with message
                if (!string.IsNullOrEmpty(parentID))
                {
                    message.Id = "parent-" + parentID;
                    var parentMsg = await Get(parentID);
                    parentMsg.Reply.Add(message);

                    await DocumentDBRepository<Message>.UpdateItemAsync(parentMsg.Id, parentMsg, "Messages");
                }
                else //if not a child to either message or matchup then create new
                    await DocumentDBRepository<Message>.CreateItemAsync(message, "Messages");
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
              
            return message;
        }

        public static async Task<IEnumerable<Message>> GetList(DateTime endDate)
        {
            return await DocumentDBRepository<Message>.GetItemsAsync(d => d.DateCreated > endDate, "Messages");
        }

        public static async Task<Message> Get(string id)
        {
            return await DocumentDBRepository<Message>.GetItemAsync(id, "Messages");
        }

        public static async Task<FormattedMessage> FormatMessage(string txt)
        {
            FormattedMessage messageItem = new FormattedMessage();
            //spilt the message into words
            List<string> words = txt.Replace("https", "http").Split(' ').ToList();
            List<string> domains = new string[] { ".com", ".org", ".net", ".mil", ".edu" }.ToList();
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
                        messageItem.userList.Add(userItem);
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

                //check for opengraph data
                messageItem.openGraph = OpenGraph.Parse(match.Value);

                string showUrl = (match.Value.Length > 25) ? googleUrl : match.Value;
                message = message.Replace(match.Value, "<a target='_blank' href='" + googleUrl + "'>" + showUrl + "</a>");
            }

            messageItem.messageText = message;
            return messageItem;
        }
    }

    //Helper classes
    public class FormattedMessage
    {
        public string messageText { get; set; }
        public OpenGraphResponse openGraph { get; set; }
        public bool userIncluded { get; set; }
        public List<user> userList { get; set; }

        public FormattedMessage()
        {
            this.userList = new List<user>();
            this.openGraph = new OpenGraphResponse();
        }
    }
}
