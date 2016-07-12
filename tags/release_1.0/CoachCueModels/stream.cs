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

namespace CoachCue.Model
{
    public class stream
    {
        //stream types - matchup, message, matchupSelected, matchupVote, tweet, empty-matchup, empty-news
        public static List<PlayerStream> GetPlayersStream(List<nflplayer> players)
        {
            List<PlayerStream> playersStream = new List<PlayerStream>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                playersStream = players.Select(ply => new PlayerStream
                {
                    BuildPlayerHeader = true,
                    Player = ply,
                    StreamItems = GetPlayerStream(ply, null, true)
                }).ToList();

                //** maybe for future release - also need to get any direct user references even if user not following
                foreach (PlayerStream player in playersStream)
                {
                    if (player.StreamItems.Count > 0)
                        player.LastUpdate = player.StreamItems[0].DateCreated;
                }

                //sort everything by date
                playersStream = playersStream.OrderByDescending(str => str.LastUpdate).ToList();
            }
            catch (Exception) { }

            return playersStream;
        }
        
        public static List<PlayerStream> GetPlayersStream(int userID, DateTime? fromDate = null)
        {
            List<PlayerStream> playersStream = new List<PlayerStream>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                List<FollowData> following = user.GetAccounts(userID);
                
                //adding players that a followed user has posted about recently
                List<FollowData> followingFromUsers = message.GetRecentMessagePlayerList(userID);
                foreach (FollowData followingFromUser in followingFromUsers)
                {
                    FollowData uniqueFollow = following.Where(flw => flw.player.playerID == followingFromUser.player.playerID).FirstOrDefault();
                    if (uniqueFollow == null)
                        following.Add(followingFromUser);
                }

                playersStream = following.Select(flw => new PlayerStream
                {
                     BuildPlayerHeader = true,
                     Player = flw.player,
                     StreamItems = GetPlayerStream( flw.player, userID, true )
                }).ToList();

                //** maybe for future release - also need to get any direct user references even if user not following
                foreach (PlayerStream player in playersStream)
                {
                    if( player.StreamItems.Count > 0) 
                        player.LastUpdate = player.StreamItems[0].DateCreated;
                }

                //sort everything by date
                playersStream = playersStream.OrderByDescending(str => str.LastUpdate).ToList();
            }
            catch (Exception) { }

            return playersStream;
        }

        public static List<StreamContent> GetPlayerStream(int playerID, int userID, bool quickView)
        {
            List<StreamContent> stream = new List<StreamContent>();

            try
            {
                //save off that the player was viewed
                if( !quickView )
                    users_view.Add("players", playerID, (userID != 0) ? (int?)userID : null);
                
                stream = GetPlayerStream(nflplayer.Get(playerID), userID, quickView, null);
            }
            catch (Exception)
            { }

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
                        UserName = tweet.User.Identifier.ScreenName,
                        FullName = tweet.User.Name,
                        ContentType = "tweet",
                        DateCreated = tweet.CreatedAt,
                        TimeAgo = twitter.GetRelativeTime(tweet.CreatedAt),
                        PlayerID = player.playerID,
                        Tweet = new TweetContent { ID = tweet.ID, Message = TwitterExtensions.TextAsHtml(tweet.Text) }
                    }).ToList();
                }
                
                //get all the messages
                List<message> msgs = message.GetRecentList(player.playerID, null);
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

        public static List<StreamContent> GetPlayerStream(nflplayer player, int? userID, bool quickView, DateTime? fromDate = null)
        {
            List<StreamContent> stream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();
            try
            {
                //get all the matchups
                List<WeeklyMatchups> usrMatchups = matchup.GetRecentList(player.playerID, userID, fromDate);
                if (quickView)
                    usrMatchups = usrMatchups.Take(5).ToList();

                stream = usrMatchups.Select(usrmtch => new StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.CreatedBy.avatar.imageName,
                    UserName = usrmtch.CreatedBy.userName,
                    FullName = usrmtch.CreatedBy.fullName,
                    ContentType = getMatchupContentType(usrmtch),
                    DateCreated = usrmtch.DateCreated,
                    TimeAgo = twitter.GetRelativeTime(usrmtch.DateCreated),
                    PlayerID = player.playerID
                }).ToList();

                if (stream.Count() <= 0)
                    stream.Add(new StreamContent { ContentType = "empty-matchup" });

                //get all the tweets
                List<Search> playerTweets = twitter.SearchPlayer(player, true);
                List<StreamContent> newsStream = new List<StreamContent>();
                if (playerTweets.Count() > 0)
                {
                    newsStream = playerTweets.Single().Statuses.Select(tweet => new StreamContent
                    {
                        DateTicks = tweet.CreatedAt.Ticks.ToString(),
                        ProfileImg = tweet.User.ProfileImageUrl,
                        UserName = tweet.User.Identifier.ScreenName,
                        FullName = tweet.User.Name,
                        ContentType = "tweet",
                        DateCreated = tweet.CreatedAt,
                        TimeAgo = twitter.GetRelativeTime(tweet.CreatedAt),
                        PlayerID = player.playerID,
                        Tweet = new TweetContent { ID = tweet.ID, Message = TwitterExtensions.TextAsHtml(tweet.Text) }
                    }).ToList();
                }

                //get all the messages
                List<message> msgs = message.GetRecentList(player.playerID, fromDate);
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

                if (quickView)
                    newsStream = newsStream.OrderByDescending(str => str.DateCreated).Take(5).ToList();

                if (newsStream.Count <= 0)
                    newsStream.Add(new StreamContent { ContentType = "empty-news" });

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
                    TimeAgo = twitter.GetRelativeTime(msg.dateCreated)
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
                //save off that the player was viewed
                users_view.Add("users", userID, (currentUserID != 0) ? (int?)currentUserID : null);

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
                    TimeAgo = twitter.GetRelativeTime(msg.dateCreated)
                }).ToList());

                if (stream.Count <= 0)
                    stream.Add(new StreamContent { ContentType = "empty-messages" });

                stream = stream.OrderByDescending(str => str.DateCreated).ToList();
            }
            catch( Exception )
            {}

            return stream;
        }

        public static int GetUpdateStreamCount( int userID, List<int> playerIDs, List<string> timeTicks )
        {
            int count = 0;

            try
            {
                int matchCount = 0;
                foreach (int playerID in playerIDs)
                {
                    DateTime lastDate = new DateTime(Convert.ToInt64(timeTicks[matchCount]));

                    List<WeeklyMatchups> usrMatchups = matchup.GetRecentList(playerID, userID, lastDate);
                    List<message> msgs = message.GetRecentList(playerID, lastDate);
                    //? add the twitter feed ?

                    if (usrMatchups.Count > 0 || msgs.Count > 0)
                    {
                        count = usrMatchups.Count + msgs.Count;
                        break;
                    }
                    matchCount++;
                }
            }
            catch (Exception)
            {
                count = 0;
            }

            return count;
        }

        public static List<StreamContent> GetStream(int userID, DateTime? fromDate = null)
        {
            List<StreamContent> stream = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                List<int> followIDs = user.GetFollowUsers(userID);

                //have to add , plus votes on matchups as they come in
                //also after you vote just show the results of it in the stream

                //get all the matchups
                List<WeeklyMatchups> usrMatchups = matchup.GetRecentList(userID, followIDs, fromDate);
                stream = usrMatchups.Select(usrmtch => new StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.CreatedBy.avatar.imageName,
                    UserName = usrmtch.CreatedBy.userName,
                    FullName = usrmtch.CreatedBy.fullName,
                    ContentType = getMatchupContentType( usrmtch ),
                    DateCreated = usrmtch.DateCreated
                }).ToList();

                //get the matchups that were voted on by people the user follows
                //stream.AddRange(GetMatchupVoteStream(usrMatchups, userID, followIDs));

                //get all the messages
                List<message> msgs = message.GetRecentList(userID, followIDs, fromDate);
                stream.AddRange( msgs.Select(msg => new StreamContent
                {
                    MessageItem = msg,
                    DateTicks = msg.dateCreated.Ticks.ToString(),
                    ProfileImg = msg.user.avatar.imageName,
                    UserName = msg.user.userName,
                    FullName = msg.user.fullName,
                    ContentType = "message",
                    DateCreated = msg.dateCreated
                }).ToList());

                //sort everything by date
                stream = stream.OrderByDescending(str => str.DateCreated).ToList();
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
                        stream.Add( ConvertToStream( matchup, matchup.Player1.PlayerID, true ) );
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
        public static StreamContent ConvertToStream(WeeklyMatchups matchup, int playerID, bool showVotes)
        {
            StreamContent streamItem = null;

            streamItem = new StreamContent
            {
                MatchupItem = matchup,
                DateTicks = matchup.DateCreated.Ticks.ToString(),
                ProfileImg = matchup.CreatedBy.avatar.imageName,
                UserName = matchup.CreatedBy.userName,
                FullName = matchup.CreatedBy.fullName,
                DateCreated = matchup.DateCreated,
                PlayerID = playerID,
                TimeAgo = twitter.GetRelativeTime(matchup.DateCreated)
            };

            streamItem.ContentType = (!showVotes) ? "matchup" : "matchupSelected";
            return streamItem;
        }

        public static StreamContent ConvertToStream(message msg, int playerID)
        {
            StreamContent streamItem = new StreamContent
            {
                MessageItem = msg,
                DateTicks = msg.dateCreated.Ticks.ToString(),
                ProfileImg = msg.user.avatar.imageName,
                UserName = msg.user.userName,
                FullName = msg.user.fullName,
                ContentType = "message",
                DateCreated = msg.dateCreated,
                PlayerID = playerID,
                TimeAgo = twitter.GetRelativeTime( msg.dateCreated )
            };

            return streamItem;
        }

        private static string getMatchupContentType(WeeklyMatchups matchup)
        { 
            string contentType = "matchupSelected";

            if (string.IsNullOrEmpty(matchup.UserSelectedPlayer))
            {
                //make sure the game hasn't passed too
                if (DateTime.Now < matchup.GameDate)
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

    public class StreamContent
    {
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
        public AccountData PlayerAccount {
            get
            {
                AccountData account = new AccountData();

                if (this.MessageItem != null)
                {
                    account.accountType = "players";
                    account.accountID = this.MessageItem.nflplayer.playerID;
                    account.fullName = this.MessageItem.nflplayer.fullName;
                    account.following = (this.MessageItem.nflplayer.isFollowing) ? 1 : 0;
                    account.teamSlug = this.MessageItem.nflplayer.nflteam.teamSlug;
                    account.position = this.MessageItem.nflplayer.position.positionName;
                    account.profileImg = this.MessageItem.nflplayer.profilePic;
                }

                return account;
            }
        }
        public PlayerStream PlayerHeader
        {
            get
            {
                PlayerStream player = new PlayerStream();

                player.BuildPlayerHeader = true;
                player.LastUpdate = this.DateCreated;
                player.Player = this.MessageItem.nflplayer;
                
                return player;
            }
        }
    }
}
