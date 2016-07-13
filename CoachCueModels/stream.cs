using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LinqToTwitter;
using System.Web;

namespace CoachCue.Model
{
    public class stream
    {
        public static List<StreamContent> GetPlayersStream(List<int> players, int userID, bool futureTimeline, DateTime fromDate)
        {
            List<StreamContent> playersStream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                foreach (int playerID in players)
                {
                    nflplayer player = nflplayer.Get(playerID);
                    playersStream.AddRange( GetPlayerStream(player, userID, false, futureTimeline, fromDate) );
                }
            }
            catch (Exception) { }

            return playersStream;
        }

        public static List<StreamContent> GetPlayersStream(List<nflplayer> players)
        {
            List<StreamContent> playersStream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                foreach (nflplayer ply in players)
                {
                    playersStream.AddRange(GetPlayerStream(ply, null, true, true));
                }

                //sort everything by date
                playersStream = playersStream.OrderByDescending(str => str.DateCreated).ToList();
            }
            catch (Exception) { }

            return playersStream;
        }

        //stream types - matchup, message, matchupSelected, matchupVote, tweet, empty-matchup, empty-news
        public static List<StreamContent> GetBasicStream()
        {
            List<StreamContent> playersStream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                playersStream = GetStreamMatchupsByWeek(0,  gameschedule.GetCurrentWeekID() );
                playersStream.AddRange(GetUserStream(43, false, 0));
                playersStream.AddRange(GetUserStream(1, false, 0));

                //sort everything by date
                playersStream = playersStream.OrderByDescending(str => str.DateCreated).ToList();
            }
            catch (Exception) { }

            return playersStream;
        }

        public static List<StreamContent> GetStreamMatchupsByWeek(int userID, int weekID)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                List<WeeklyMatchups> matchupList = matchup.GetMatchupsByWeek(userID, weekID, true);
                user currentUser = user.Get(userID);

                if (matchupList.Count() > 0)
                {
                    stream = matchupList.Select(usrmtch => new StreamContent
                    {
                        MatchupItem = usrmtch,
                        DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                        ProfileImg = usrmtch.CreatedBy.avatar.imageName,
                        UserName = usrmtch.CreatedBy.userName,
                        FullName = usrmtch.CreatedBy.fullName,
                        ContentType = (!usrmtch.HasVoted && usrmtch.AllowVote) ? "matchup" : "matchupSelected",
                        DateCreated = usrmtch.LastDate,
                        TimeAgo = twitter.GetRelativeTime(usrmtch.DateCreated),
                        Source = "mtchtab",
                        UserProfileImg = (currentUser.userID != 0 ) ? currentUser.avatar.imageName : string.Empty
                    }).OrderByDescending(str => str.DateCreated).ToList();
                }
            }
            catch (Exception)
            {

            }

            return stream;
        }


        public static List<StreamContent> GetPlayerStream(int playerID, int userID, bool quickView)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                //kill the recent list cache
                HttpContext.Current.Cache.Remove("viewed" + userID.ToString());

                //save off that the player was viewed
                if( !quickView )
                    users_view.Add("players", playerID, (userID != 0) ? (int?)userID : null);
                
                stream = GetPlayerStream(nflplayer.Get(playerID), userID, quickView, true, null);
            }
            catch (Exception)
            { }

            return stream;
        }

        public static List<StreamContent> GetPlayerTwitterStream(nflplayer player)
        {
            List<StreamContent> stream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();
            try
            {
                //get all the tweets
                List<Search> playerTweets = twitter.SearchPlayer(player, true);
                stream = new List<StreamContent>();
                if (playerTweets.Count() > 0)
                {
                    stream = playerTweets.Single().Statuses.Select(tweet => new StreamContent
                    {
                        DateTicks = tweet.CreatedAt.Ticks.ToString(),
                        ProfileImg = tweet.User.ProfileImageUrl,
                        UserName = tweet.User.ScreenNameResponse,
                        FullName = tweet.User.Name,
                        ContentType = "tweet",
                        DateCreated = tweet.CreatedAt,
                        TimeAgo = twitter.GetRelativeTime(tweet.CreatedAt, false),
                        PlayerID = player.playerID,
                        Tweet = new TweetContent { ID = tweet.UserID.ToString(), Message = TwitterExtensions.TextAsHtml(tweet.Text) }
                    }).Take(8).ToList();
                }
            }
            catch (Exception) { }

            return stream;
        }

        public static List<StreamContent> GetRandomPlayerStream(nflplayer player)
        {
            List<StreamContent> stream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();
            try
            {
                //get all the tweets
                List<Search> playerTweets = twitter.SearchPlayer(player, true);
                List<StreamContent> newsStream = new List<StreamContent>();
                if (playerTweets.Count() > 0)
                {
                    newsStream = playerTweets.Single().Statuses.Select(tweet => new StreamContent
                    {
                        DateTicks = tweet.CreatedAt.Ticks.ToString(),
                        ProfileImg = tweet.User.ProfileImageUrl,
                        UserName = tweet.User.ScreenName,
                        FullName = tweet.User.Name,
                        ContentType = "tweet",
                        DateCreated = tweet.CreatedAt,
                        TimeAgo = twitter.GetRelativeTime(tweet.CreatedAt),
                        PlayerID = player.playerID,
                        Tweet = new TweetContent { ID = tweet.ID.ToString(), Message = TwitterExtensions.TextAsHtml(tweet.Text) }
                    }).ToList();
                }
                
                //get all the messages
                List<message> msgs = message.GetRecentList(player.playerID, true, null);
                newsStream.AddRange(msgs.Select(msg => new StreamContent
                {
                    MessageItem = msg,
                    DateTicks = msg.dateCreated.Ticks.ToString(),
                    ProfileImg = msg.user.avatar.imageName,
                    UserName = msg.user.userName,
                    FullName = msg.user.fullName,
                    ContentType = "message",
                    DateCreated = msg.dateCreated,
                    PlayerID = player.playerID,
                    TimeAgo = twitter.GetRelativeTime(msg.dateCreated)
                }).ToList());

                newsStream = newsStream.OrderByDescending(str => str.DateCreated).Take(5).ToList();
                stream.AddRange(newsStream);

                //sort everything by date
                stream = stream.OrderByDescending(str => str.DateCreated).ToList();
                for (int i = 0; i < stream.Count; i++)
                {
                    if (i > 4)
                        stream[i].CssClass = "over-limit";
                }
            }
            catch (Exception) { }

            return stream;
        }

        public static List<StreamContent> GetPlayerStream(nflplayer player, int? userID, bool quickView, bool futureTimeline, DateTime? fromDate = null)
        {
            List<StreamContent> stream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();
            try
            {
                //get all the matchups
                List<WeeklyMatchups> usrMatchups = matchup.GetRecentList(player.playerID, userID, futureTimeline, fromDate);
                if (quickView)
                    usrMatchups = usrMatchups.Take(5).ToList();

                user currentUser = (userID.HasValue) ? user.Get(userID.Value) : new user();

                stream = usrMatchups.Select(usrmtch => new StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.CreatedBy.avatar.imageName,
                    UserName = usrmtch.CreatedBy.userName,
                    FullName = usrmtch.CreatedBy.fullName,
                    ContentType = GetMatchupContentType(usrmtch),
                    DateCreated = usrmtch.DateCreated,
                    TimeAgo = twitter.GetRelativeTime(usrmtch.DateCreated),
                    PlayerID = player.playerID,
                    UserProfileImg = (currentUser.userID != 0 ) ? currentUser.avatar.imageName : string.Empty
                }).ToList();

                if (stream.Count() <= 0)
                    stream.Add(new StreamContent { ContentType = "empty-matchup" });

                stream.AddRange(GetNewsStream(player, quickView, futureTimeline, userID, fromDate));
                
                //sort everything by date
                stream = stream.OrderByDescending(str => str.DateCreated).ToList();
                for (int i = 0; i < stream.Count; i++)
                {
                    if (i > 4)
                        stream[i].CssClass = "over-limit";
                }
            }
            catch (Exception ex) 
            {
                string msg = ex.Message;
            }

            return stream;
        }

        public static List<StreamContent> GetNewsStream(nflplayer player, bool quickView, bool futureTimeline, int? userID, DateTime? fromDate = null)
        {
             //get all the tweets
            List<StreamContent> newsStream = new List<StreamContent>();

            user currentUser = (userID.HasValue) ? user.Get(userID.Value) : new user();


            //get all the messages
            List<message> msgs = message.GetRecentList(player.playerID, futureTimeline, fromDate);
            newsStream.AddRange(msgs.Select(msg => new StreamContent
            {
                MessageItem = msg,
                DateTicks = msg.dateCreated.Ticks.ToString(),
                ProfileImg = msg.user.avatar.imageName,
                UserName = msg.user.userName,
                FullName = msg.user.fullName,
                ContentType = "message",
                DateCreated = msg.dateCreated,
                PlayerID = player.playerID,
                TimeAgo = twitter.GetRelativeTime(msg.dateCreated),
                UserProfileImg = (currentUser.userID != 0 ) ? currentUser.avatar.imageName : string.Empty
            }).ToList());

            if (quickView)
                newsStream = newsStream.OrderByDescending(str => str.DateCreated).Take(5).ToList();

            if (newsStream.Count <= 0)
                newsStream.Add(new StreamContent { ContentType = "empty-news" });

            return newsStream.OrderByDescending(str => str.DateCreated).ToList();
        }

        public static string GetStreamItemClass(message messageItem)
        {
            string css = string.Empty;

            if (messageItem.IsReply)
                css = "reply-message";

            if (messageItem.IsParent)
                css = "parent-message";

            return css;
        }

        public static List<StreamContent> GetDetails(int messageID)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                List<message> msgs = message.GetDetails(messageID);
                stream.AddRange(msgs.Select(msg => new StreamContent
                {
                    MessageItem = msg,
                    DateTicks = msg.dateCreated.Ticks.ToString(),
                    ProfileImg = msg.user.avatar.imageName,
                    UserName = msg.user.userName,
                    FullName = msg.user.fullName,
                    ContentType = "message",
                    DateCreated = msg.dateCreated,
                    CssClass = GetStreamItemClass( msg ),
                    TimeAgo = twitter.GetRelativeTime(msg.dateCreated),
                    Source = "Mention"
                }).ToList());

                if (stream.Count <= 0)
                    stream.Add(new StreamContent { ContentType = "empty-messages" });

                stream = stream.OrderBy(str => str.DateCreated).ToList();
            }
            catch (Exception)
            { }

            return stream;
        }

        public static List<StreamContent> GetUserStream(int userID, bool quickView, int currentUserID, DateTime? fromDate = null)
        {
            List<StreamContent> stream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();
            try
            {
                //kill the recent list cache
                HttpContext.Current.Cache.Remove("viewed" + userID.ToString());

                //save off that the player was viewed
                users_view.Add("users", userID, (currentUserID != 0) ? (int?)currentUserID : null);
                user currentUser = user.Get(currentUserID);

                List<message> msgs = message.GetUserRecentList(userID);
                stream.AddRange(msgs.Select(msg => new StreamContent
                {
                    MessageItem = msg,
                    DateTicks = msg.dateCreated.Ticks.ToString(),
                    ProfileImg = msg.user.avatar.imageName,
                    UserName = msg.user.userName,
                    FullName = msg.user.fullName,
                    ContentType = "message",
                    DateCreated = msg.dateCreated,
                    CssClass = GetStreamItemClass(msg),
                    TimeAgo = twitter.GetRelativeTime(msg.dateCreated),
                    UserProfileImg = (currentUser.userID != 0) ? currentUser.avatar.imageName : string.Empty
                }).ToList());

                //add user created matchups
                stream.AddRange(matchup.GetAllUserMatchups(userID, 15, currentUserID));

                if (stream.Count <= 0)
                    stream.Add(new StreamContent { ContentType = "empty-messages" });

                stream = stream.OrderByDescending(str => str.DateCreated).Take(50).ToList();
            }
            catch( Exception )
            {}

            return stream;
        }

        public async static Task<int> GetUpdateStreamCount( int userID, string timeTicks )
        {
            int count = 0;

            try
            {
                DateTime lastDate = new DateTime(Convert.ToInt64(timeTicks));
                List<StreamContent> timeStream = await GetStream(userID, true, lastDate);
                timeStream = timeStream.Where( ts => ts.ContentType != "empty-news" && ts.ContentType != "empty-matchup" && ts.ContentType != "empty-messages").ToList();
                count = timeStream.Count();
            }        
            catch (Exception)
            {
                count = 0;
            }

            return count;
        }

        //gets the latest stream for a user - main function for stream
        public async static Task<List<StreamContent>> GetStream(int userID, bool futureTimeline, DateTime? fromDate = null)
        {
            List<StreamContent> stream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                bool includeFromDate = (fromDate.HasValue) ? true : false;
                //if no date, grab everything from the last 60 days
                if( !fromDate.HasValue )
                    fromDate = DateTime.UtcNow.GetEasternTime().AddDays(-280);

                user currentUser = user.Get(userID);

                //removing the follow concept for now
                // List<users_account> followList = user.GetFollowAll(userID);
              //  List<int> followPlayers = followList.Where(usaccnt => usaccnt.accounttype.accountName == "players").Select(usaccnt => usaccnt.accountID).ToList();
              //  List<int> followUsers = followList.Where(usaccnt => usaccnt.accounttype.accountName == "users").Select(usaccnt => usaccnt.accountID).ToList();
                List<int> followPlayers = new List<int>();
                List<int> followUsers = new List<int>();

                //always include the user too, unless in the future which have been added already
                if (!futureTimeline || !includeFromDate)
                    followUsers.Add(userID);

                //gets the matchups for the stream
                List<WeeklyMatchups> usrMatchups = await matchup.GetList(userID, fromDate.Value, futureTimeline);
                stream = usrMatchups.Select(usrmtch => new StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.CreatedBy.avatar.imageName,
                    UserProfileImg = (currentUser.userID != 0 ) ? currentUser.avatar.imageName : string.Empty,
                    UserName = usrmtch.CreatedBy.userName,
                    FullName = usrmtch.CreatedBy.fullName,
                    ContentType = GetMatchupContentType( usrmtch ),
                    DateCreated = usrmtch.LastDate
                }).ToList();
                

                //get all the user messages for following a user or player
                List<message> msgs = message.GetRecentList(userID, followUsers, followPlayers, futureTimeline, fromDate.Value);
                stream.AddRange( msgs.Select(msg => new StreamContent
                {
                    MessageItem = msg,
                    DateTicks = msg.dateCreated.Ticks.ToString(),
                    ProfileImg = msg.user.avatar.imageName,
                    UserName = msg.user.userName,
                    FullName = msg.user.fullName,
                    ContentType = "message",
                    DateCreated = msg.dateCreated,
                    UserProfileImg = (currentUser.userID != 0) ? currentUser.avatar.imageName : string.Empty,
                    TimeAgo = twitter.GetRelativeTime(msg.dateCreated)
                }).ToList());
                
                //sort everything by date
                stream = (futureTimeline) ? stream.OrderByDescending(str => str.DateCreated).Take(50).ToList() : stream.OrderByDescending(str => str.DateCreated).Take(20).ToList();
            }
            catch (Exception) { }

            return stream;
        }

        public static List<StreamContent> GetMatchupVoteStream(List<WeeklyMatchups> usrMatchups, int userID, List<int> followIDs)
        {
            List<StreamContent> stream = new List<StreamContent>();

            var userCreatedMatchups = usrMatchups.Where(urmtch => urmtch.CreatedBy.userID == userID && urmtch.NoVotes == false);

            foreach (WeeklyMatchups matchup in userCreatedMatchups.ToList())
            {
                foreach( UserVoteData userData in matchup.Coaches ) 
                {
                    if( followIDs.Contains( userData.userID ) )
                    {
                        stream.Add(ConvertToStream(matchup, matchup.Player1.PlayerID, true, userID));
                        break;
                    }
                }
            }

            return stream;
        }

        public static int GetID(string statusName, string componentName)
        {
            int statusID = 0;

            CoachCueDataContext db = new CoachCueDataContext();
            var stat = db.status.Where(st => st.statusName.ToLower() == statusName.ToLower() && st.component.componentName == componentName);

            if (stat.Count() > 0)
                statusID = stat.FirstOrDefault().statusID;

            return statusID;
        }

        //converts the base item to a common stream type
        public static StreamContent ConvertToStream(WeeklyMatchups matchup, int playerID, bool showVotes, int currentUser)
        {
            StreamContent streamItem = null;
            user currentUserItem = new user();

            if (currentUser != 0)
                currentUserItem = user.Get(currentUser);

            streamItem = new StreamContent
            {
                MatchupItem = matchup,
                ContentType = (!matchup.HasVoted && matchup.AllowVote) ? "matchup" : "matchupSelected",
                DateTicks = matchup.DateCreated.Ticks.ToString(),
                ProfileImg = matchup.CreatedBy.avatar.imageName,
                UserName = matchup.CreatedBy.userName,
                FullName = matchup.CreatedBy.fullName,
                DateCreated = matchup.DateCreated,
                PlayerID = playerID,
                TimeAgo = twitter.GetRelativeTime(matchup.DateCreated),
                UserProfileImg = (currentUserItem.userID != 0 ) ? currentUserItem.avatar.imageName : string.Empty
            };

            return streamItem;
        }

        public static StreamContent ConvertToStream(message msg, string type, int userID)
        {
            user currentUser = new user();
            if( userID != 0 )
                currentUser = user.Get(userID);

            StreamContent streamItem = new StreamContent
            {
                MessageItem = msg,
                DateTicks = msg.dateCreated.Ticks.ToString(),
                ProfileImg = msg.user.avatar.imageName,
                UserName = msg.user.userName,
                FullName = msg.user.fullName,
                ContentType = "message",
                DateCreated = msg.dateCreated,
                TimeAgo = twitter.GetRelativeTime( msg.dateCreated ),
                HideActions = (type == "matchup") ? true : false,
                UserProfileImg = (currentUser.userID != 0 ) ? currentUser.avatar.imageName : string.Empty
            };

            return streamItem;
        }

        public static string GetMatchupContentType(WeeklyMatchups matchup)
        { 
            string contentType = "matchupSelected";

            if (string.IsNullOrEmpty(matchup.UserSelectedPlayer))
            {
                //make sure the game hasn't passed too
                if (DateTime.UtcNow.GetEasternTime() < matchup.GameDate)
                    contentType = "matchup";
            }
            
            return contentType;
        }
    }

    public class PlayerStream
    {
        public bool BuildPlayerHeader { get; set; }
        public nflplayer Player { get; set; }
        public List<StreamContent> StreamItems { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string LastUpdateTicks{
            get
            {
                return (this.LastUpdate.HasValue) ? this.LastUpdate.Value.Ticks.ToString() : string.Empty;
            }
        }
    }

    public class PlayerPopover
    {
        public List<StreamContent> TwitterContent { get; set; }
        public VotedPlayersByPosition Voting { get; set; }

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
        public message MessageItem { get; set; }
        public WeeklyMatchups MatchupItem { get; set; }
        public TweetContent Tweet { get; set; }
        public int PlayerID { get; set; }
        public string CssClass { get; set; }
        public nflplayer Player { get; set; }
        public string Source { get; set; }
        public bool HideActions { get; set; }
    }
}
