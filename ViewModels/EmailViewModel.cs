using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoachCue.Model;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CoachCue.ViewModels
{
    public class MailNotificationsViewModel
    {
        public string FullName { get; set; }
        public List<MatchupNotification> Notices { get; set; }
    }

    public class MailRequestVoteViewModel
    {
        public string FullName { get; set; }
        public LinkData MatchupLink { get; set; }
        public string FromAvatarSrc { get; set; }
        public string FullLink { get; set; }
    }

    public class MailMentionViewModel
    {
        public string FullName { get; set; }
        public LinkData MessageLink { get; set; }
        public string FromAvatarSrc { get; set; }
        public string UserMessageLink { get; set; }
    }

    public class MailFollowViewModel
    {
        public string NewFollowName { get; set; }
        public string NewFollowLink { get; set; }
        public string NewFollowAvatarSrc { get; set; }
    }
}