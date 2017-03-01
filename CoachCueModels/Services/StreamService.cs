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

                //gets the matchups for the stream
                /*List<WeeklyMatchups> usrMatchups = matchup.GetList(userID, fromDate.Value, position, futureTimeline);
                stream = usrMatchups.Select(usrmtch => new Service.StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.CreatedBy.avatar.imageName,
                    UserProfileImg = profileImage,
                    UserName = usrmtch.CreatedBy.userName,
                    FullName = usrmtch.CreatedBy.fullName,
                    ContentType = GetMatchupContentType(usrmtch),
                    DateCreated = usrmtch.LastDate
                }).ToList();
                */


                //get all the user messages for following a user or player
                var endDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);

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
        public WeeklyMatchups MatchupItem { get; set; }
        public TweetContent Tweet { get; set; }
        public int PlayerID { get; set; }
        public string CssClass { get; set; }
        public nflplayer Player { get; set; }
        public string Source { get; set; }
        public bool HideActions { get; set; }
    }
}
