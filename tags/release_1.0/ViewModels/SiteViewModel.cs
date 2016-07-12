using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoachCue.Model;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CoachCue.ViewModels
{
    public class BaseViewModel
    {
        public string Name { get; set; }
        public int Players { get; set; }
        public int Coaches { get; set; }
        public string Avatar { get; set; }
        public int TotalStarters { get; set; }
        public int CorrectStarters { get; set; }
        public int AccountID { get; set; }
        public int TotalCreatedMatchups { get; set; }
        public int NoticeCount { get; set; }
        public List<user> TopCoaches { get; set; }
        public List<user> WeeklyTopCoaches { get; set; }
        public int WeekNumber { get; set; }
        public List<WeeklyMatchups> Matchups { get; set; }
        public List<user> AskCoaches { get; set; }
        public List<user> InvitedCoaches { get; set; }
    }

    public class HomeViewModel : BaseViewModel
    {
        public bool ShowWelcome { get; set; }
        public bool LoggedIn { get; set; }
        public List<PlayerStream> Stream { get; set; }
        public List<AccountData> TrendingItems { get; set; }
        public List<AccountData> AllTrendingItems { get; set; }
        public bool ShowRegistration { get; set; }
        public bool ShowFriendInvite { get; set; }
    }

    public class PlayerViewModel : BaseViewModel
    {
        public bool LoggedIn { get; set; }
        public List<AccountData> TrendingItems { get; set; }
        public nflplayer PlayerDetail { get; set; }
        public List<StreamContent> PlayerStream { get; set; }
        public int Followers { get; set; }
    }

    public class UserViewModel : BaseViewModel
    {
        public List<AccountData> TrendingItems { get; set; }
        public user UserDetail { get; set; }
        public List<StreamContent> UserStream { get; set; }
        public int Followers { get; set; }
        public bool MessageDetails {get; set;}
    }

    public class LeaderBoardModel : BaseViewModel
    {
        public List<AccountData> TrendingItems { get; set; }
        public List<LeaderboardCoach> LeaderCoaches { get; set; }
        public List<GameWeek> Weeks { get; set; }
        public int SelectedWeek { get; set; }
    }

    public class FollowersModel : BaseViewModel
    {
        public string FollowItem { get; set; }
        public string FollowImg { get; set; }
        public string FollowLink { get; set; }
        public List<user> FollowCoaches { get; set; }
        public List<AccountData> TrendingItems { get; set; }
    }

    public class FollowingModel : BaseViewModel
    {
        public int FollowingCount { get; set; }
        public List<user> FollowingCoaches { get; set; }
        public List<AccountData> FollowingPlayers { get; set; }
        public List<AccountData> TrendingItems { get; set; }
    }

    public class MyMatchupViewModel : BaseViewModel
    {
        public List<AccountData> TrendingItems { get; set; }
        public WeeklyMatchups MyMatchup { get; set; }
        public bool LoggedIn { get; set; }
    }

    public class MatchupsListViewModel : BaseViewModel
    {
        public List<WeeklyMatchups> MyMatchups { get; set; }
        public List<WeeklyMatchups> AllMatchups { get; set; }
        public List<AccountData> TrendingItems { get; set; }
        public int SelectedWeek { get; set; }
        public List<GameWeek> Weeks { get; set; }
        public bool ShowMatchupAdd { get; set; }
    }

    public class NotificationsViewModel : BaseViewModel
    {
        public List<notification> Notifications { get; set; }
        public List<AccountData> TrendingItems { get; set; }
    }

    public class ConversationViewModel
    {
        public List<message> ReplyMessages { get; set; }
        public message ParentMessage { get; set; }
        public string Avatar { get; set; }
        public bool IsAuth { get; set; }
        public bool SingleMessage { get; set; }
        public bool IsReply { get; set; }
        public bool ShowReply { get; set; }
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
            this.UserName = userAccount.userName;
            this.Email = userAccount.email;
            this.FullName = userAccount.fullName;
            this.CurrentTab = "profile";
            this.DisplayMessage = false;
            this.Avatar = userAccount.avatar.imageName;
        }

        public bool DisplayMessage { get; set; }
        public string Message { get; set; }
        public string CurrentTab { get; set; }
 
        public string UserName { get; set; }
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
}