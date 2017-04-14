using CoachCue.Model;
using CoachCue.Models;
using CoachCue.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CoachCue.Service
{
    public static class NotificationService
    { 
        //save a notification document to the Notifications collection
        public static async Task<Notification> Save(string userFrom, string toUser, string text, string type, string referenceId)
        {
            Notification notification = new Notification();

            try
            {
                notification.DateCreated = DateTime.UtcNow.GetEasternTime();
                notification.CreatedBy = userFrom;
                notification.Text = text;
                notification.Type = type;
                notification.UserFrom = userFrom;
                notification.UserTo = toUser;
                if (type == "vote" || type == "voteRequested")
                    notification.Matchup = referenceId;
                else
                    notification.Message = referenceId;

                notification.Id = Guid.NewGuid().ToString();

                //add notification to the user and update
                var user = await UserService.Get(toUser);
                user.Notifications.Add(notification);
                await DocumentDBRepository<User>.UpdateItemAsync(user.Id, user, "Users");
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return notification;
        }

        public static async Task<IEnumerable<Notification>> GetList(string userId)
        {
            var user = await UserService.Get(userId);
            return user.Notifications;
        }

        public static async Task<IEnumerable<Notification>> GetByMessage(string messageId)
        {
            var message = await MessageService.Get(messageId);
            var user = await UserService.Get(message.UserMentions.FirstOrDefault());

            return user.Notifications.Where(d => d.Message == messageId
                && d.Sent == false
                && (d.Type == "mention" || d.Type == "reply"));
        }

        public static async Task<Notification> GetByMatchup(string matchupId, string fromUser)
        {
            var matchup = await MatchupService.Get(matchupId);
            var user = await UserService.Get(matchup.CreatedBy);

            var notifications = user.Notifications.Where(d => d.Matchup == matchupId
                && d.Sent == false
                && d.Type == "vote"
                && d.UserFrom == fromUser);

            return notifications.FirstOrDefault();
        }

        public static async Task<Microsoft.Azure.Documents.Document> Update(Notification notification)
        {
            var user = await UserService.Get(notification.UserTo);

            var update=  user.Notifications.Where(nt => nt.Id == notification.Id).FirstOrDefault();
            update.Sent = notification.Sent;
            update.Read = notification.Read;
          
            return await DocumentDBRepository<User>.UpdateItemAsync(user.Id, user, "Users");
        }

        public static async Task<List<User>> GetMatchupNotificationUsers(string userId, Matchup matchup)
        {
            List<string> userIds = new List<string>();

            if(userId != matchup.CreatedBy)
                userIds.Add(matchup.CreatedBy);

            //don't include if user is replying to own message
            foreach (Message reply in matchup.Messages)
            {
                //don't send a reply if the user creating the message is already in the chain
                if (!userIds.Contains(reply.CreatedBy) && reply.CreatedBy != userId)
                    userIds.Add(reply.CreatedBy);
            }

            var replies = await UserService.GetListByIds(userIds);

            return replies.ToList();
        }


        public static async Task<List<User>> GetReplyNotificationUsers(string userId, Message message)
        {
            List<string> userIds = new List<string>();

            //don't include if user is replying to own message
            if(userId != message.CreatedBy)
                userIds.Add(message.CreatedBy);

            foreach (Message reply in message.Reply)
            {
                //don't send a reply if the user creating the message is already in the chain
                if (!userIds.Contains(reply.CreatedBy) && reply.CreatedBy != userId) 
                    userIds.Add(reply.CreatedBy);
            }

            var replies = await UserService.GetListByIds(userIds);

            return replies.ToList();
        }
    }

    public class LinkData
    {
        public string Message { get; set; }
        public string ID { get; set; }
        public string Guid { get; set; }
    }
}
