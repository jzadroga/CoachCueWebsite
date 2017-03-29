using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using CoachCue.Mailers;
using CoachCue.Model;
using Mvc.Mailer;
using System.Threading.Tasks;
using CoachCue.Models;
using CoachCue.Service;

namespace CoachCue.Helpers
{
    public class EmailHelper
    {
        public static async Task <bool> SendVoteRequestEmail(List<Notification> notifications)
        {
            try
            {
                /*LinkData link = notification.GetMatchupLink(matchupID, noticeGuid);
                string fromName = user.Get(fromID).fullName;
                
                user userToItem = user.Get(toID);
                string toEmail = userToItem.email;

                CoachCue.Mailers.IUserMailer UserMailer = new UserMailer();

                SmtpClientWrapper wrapper = new SmtpClientWrapper(getSmtpConfig());
                //check the users settings before sending
                if( user.GetSettings( toID ).emailNotifications.Value == true )
                    UserMailer.RequestVote(toEmail, userToItem.userGuid, fromID, link).Send(wrapper);*/

                IUserMailer UserMailer = new UserMailer();

                //check the users settings before sending
                foreach (var notification in notifications)
                {
                    if (notification.UserTo.Settings.EmailNotifications == true)
                        UserMailer.RequestVote(notification).Send(new SmtpClientWrapper(getSmtpConfig()));

                    //mark as sent
                    notification.Sent = true;
                    await NotificationService.Update(notification);
                }
            }
            catch(Exception){}

            return true;
        }

        public static async Task<bool> SendMatchupVoteEmail(Notification notification)
        {
            try
            {
                IUserMailer UserMailer = new UserMailer();

                if (notification.UserTo.Settings.EmailNotifications == true)
                    UserMailer.MatchupVoted(notification).Send(new SmtpClientWrapper(getSmtpConfig()));

                //mark as sent
                notification.Sent = true;
                await NotificationService.Update(notification);             
            }
            catch (Exception) { }

            return true;
        }

        public static async Task<bool> SendMessageNotificationEmails(List<Notification> notifications)
        {
            try
            {              
                IUserMailer UserMailer = new UserMailer();

                //check the users settings before sending
                foreach (var notification in notifications)
                {
                    if (notification.UserTo.Settings.EmailNotifications == true)
                        UserMailer.Notifications(notification).Send(new SmtpClientWrapper(getSmtpConfig()));

                    //mark as sent
                    notification.Sent = true;
                    await NotificationService.Update(notification);
                }                         
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return true;
        }

        public static void SendMatchupMessageEmail(int fromID, int toID, int matchupID, string noticeGuid)
        {
            try
            {
                LinkData link = notification.GetMatchupLink(matchupID, string.Empty);
                string fromName = user.Get(fromID).fullName;

                user userToItem = user.Get(toID);
                string toEmail = userToItem.email;

                CoachCue.Mailers.IUserMailer UserMailer = new UserMailer();

                SmtpClientWrapper wrapper = new SmtpClientWrapper(getSmtpConfig());
                //check the users settings before sending
                if (user.GetSettings(toID).emailNotifications.Value == true)
                    UserMailer.MatchupMessage(userToItem, fromID, link).Send(wrapper);
            }
            catch (Exception) { }
        }

        public static void Send(string toAddress, string emailLabel, string inviteMessage) 
        {
            CoachCue.Mailers.IUserMailer UserMailer = new UserMailer();

            SmtpClientWrapper wrapper = new SmtpClientWrapper(getSmtpConfig());
            UserMailer.Invite(toAddress, emailLabel, inviteMessage).Send(wrapper);
        }

        public static string Send(string toAddress, string messageBody, string subject, string emailLabel)
        {
            string emailAddress = "info@coachcue.com";

            string msg = string.Empty;

            try
            {
                SmtpClient client = getSmtpConfig();

                MailMessage mailMessage = new System.Net.Mail.MailMessage();
                foreach (string emailAdd in toAddress.Split(','))
                {
                    mailMessage.To.Add(new MailAddress(emailAdd));
                }
                mailMessage.From = new MailAddress(emailAddress, emailLabel);

                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = messageBody;

                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return msg;
        }

        private static SmtpClient getSmtpConfig()
        {
            string emailAddress = "info@coachcue.com";

            SmtpClient client = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailAddress, "coachcue_12")
            };

            return client;
        }
    }
}