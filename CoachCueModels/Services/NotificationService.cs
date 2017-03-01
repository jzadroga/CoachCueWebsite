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
using System.Web;

namespace CoachCue.Service
{
    public static class NotificationService
    { 
        //save a notification document to the Notifications collection
        public static async Task<Notification> Save(CoachCueUserData userData, User toUser, string text, string type, Message message)
        {
            Notification notification = new Notification();

            try
            {
                notification.DateCreated = DateTime.UtcNow.GetEasternTime();
                notification.CreatedBy = userData.UserId;
                notification.Text = text;
                notification.Type = type;
                notification.UserFrom = new User()
                {
                    Id = userData.UserId,
                    UserName = userData.UserName,
                    Name = userData.Name,
                    Profile = new UserProfile() { Image = userData.ProfileImage },
                    Email = userData.Email
                };
                notification.UserTo = toUser;
                notification.Message = message;

                var result = await DocumentDBRepository<Notification>.CreateItemAsync(notification, "Notifications");
                notification.Id = result.Id;            
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
              
            return notification;
        }

        public static async Task<IEnumerable<Notification>> GetList(string userId)
        {
            return await DocumentDBRepository<Notification>.GetItemsAsync(d => d.UserTo.Id == userId, "Notifications");
        }

        public static async Task<Notification> Get(string id)
        {
            return await DocumentDBRepository<Notification>.GetItemAsync(id, "Notifications");
        }

        public static async Task<IEnumerable<Notification>> GetByMessage(string messageId)
        {
            return await DocumentDBRepository<Notification>.GetItemsAsync(d => d.Message.Id == messageId && d.Sent == false, "Notifications");
        }
    }

    public class LinkData
    {
        public string Message { get; set; }
        public string ID { get; set; }
        public string Guid { get; set; }
    }
}
