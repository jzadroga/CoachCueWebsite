﻿using CoachCue.Model;
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
using System.Web;

namespace CoachCue.Service
{
    public static class MessageService
    { 
        //save a message document to the Messages collection
        public static async Task<Message> Save(CoachCueUserData userData, string playerIDs, string text, string type, string parentID, HttpPostedFileBase imgFile)
        {
            Message message = new Message();

            try
            {
                if (string.IsNullOrEmpty(text)) //don't save if too big or empty
                    return message;

                FormattedMessage messageItem = await FormatMessage(text);
                message.CreatedBy = userData.UserId;
                message.Text = messageItem.messageText;
                message.UserMentions = messageItem.userList.Select( us => us.Id).ToList();

                DateTime messageCreated = DateTime.UtcNow.GetEasternTime();
                message.DateCreated = messageCreated;
                message.UserName = userData.UserName;
                message.Name = userData.Name;
                message.ProfileImage = userData.ProfileImage;
                message.Email = userData.Email;

                //check for opengraph info
                if (messageItem.openGraph.IsOpenGraph)
                {
                    Media media = new Media();
                    media.Title = messageItem.openGraph.Title;
                    media.Url = messageItem.openGraph.URL;
                    media.Site = messageItem.openGraph.SiteName;
                    media.Description = messageItem.openGraph.Description;

                    if (messageItem.openGraph.MediaTypeID == 1)
                    {
                        media.ObjectUrl = messageItem.openGraph.Video;
                        media.Type = "openGraphVideo";
                    }
                    else if (messageItem.openGraph.MediaTypeID == 2 || messageItem.openGraph.MediaTypeID == 0)
                    {
                        /*byte[] data;
                        using (WebClient client = new WebClient())
                        {
                            data = client.DownloadData(messageItem.openGraph.Image);
                        }
                        string mediaImg = HttpContext.Current.Request.PhysicalApplicationPath + "assets\\img\\avatar\\";
                        File.WriteAllBytes(mediaImg, data);
                        */
                        media.ObjectUrl = messageItem.openGraph.Image;
                        media.Type = "openGraphImage";
                    }
                    message.Media.Add(media);
                }

                //check for message image
                if (imgFile != null)
                {
                    if (imgFile.ContentLength <= 3000000)
                    {
                        Media messageMedia = new Media();
                        //save off message image
                        var fileName = userData.UserId + "_" + DateTime.Now.Ticks.ToString() + "_" + imgFile.FileName.Substring(imgFile.FileName.LastIndexOf("\\") + 1);
                        imgFile.SaveAs(HttpContext.Current.Request.PhysicalApplicationPath + "assets\\img\\avatar\\" + fileName);

                        messageMedia.Title = imgFile.FileName;
                        messageMedia.Url = fileName;
                        messageMedia.Type = "messageImage";

                        message.Media.Add(messageMedia);
                    }
                }

                //update message count
                await UserService.UpdateMessageCount(userData.UserId);

                //add any player mentions
                if (!string.IsNullOrEmpty(playerIDs))
                {
                    List<string> players = playerIDs.Split(',').ToList();
                    var playerList = await PlayerService.GetListByIds(players);

                    message.PlayerMentions.AddRange(playerList);
                }

                //it has a parent (matchup or message) so update with message
                if (!string.IsNullOrEmpty(parentID))
                {
                    if (type == "matchup")
                    {
                        var parentMatchup = await MatchupService.Get(parentID);
                        message.Id = parentMatchup.Id;

                        //add a notification if replying, send to everyone in matchup chain
                        var matchupUsers = await NotificationService.GetMatchupNotificationUsers(userData.UserId, parentMatchup);
                        foreach (var matchupUser in matchupUsers)
                        {
                            await NotificationService.Save(userData.UserId, matchupUser.Id, userData.Name + " Posted a new message about a matchup you are following.", "replyMatchup", message.Id);
                        }

                        parentMatchup.Messages.Add(message);
                        await DocumentDBRepository<Matchup>.UpdateItemAsync(parentMatchup.Id, parentMatchup, "Matchups");
                    }
                    else
                    {
                        message.Id = "parent-" + parentID;
                        var parentMsg = await Get(parentID);

                        //add a notification if replying, send to everyone in chain
                        var replyUsers = await NotificationService.GetReplyNotificationUsers(userData.UserId, parentMsg);
                        foreach (var replyUser in replyUsers)
                        {
                            await NotificationService.Save(userData.UserId, replyUser.Id, userData.Name + " Posted a new reply message.", "reply", parentID);
                        }

                        parentMsg.Reply.Add(message);
                        await DocumentDBRepository<Message>.UpdateItemAsync(parentMsg.Id, parentMsg, "Messages");
                    }
                }
                else //if not a child to either message or matchup then create new
                {
                   var result = await DocumentDBRepository<Message>.CreateItemAsync(message, "Messages");
                    message.Id = result.Id;
                }

                //add a notifications if a user is mentioned
                if (messageItem.userIncluded)
                {
                    foreach (User mention in messageItem.userList)
                    {
                        await NotificationService.Save(userData.UserId, mention.Id, userData.Name + " Posted a new message, mentioning you.", "mention", message.Id);
                    }
                }             
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
              
            return message;
        }

        public static async Task<bool> Delete(string id)
        {
            await DocumentDBRepository<Message>.DeleteItemAsync(id, "Messages");

            return true;
        }

        public static async Task<IEnumerable<Message>> GetList(DateTime endDate)
        {
            //return await DocumentDBRepository<Message>.GetItemsAsync(d => d.DateCreated > endDate, "Messages");
            return await DocumentDBRepository<Message>.GetItemsAsync(d => d.UserName != "", "Messages");
        }

        public static async Task<IEnumerable<Message>> GetListByUser(DateTime endDate, string userId)
        {
            //return await DocumentDBRepository<Message>.GetItemsAsync(d => d.DateCreated > endDate, "Messages");
            return await DocumentDBRepository<Message>.GetItemsAsync(d => d.CreatedBy == userId, "Messages");
        }

        public static IEnumerable<Message> GetListByPlayer(DateTime endDate, string playerId)
        {
            return DocumentDBRepository<Message>.GetPlayerMessages(playerId).OrderByDescending(msg => msg.DateCreated);
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
                    var userItem = await UserService.GetByUsername(word.Substring(1));
                    if (userItem != null)
                    {
                        messageItem.userIncluded = true;
                        messageItem.userList.Add(userItem);
                        message += "<a href='/coach/" + userItem.UserName + "'>" + word + "</a>";
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
                message = (messageItem.openGraph.IsOpenGraph ) ? message.Replace(match.Value,"") : message.Replace(match.Value, "<a target='_blank' href='" + googleUrl + "'>" + showUrl + "</a>");
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
        public List<User> userList { get; set; }

        public FormattedMessage()
        {
            this.userList = new List<User>();
            this.openGraph = new OpenGraphResponse();
        }
    }
}
