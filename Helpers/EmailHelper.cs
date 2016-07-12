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

namespace CoachCue.Helpers
{
    public class EmailHelper
    {
        public static void SendVoteRequestEmail(int fromID, int toID, int matchupID, string noticeGuid)
        {
            try
            {
                LinkData link = notification.GetMatchupLink(matchupID, noticeGuid);
                string fromName = user.Get(fromID).fullName;
                
                user userToItem = user.Get(toID);
                string toEmail = userToItem.email;

                CoachCue.Mailers.IUserMailer UserMailer = new UserMailer();

                SmtpClientWrapper wrapper = new SmtpClientWrapper(getSmtpConfig());
                //check the users settings before sending
                if( user.GetSettings( toID ).emailNotifications.Value == true )
                    UserMailer.RequestVote(toEmail, userToItem.userGuid, fromID, link).Send(wrapper);
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
            }
        }

        public static void SendMatchupVoteEmail(int fromID, int matchupID)
        {
            try
            {
                LinkData link = notification.GetMatchupLink(matchupID, string.Empty);

                matchup matchupItem = matchup.Get(matchupID);
                user userToItem = matchupItem.user;

                //don't send if voting on matchup that user created
                if (matchupItem.createdBy != fromID)
                {
                    CoachCue.Mailers.IUserMailer UserMailer = new UserMailer();

                    SmtpClientWrapper wrapper = new SmtpClientWrapper(getSmtpConfig());
                    //check the users settings before sending
                    if (user.GetSettings(matchupItem.createdBy).emailNotifications.Value == true)
                        UserMailer.MatchupVoted(userToItem, fromID, link).Send(wrapper);
                }
            }
            catch (Exception) { }
        }

        public static void SendFollowEmail(int userID, int followID)
        {
            try
            {
                //don't send if voting on matchup that user created
                if (userID != 0 && followID != 0)
                {
                    CoachCue.Mailers.IUserMailer UserMailer = new UserMailer();

                    SmtpClientWrapper wrapper = new SmtpClientWrapper(getSmtpConfig());
                    //check the users settings before sending
                    if (user.GetSettings(followID).emailNotifications.Value == true)
                        UserMailer.Follow(userID, followID).Send(wrapper);
                }
            }
            catch (Exception) { }
        }

        public static void SendMentionEmail(int fromID, int toID, int messageID, string noticeGuid)
        {
            try
            {
                LinkData link = notification.GetMentionLink(messageID, noticeGuid);
                string fromName = user.Get(fromID).fullName;

                user userToItem = user.Get(toID);
                string toEmail = userToItem.email;

                CoachCue.Mailers.IUserMailer UserMailer = new UserMailer();

                SmtpClientWrapper wrapper = new SmtpClientWrapper(getSmtpConfig());
                //check the users settings before sending
                if (user.GetSettings(toID).emailNotifications.Value == true)
                    UserMailer.UserMention(toEmail, fromID, userToItem.userGuid, fromName, link).Send(wrapper);
            }
            catch (Exception) { }
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

        public static int SendNotificationEmail()
        {
            int sent = 0;
            
            /*CoachCue.Mailers.IUserMailer UserMailer = new UserMailer();

            foreach (user userItem in user.List())
            {
                List<MatchupNotification> matchupNotices = new List<MatchupNotification>();

                List<notification> notices = notification.GetByUserID(userItem.userID, false).Where( not => not.status.statusName == "Active" ).ToList();
                if (notices.Count() > 0)
                {
                    List<LinkData> links = new List<LinkData>();
                    foreach (notification notice in notices)
                    {
                        List<int> matchups = notice.user1.matchups.Where(mt => mt.createdBy == notice.sentTo).Select(mt => mt.matchupID).ToList();
                        users_matchup userMatch = notice.user.users_matchups.Where(usm => matchups.Contains(usm.matchupID) && usm.dateCreated == notice.dateCreated).FirstOrDefault();
                        if (userMatch != null)
                        {
                            links.Add(new LinkData { Message = userMatch.matchup.nflplayer.fullName + " vs " + userMatch.matchup.nflplayer1.fullName, ID = userMatch.matchupID });
                        }
                    }

                    var matchupGroups = from n in links
                                        group n by n.ID into g
                                        select new MatchupNotification { ID = g.Key, VoteCount = g.Count(), Match = links.Where( ln => ln.ID == g.Key ).FirstOrDefault().Message };

                    matchupNotices = matchupGroups.ToList();

                    if (matchupNotices.Count() > 0)
                    {
                        SmtpClientWrapper wrapper = new SmtpClientWrapper(getSmtpConfig());
                        //check the users settings before sending
                        if (user.GetSettings(userItem.userID).emailNotifications.Value == true)
                        {
                            UserMailer.Notifications(userItem.email, userItem.userGuid, userItem.fullName, matchupNotices).Send(wrapper);
                            sent++;
                        }
                    }
                }
            }*/

            return sent;
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