using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using CoachCue.Mailers;
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
                IUserMailer UserMailer = new UserMailer();

                //check the users settings before sending
                foreach (var notification in notifications)
                {
                    var userTo = await UserService.Get(notification.UserTo);
                    var userFrom = await UserService.Get(notification.UserFrom);
                    var matchup = await MatchupService.Get(notification.Matchup);

                    if (userTo.Settings.EmailNotifications == true)
                        UserMailer.RequestVote(notification, userTo, userFrom, matchup).Send(new SmtpClientWrapper(getSmtpConfig()));

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

                var userTo = await UserService.Get(notification.UserTo);
                var userFrom = await UserService.Get(notification.UserFrom);
                var matchup = await MatchupService.Get(notification.Matchup);

                if (userTo.Settings.EmailNotifications == true)
                    UserMailer.MatchupVoted(notification, userTo, userFrom, matchup).Send(new SmtpClientWrapper(getSmtpConfig()));

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
                    var userTo = await UserService.Get(notification.UserTo);
                    var userFrom = await UserService.Get(notification.UserFrom);

                    if (userTo.Settings.EmailNotifications == true)
                    {
                        if (notification.Type == "trophy")
                        {
                            var trophyMsg = new Message();
                            trophyMsg.Text = "Congratulations! You have earned a new Trophy, " + notification.Message + ", from CoachCue";
                            UserMailer.Notifications(notification, userFrom, userTo, trophyMsg).Send(new SmtpClientWrapper(getSmtpConfig()));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(notification.Message))
                            {
                                var message = await MessageService.Get(notification.Message);
                                UserMailer.Notifications(notification, userFrom, userTo, message).Send(new SmtpClientWrapper(getSmtpConfig()));
                            }
                            else if (!string.IsNullOrEmpty(notification.Matchup))
                            {
                                var matchup = await MatchupService.Get(notification.Matchup);
                                UserMailer.MatchupNotifications(notification, userFrom, userTo, matchup).Send(new SmtpClientWrapper(getSmtpConfig()));
                            }
                        }
                    }

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