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

namespace CoachCue.Service
{
    public static class StreamService
    {
        public static async Task<List<StreamContent>> GetHomeStream(CoachCueUserData userData)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                string profileImage = userData.ProfileImage;

                //get all the user messages for following a user or player
                var endDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);

                //gets the matchups for the stream
                string contentType = "matchupSelected";
                /*if (string.IsNullOrEmpty(matchup.UserSelectedPlayer))
                {
                    //make sure the game hasn't passed too
                    if (DateTime.UtcNow.GetEasternTime() < matchup.GameDate)
                        contentType = "matchup";
                }*/
                var matchups = await MatchupService.GetList(endDate);
                stream = matchups.Select(usrmtch => new Service.StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.ProfileImage,
                    UserProfileImg = profileImage,
                    UserName = usrmtch.UserName,
                    FullName = usrmtch.Name,
                    ContentType = contentType,
                    DateCreated = usrmtch.DateCreated,
                    TimeAgo = twitter.GetRelativeTime(usrmtch.DateCreated),
                    HideActions = (usrmtch.CreatedBy == userData.UserId) ? false : true,
                }).ToList();
                
                var msgs = await MessageService.GetList(endDate);
                stream.AddRange(msgs.Select(msg => new StreamContent
                {
                    MessageItem = msg,
                    DateTicks = msg.DateCreated.Ticks.ToString(),
                    ProfileImg = msg.ProfileImage,
                    UserName = msg.UserName,
                    FullName = msg.Name,
                    ContentType = "message",
                    DateCreated = msg.DateCreated,
                    UserProfileImg = profileImage,
                    TimeAgo = twitter.GetRelativeTime(msg.DateCreated)
                }).ToList());
                
                //sort everything by date
                stream = stream.OrderByDescending(str => str.DateCreated).Take(80).ToList();
            }
            catch (Exception ex)
            {
                string r = ex.Message;
            }

            return stream;
        }

        public static async Task<StreamContent> GetDetailStream(CoachCueUserData userData, string link)
        {
            StreamContent stream = new StreamContent();

            string profileImage = userData.ProfileImage;

            var matchup = await MatchupService.GetByLink(link);
            string contentType = "matchupSelected";

            if (matchup != null)
            {
                stream.MatchupItem = matchup;
                stream.DateTicks = matchup.DateCreated.Ticks.ToString();
                stream.ProfileImg = matchup.ProfileImage;
                stream.UserProfileImg = profileImage;
                stream.UserName = matchup.UserName;
                stream.FullName = matchup.Name;
                stream.ContentType = contentType;
                stream.DateCreated = matchup.DateCreated;
                stream.TimeAgo = twitter.GetRelativeTime(matchup.DateCreated);
                stream.HideActions = (matchup.CreatedBy == userData.UserId) ? false : true;
            }

            return stream;           
        }

        public static async Task<List<StreamContent>> GetRelatedStream(CoachCueUserData userData, Matchup matchup)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                string profileImage = userData.ProfileImage;

                string contentType = "matchupSelected";
                var endDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);

                var matchups = await MatchupService.GetRelatedList(endDate, matchup);
                stream = matchups.Select(usrmtch => new Service.StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.ProfileImage,
                    UserProfileImg = profileImage,
                    UserName = usrmtch.UserName,
                    FullName = usrmtch.Name,
                    ContentType = contentType,
                    DateCreated = usrmtch.DateCreated,
                    TimeAgo = twitter.GetRelativeTime(usrmtch.DateCreated),
                    HideActions = (usrmtch.CreatedBy == userData.UserId) ? false : true
                }).ToList();
            }
            catch (Exception)
            {
            }

            return stream;
        }

        public static async Task<List<StreamContent>> GetPlayerStream(CoachCueUserData userData, string playerId)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                string profileImage = userData.ProfileImage;

                //get all the user messages for following a user or player
                var endDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);

                //gets the matchups for the stream
                string contentType = "matchupSelected";
                /*if (string.IsNullOrEmpty(matchup.UserSelectedPlayer))
                {
                    //make sure the game hasn't passed too
                    if (DateTime.UtcNow.GetEasternTime() < matchup.GameDate)
                        contentType = "matchup";
                }*/
                var matchups =  MatchupService.GetListByPlayer(endDate, playerId);
                stream = matchups.Select(usrmtch => new Service.StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.ProfileImage,
                    UserProfileImg = profileImage,
                    UserName = usrmtch.UserName,
                    FullName = usrmtch.Name,
                    ContentType = contentType,
                    DateCreated = usrmtch.DateCreated,
                    TimeAgo = twitter.GetRelativeTime(usrmtch.DateCreated),
                    HideActions = (usrmtch.CreatedBy == userData.UserId) ? false : true
                }).ToList();

                var msgs = await MessageService.GetListByPlayer(endDate, playerId);
                stream.AddRange(msgs.Select(msg => new StreamContent
                {
                    MessageItem = msg,
                    DateTicks = msg.DateCreated.Ticks.ToString(),
                    ProfileImg = msg.ProfileImage,
                    UserName = msg.UserName,
                    FullName = msg.Name,
                    ContentType = "message",
                    DateCreated = msg.DateCreated,
                    UserProfileImg = profileImage,
                    TimeAgo = twitter.GetRelativeTime(msg.DateCreated)
                }).ToList());

                //sort everything by date
                stream = stream.OrderByDescending(str => str.DateCreated).Take(80).ToList();
            }
            catch (Exception ex)
            {
                string r = ex.Message;
            }

            return stream;
        }

        public static async Task<List<StreamContent>> GetUserStream(CoachCueUserData userData, string userId, bool matchupsOnly)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                string profileImage = userData.ProfileImage;

                //get all the user messages for following a user or player
                var endDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);

                //gets the matchups for the stream
                string contentType = "matchupSelected";
                /*if (string.IsNullOrEmpty(matchup.UserSelectedPlayer))
                {
                    //make sure the game hasn't passed too
                    if (DateTime.UtcNow.GetEasternTime() < matchup.GameDate)
                        contentType = "matchup";
                }*/
                var matchups = await MatchupService.GetListByUser(endDate, userId);
                stream = matchups.Select(usrmtch => new Service.StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.ProfileImage,
                    UserProfileImg = profileImage,
                    UserName = usrmtch.UserName,
                    FullName = usrmtch.Name,
                    ContentType = contentType,
                    DateCreated = usrmtch.DateCreated,
                    TimeAgo = twitter.GetRelativeTime(usrmtch.DateCreated),
                    HideActions = (usrmtch.CreatedBy == userData.UserId) ? false : true
                }).ToList();

                if (!matchupsOnly)
                {
                    var msgs = await MessageService.GetListByUser(endDate, userId);
                    stream.AddRange(msgs.Select(msg => new StreamContent
                    {
                        MessageItem = msg,
                        DateTicks = msg.DateCreated.Ticks.ToString(),
                        ProfileImg = msg.ProfileImage,
                        UserName = msg.UserName,
                        FullName = msg.Name,
                        ContentType = "message",
                        DateCreated = msg.DateCreated,
                        UserProfileImg = profileImage,
                        TimeAgo = twitter.GetRelativeTime(msg.DateCreated)
                    }).ToList());
                }

                //sort everything by date
                stream = stream.OrderByDescending(str => str.DateCreated).Take(80).ToList();
            }
            catch (Exception ex)
            {
                string r = ex.Message;
            }

            return stream;
        }
    }

    public class StreamContent
    {
        public string UserProfileImg { get; set; }
        public string DateTicks { get; set; }
        public string TimeAgo { get; set; }
        public string ProfileImg { get; set; }
        public string UserName { get; set; }
        public DateTime DateCreated { get; set; }
        public string FullName { get; set; }
        public string ContentType { get; set; }
        public Message MessageItem { get; set; }
        public Matchup MatchupItem { get; set; }
        public TweetContent Tweet { get; set; }
        public int PlayerID { get; set; }
        public string CssClass { get; set; }
        public nflplayer Player { get; set; }
        public string Source { get; set; }
        public bool HideActions { get; set; }
    }
}
