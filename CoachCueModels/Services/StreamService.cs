using CoachCue.Model;
using CoachCue.Models;
using CoachCue.Repository;
using HtmlAgilityPack;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace CoachCue.Service
{
    public static class StreamService
    {
        public static List<StreamContent> GetPositionStream(CoachCueUserData userData, string position)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                string profileImage = userData.ProfileImage;

                //get all the user messages for following a user or player
                var endDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);
       
                var matchups = MatchupService.GetListByPosition(endDate, position).Take(80);
                stream = MatchupToStream(userData, matchups);
            }
            catch (Exception ex)
            {
                string r = ex.Message;
            }

            return stream;
        }

        public static async Task<List<StreamContent>> GetTrendingNews()
        {
            List<StreamContent> stream = new List<Service.StreamContent>();

            try
            {
                string cacheID = "trendingPlayerNews";
                if (HttpContext.Current.Cache[cacheID] != null)
                    stream = (List<StreamContent>)HttpContext.Current.Cache[cacheID];
                else
                {
                    var trendingPlayers = await GetTrendingStream();
                    foreach (var trendingPlayer in trendingPlayers.Take(15))
                    {
                        var playerNews = await GetPlayerTwitterStream(trendingPlayer);
                        stream.AddRange(playerNews.Take(5));
                    }

                    HttpContext.Current.Cache.Insert(cacheID, stream, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 8, 0));
                }
            }
            catch (Exception) { }

            return stream.OrderByDescending( st => st.DateCreated ).ToList();
        }

        public static async Task<List<StreamContent>> GetPlayerTwitterStream(Player player)
        {
            List<StreamContent> stream = new List<StreamContent>();
            List<Player> playerMentions = new List<Player>();
            playerMentions.Add(player);

            try
            {
                //get all the tweets
                List<Search> playerTweets = await twitter.SearchPlayer(player, true);
                stream = new List<StreamContent>();
                if (playerTweets.Count() > 0)
                {
                    stream = playerTweets.Single().Statuses.Select(tweet => new Service.StreamContent
                    {
                        DateTicks = tweet.CreatedAt.Ticks.ToString(),
                        ProfileImg = tweet.User.ProfileImageUrl,
                        UserName = tweet.User.ScreenNameResponse,
                        FullName = tweet.User.Name,
                        ContentType = "tweet",
                        DateCreated = tweet.CreatedAt,
                        TimeAgo = twitter.GetRelativeTime(tweet.CreatedAt, false),
                        PlayerID = player.Id,
                        Tweet = new TweetContent { ID = tweet.UserID.ToString(),
                            Message = CoachCue.Model.TwitterExtensions.TextAsHtml(tweet.Text),
                            PlayerMentions = playerMentions
                        }
                    }).ToList();
                }
            }
            catch (Exception) { }

            return stream;
        }

        public static async Task<List<Player>> GetTrendingStream()
        {
            //need to cache this list
            List<Player> stream = new List<Player>();

            try
            {
                string cacheID = "trendingPlayers";
                if (HttpContext.Current.Cache[cacheID] != null)
                    stream = (List<Player>)HttpContext.Current.Cache[cacheID];
                else
                {
                    //try to pull in players from rotoword rss feed and blend with CoachCue
                    var rotoWorldRss = XmlReader.Create("http://www.rotoworld.com/rss/feed.aspx?sport=nfl&ftype=news&count=12&format=rss");
                    var players = SyndicationFeed.Load(rotoWorldRss);
                    rotoWorldRss.Close();

                    foreach (SyndicationItem story in players.Items)
                    {
                        if (story.Links != null)
                        {
                            var link = story.Links[0];
                            var playerName = link.Uri.Segments[link.Uri.Segments.Count() - 1];

                            //try and find a matching player
                            var player = await PlayerService.GetByLink(playerName);
                            if (player != null)
                            {
                                if( stream.Where( pl => pl.Id == player.Id).FirstOrDefault() == null)
                                 stream.Add(player);
                            }
                        }
                    }

                    //get the most voted on or matchup involved players
                    stream.AddRange(await PlayerService.GetTrending());

                    HttpContext.Current.Cache.Insert(cacheID, stream, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 2, 0));
                }
            }
            catch (Exception) {}

            return stream;
        }

        public static async Task<List<StreamContent>> GetHomeStream(CoachCueUserData userData)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                string profileImage = userData.ProfileImage;

                //get all the user messages for following a user or player
                var endDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);

                var matchups = await MatchupService.GetList(endDate);
                stream = MatchupToStream(userData, matchups);
                
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
                stream = stream.OrderByDescending(str => str.DateCreated).Take(100).ToList();
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

            var matchup = await MatchupService.GetByLink(link);
            if (matchup != null)
            {
                var matchups = new List<Matchup>();
                matchups.Add(matchup);
                stream = MatchupToStream(userData, matchups)[0];
            }

            return stream;           
        }

        public static async Task<List<StreamContent>> GetRelatedStream(CoachCueUserData userData, Matchup matchup)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                var endDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);

                var matchups = await MatchupService.GetRelatedList(endDate, matchup);
                stream = MatchupToStream(userData, matchups);
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
           
                var matchups =  MatchupService.GetListByPlayer(endDate, playerId);
                stream = MatchupToStream(userData, matchups);

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
                stream = stream.OrderByDescending(str => str.DateCreated).Take(100).ToList();
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
             
                var matchups = await MatchupService.GetListByUser(endDate, userId);
                stream = MatchupToStream(userData, matchups);

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
                stream = stream.OrderByDescending(str => str.DateCreated).Take(100).ToList();
            }
            catch (Exception ex)
            {
                string r = ex.Message;
            }

            return stream;
        }

        public static List<StreamContent> MatchupToStream(CoachCueUserData userData, IEnumerable<Matchup> matchups)
        {
            return matchups.Select(usrmtch => new StreamContent
            {
                MatchupItem = usrmtch,
                DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                ProfileImg = usrmtch.ProfileImage,
                UserProfileImg = userData.ProfileImage,
                UserName = usrmtch.UserName,
                FullName = usrmtch.Name,
                ContentType = getMatchupContentType(userData.UserId, usrmtch),
                DateCreated = usrmtch.DateCreated,
                TimeAgo = twitter.GetRelativeTime(usrmtch.DateCreated),
                HideActions = (usrmtch.CreatedBy == userData.UserId) ? false : true,
                SelectedPlayerID = getSelectedPlayer(userData.UserId, usrmtch)
            }).ToList();
        }

        private static string getMatchupContentType(string userID, Matchup matchup)
        {
            string type = "matchupSelected";

            //if its over
            if (matchup.Completed == true || (DateTime.UtcNow.GetEasternTime() >= matchup.Players[0].GameWeek.Date))
                return type;

            type = (matchup.Votes.Where(vt => vt.UserId == userID).Count() > 0) ? "matchupSelected" : "matchup";

            return type;
        }

        private static string getSelectedPlayer(string userID, Matchup matchup)
        {
            string selected = string.Empty;
            if (string.IsNullOrEmpty(userID))
                return selected;

            var voted = matchup.Votes.Where(vt => vt.UserId == userID).FirstOrDefault();
            if (voted != null)
                selected = voted.PlayerId;

            return selected;
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
        public string PlayerID { get; set; }
        public string SelectedPlayerID { get; set; }
        public string CssClass { get; set; }
        public string Source { get; set; }
        public bool HideActions { get; set; }
    }
}
