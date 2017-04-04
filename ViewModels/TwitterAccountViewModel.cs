using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoachCue.Model;
using CoachCue.Models;

namespace CoachCue.ViewModels
{
    public class TwitterAccountViewModel
    {
        public List<nflteam> Teams { get; set; }
        public List<twitteraccounttype> Types { get; set; }
    }

    public class TeamAccountModel
    {
        public nflteam Team { get; set; }
        public List<twitteraccount> TwitterAccounts { get; set; }
    }

    public class PlayerTwitterModel
    {
        public string PlayerID { get; set; }
        public string TeamSlug { get; set; }
        public List<TwitterAccount> TwitterAccounts { get; set; }
    }
}