﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoachCue.Model;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CoachCue.Service;
using CoachCue.Models;
using CoachCue.Repository;

namespace CoachCue.ViewModels
{
    public class BaseViewModel
    {
        public int FollowingCount { get; set; }
        public int FollowersCount { get; set; }
        public int MessageCount { get; set; }
        public int TotalStarters { get; set; }
        public int CorrectStarters { get; set; }
        public int MatchupCount { get; set; }
        public string PlayerID { get; set; }
        public List<AccountData> TrendingItems { get; set; }
        public List<AccountData> RecentlyViewedItems { get; set; }
        public List<VotedPlayers> TopVotedPlayers { get; set; }
        public bool IsMobile { get; set; }
        public List<LeaderboardCoach> TopCoaches { get; set; }
        public CoachCueUserData UserData { get; set; }
        public List<Player> MessagePlayers { get; set; }

        public BaseViewModel()
        {
            this.RecentlyViewedItems = new List<AccountData>();
            this.TrendingItems = new List<AccountData>();
            this.MessagePlayers = new List<Player>();
        }
    }

    public class PageViewModel : BaseViewModel
    {
        public string Content { get; set; }
    }

    public class HomeViewModel : BaseViewModel
    {
        public bool ShowWelcome { get; set; }
        public bool LoggedIn { get; set; }
        public List<StreamContent> Stream { get; set; }
        public bool ShowRegistration { get; set; }
        public bool ShowFriendInvite { get; set; }
        public List<WeeklyMatchups> CurrentMatchups {get;set;}
        public List<WeeklyMatchups> MyMatchups { get; set; }
        public bool LoadMatchups { get; set; }

        public HomeViewModel()
        {
            this.Stream = new List<Service.StreamContent>();
            this.CurrentMatchups = new List<WeeklyMatchups>();
            this.MyMatchups = new List<WeeklyMatchups>();
        }
    }

    public class PlayerViewModel : BaseViewModel
    {
        public bool LoggedIn { get; set; }
        public Player PlayerDetail { get; set; }
        public List<StreamContent> PlayerStream { get; set; }
        public List<StreamContent> TwitterContent { get; set; }
    }

    public class UserViewModel : BaseViewModel
    {
        public User UserDetail { get; set; }
        public List<StreamContent> UserStream { get; set; }
    }

    public class LeaderBoardModel : BaseViewModel
    {
        public List<LeaderboardCoach> LeaderCoaches { get; set; }
        public List<GameWeek> Weeks { get; set; }
        public int SelectedWeek { get; set; }
        public LeaderboardCoach CurrentUser { get; set; }
        public bool UserIncluded { get; set; }
        public string Sort { get; set; }
        public string Direction { get; set; }
    }

    public class FollowersModel : BaseViewModel
    {
        public string FollowItem { get; set; }
        public string FollowImg { get; set; }
        public string FollowLink { get; set; }
        public List<user> FollowCoaches { get; set; }
    }

    public class FollowingModel : BaseViewModel
    {
        public int FollowingCount { get; set; }
        public List<user> FollowingCoaches { get; set; }
        public List<AccountData> FollowingPlayers { get; set; }
        public user UserDetail { get; set; }
        public int CurrentUserID { get; set; }

    }

    public class MyMatchupViewModel : BaseViewModel
    {
        public StreamContent Matchup { get; set; }
        public bool LoggedIn { get; set; }
        public List<StreamContent> RelatedMatchups { get; set; }
    }

    public class MatchupsListViewModel : BaseViewModel
    {
        public List<WeeklyMatchups> MyMatchups { get; set; }
        public List<WeeklyMatchups> AllMatchups { get; set; }
        public int SelectedWeek { get; set; }
        public List<GameWeek> Weeks { get; set; }
        public string WeekDescription { get; set; }
    }

    public class NotificationsViewModel : BaseViewModel
    {
        public List<notification> Notifications { get; set; }
    }

    public class ConversationViewModel
    {
        public List<message> Messages { get; set; }
        public int SourceMessageID { get; set; }
        public string Type { get; set; }
        public string Avatar { get; set; }
        public bool ShowInlineMessage { get; set; }
        public bool IsTopMessage { get; set; }

        public ConversationViewModel()
        {
            this.Messages = new List<message>();
        }
    }

    public class MessageViewModel
    {
        public CoachCueUserData User { get; set; }
        public Message ParentMessage { get; set; }
        public List<nflplayer> MessagePlayers { get; set; }
        public string Type { get; set; }
        public matchup Matchup { get; set; }
        public string ParentID { get; set; }
        public List<user> MatchupInvites { get; set; }

        public MessageViewModel()
        {
            this.MessagePlayers = new List<nflplayer>();
            this.ParentMessage = new Message();
            this.Matchup = new matchup();
            this.MatchupInvites = new List<user>();
        }
    }

    public class SettingsViewModel : BaseViewModel
    {
        public bool MatchVotes { get; set; }
        public bool DisplayMessage { get; set; }
        public string Message { get; set; }
        public string CurrentTab { get; set; }
        public string RecieveNotificationEmail { get; set; }
        public IEnumerable<SelectListItem> RecieveNotificationEmailOptions { get; set; }
    }

    public class ProfileViewModel : BaseViewModel
    {
        public ProfileViewModel() { }

        public ProfileViewModel(user userAccount)
        {
            this.AccountUserName = userAccount.userName;
            this.Email = userAccount.email;
            this.FullName = userAccount.fullName;
            this.CurrentTab = "profile";
            this.DisplayMessage = false;
            //this.Avatar = userAccount.avatar.imageName;
        }

        public bool DisplayMessage { get; set; }
        public string Message { get; set; }
        public string CurrentTab { get; set; }
 
        public string AccountUserName { get; set; }
        public string FullName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class SearchResultViewModel : BaseViewModel
    {
        public List<AccountData> Accounts { get; set; }
        public string SearchTerm { get; set; }
        public bool ShowFollow { get; set; }
    }

    public class TopPlayersViewModel : BaseViewModel
    {
        public List<VotedPlayers> Players { get; set; }
    }

    public class TrendingNewsViewModel : BaseViewModel
    {
        public List<AccountData> Players { get; set; }
    }

    public class SignupViewModel
    {
        public string Message { get; set; }
        public bool ShowForm { get; set; }
        public bool InvalidLogin { get; set; }
    }
        
}