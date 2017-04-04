using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using LinqToTwitter;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Configuration;
using CoachCue.Service;
using CoachCue.Models;

namespace CoachCue.Model
{
    public class twitter
    {
        public static string Search(string term)
        {
            var tweetJson = string.Empty;

            var baseTwitterApiUrl = "http://search.twitter.com/search.json?callback=?&rpp=100&q=";
            var url = baseTwitterApiUrl + term;

            //get the json from twitter
            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 2000;
            using (var response = webRequest.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var receiveStream = response.GetResponseStream();
                    if (receiveStream != null)
                    {
                        var stream = new StreamReader(receiveStream);
                        tweetJson = stream.ReadToEnd();
                    }
                }
            }

            return tweetJson;
        }

        public static string GetPlayer(string username)
        {
            //if (HttpContext.Current.Cache[cacheID] != null)
            //    centerStaff = (List<lw_staff>)HttpContext.Current.Cache[cacheID];
            //else run it and add the cache
            //int hours = Convert.ToInt16(ConfigurationManager.AppSettings["CacheTime"].ToString());
            //HttpContext.Current.Cache.Insert(cacheID, centerStaff, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(hours, 0, 0));


            var tweetJson = string.Empty;

            var baseTwitterApiUrl = "https://api.twitter.com/1/statuses/user_timeline.json?include_entities=true&include_rts=true&count=10&screen_name=";
            var url = baseTwitterApiUrl + username;

            //get the json from twitter
            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 2000;
            //webRequest.Headers.Add("referrer", "http://CoachCue");
            using (var response = webRequest.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var receiveStream = response.GetResponseStream();
                    if (receiveStream != null)
                    {
                        var stream = new StreamReader(receiveStream);
                        tweetJson = stream.ReadToEnd();
                    }
                }
            }

            return tweetJson;
        }

        public static TwitterContext SingleUserLogin(){

            var auth = new MvcAuthorizer
            {
                CredentialStore = new SessionStateCredentialStore
                {
                    ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"],
                    OAuthToken = "385027108-fcLgoLl0oKHup5aBy2f3fQQhwEMBzpgi2L6TX57C",
                    OAuthTokenSecret = "fSNsKC7vzf4mT3CPoqsPrFmuuMrT1PU9LlS1CBIfE"
                }
            };

            TwitterContext twitterCtx = new TwitterContext(auth);
            return twitterCtx;
        }

        //add caching to this search
        public static async Task<List<Search>> SearchPlayer(Player player, bool loggedIn, DateTime? fromDate = null)
        {
            var tweetJson = string.Empty;
            List<Search> searchResults = new List<Search>();

            try
            {
                //need to make the search relevant- search for tweets from certain people that include the 
                //players name or the players team and last name
                //the from accounts should be tied to the players team
                string dateTicks = (fromDate.HasValue) ? fromDate.Value.Ticks.ToString() : "";

                string cacheID = "playerTweets" + player.Id + dateTicks;
                if (HttpContext.Current.Cache[cacheID] != null)
                    searchResults = (List<Search>)HttpContext.Current.Cache[cacheID];
                else
                {
                    string searchFilters = await getSearchFilters(player);
                    string cleanName = player.LastName.Replace(".", "").Replace("-", " ").Replace("'", " ");
                    string query = (string.IsNullOrEmpty(searchFilters)) ? cleanName + " AND " : cleanName + searchFilters + " AND ";

                    //always include the rotoworld fantasy football account
                    player.BeatWriters.Add("Rotoworld_FB");

                    if (player.BeatWriters.Count > 0)
                    {
                        for (int i = 0; i < player.BeatWriters.Count; i++)
                        {
                            query += "from:" + player.BeatWriters[i] + (i < player.BeatWriters.Count - 1 ? " OR " : "");
                        }

                        TwitterContext twitterCtx = SingleUserLogin();
                        var queryResults = from search in twitterCtx.Search
                                           where search.Type == SearchType.Search &&
                                           search.Query == query
                                           select search;

                        searchResults = queryResults.Take(50).ToList();

                        HttpContext.Current.Cache.Insert(cacheID, searchResults, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 5, 0));
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            return searchResults;
        }

        public static List<TwitterEntry> SearchAtom(string value)
        {
            // create the empty TwitterEntry List
            List<TwitterEntry> twitterEntries = new List<TwitterEntry>();

            // Get the Twitter feed
            // The WebClient class is located in the System.Net namespace; it 
            // provides methods for sending data to and from uri resources.
            WebClient web = new WebClient();

            string xml = web.DownloadString(String.Format("http://search.twitter.com/search.atom?q={0}", value));

            // Declare the namespaces that we'll need to select the different
            // elements using LINQ..
            XNamespace ns = "http://www.w3.org/2005/Atom";
            XNamespace nsTwitter = "http://api.twitter.com/";

            // Select the entry elements
            var entries = from elem in XDocument.Parse(xml).Descendants(ns + "entry")
                          select elem;

            // Create an instance of TwitterEntry for each entry eleent and add it to the list
            foreach (var entry in entries)
            {
                TwitterEntry twitterEntry = new TwitterEntry(entry.Element(ns + "id").Value, entry.Element(ns + "published").Value,
                entry.Element(ns + "title").Value,
                entry.Element(ns + "content").Value,
                entry.Element(ns + "updated").Value, entry.Element(nsTwitter + "source").Value,
                entry.Element(ns + "author").Element(ns + "name").Value,
                entry.Element(ns + "author").Element(ns + "uri").Value,
                getTimeAgo(Convert.ToDateTime(entry.Element(ns + "updated").Value)));
                twitterEntries.Add(twitterEntry);
            }

            // Sort the list
            twitterEntries.Sort();

            // Return the list
            return twitterEntries;
        }

        public static List<TwitterEntry> GetList(string listName, string searchTerm)
        {
            // create the empty TwitterEntry List
            List<TwitterEntry> twitterEntries = new List<TwitterEntry>();

            // Get the Twitter feed
            // The WebClient class is located in the System.Net namespace; it 
            // provides methods for sending data to and from uri resources.
            WebClient web = new WebClient();

            string xml = web.DownloadString(String.Format("https://api.twitter.com/1/lists/statuses.atom?slug={0}&owner_screen_name=coachcue&count=1&page=1&include_entities=true", listName));

            // Declare the namespaces that we'll need to select the different
            // elements using LINQ..
            XNamespace ns = "http://www.w3.org/2005/Atom";
            XNamespace nsTwitter = "http://api.twitter.com";

            // Select the entry elements
            var entries = from elem in XDocument.Parse(xml).Descendants(ns + "entry")
                          where elem.Element(ns + "content").Value.Contains(searchTerm) || elem.Element(ns + "title").Value.Contains(searchTerm)
                          select elem;

            // Create an instance of TwitterEntry for each entry eleent and add it to the list
            foreach (var entry in entries)
            {
                TwitterEntry twitterEntry = new TwitterEntry(entry.Element(ns + "id").Value, entry.Element(ns + "published").Value,
                entry.Element(ns + "title").Value,
                HtmlFormatTweet(entry.Element(ns + "content").Value),
                entry.Element(ns + "updated").Value, entry.Element(nsTwitter + "source").Value,
                entry.Element(ns + "author").Element(ns + "name").Value,
                entry.Element(ns + "author").Element(ns + "uri").Value,
                getTimeAgo(Convert.ToDateTime(entry.Element(ns + "updated").Value)));
                twitterEntries.Add(twitterEntry);
            }

            // Sort the list
            twitterEntries.Sort();
            twitterEntries.Reverse();

            return twitterEntries;
        }

        private static async Task<string> getSearchFilters(Player player)
        {
            string filters = string.Empty;

            //exclude all other players with the same first name
            List<string> names = await PlayerService.GetPlayerNotConditions(player.FirstName, player.LastName);

            //dont do more than 6 or the search will fail
            int count = 0;
            foreach (string name in names)
            {
                if (count <= 5)
                    filters += " -" + name;

                count++;
            }

            return filters;
        }

        private static string getParentHtml(nflplayer player, bool loggedIn)
        {
            string plyHtml = string.Empty;
            string username = string.Empty;
            string user = string.Empty;
            string playerTweets = string.Empty;
            string twitterLink = "#";

            //default the profile image to the team
            string profile = "/assets/img/teams/" + player.nflteam.teamSlug + ".jpg";

            if (player != null)
            {
                user = player.fullName;

                if (player.twitteraccount != null)
                {
                    profile = player.twitteraccount.profileImageUrl;
                    username = "@" + player.twitteraccount.twitterUsername;
                    user = player.twitteraccount.twitterName;
                    twitterLink = "http://twitter.com/" + player.twitteraccount.twitterUsername;
                    playerTweets = "<div class='player-loader'></div><a rel='tooltip' title='Show Player Tweets' data-user='" + player.twitteraccount.twitterName + "' data-username='" + player.twitteraccount.twitterUsername + "' class='show-tweets tltip' href='#'><i class='tweets'></i></a>";
                }

                string followLink = (loggedIn) ? "<a data-account='" + player.playerID + "' class='btn btn-danger unfollow btn-mini action' href='#'><i class='icon-minus-sign icon-white'></i>Unfollow</a>" : string.Empty;

                plyHtml = HttpContext.Current.Server.UrlEncode("<div class='item parent'><div class='content'><div class='action'>" + playerTweets + "<a class='close-item' href='#'><i class='icon-chevron-up icon-white'></i></a></div><div class='content-header'><a class='username' target='_blank' href='" + twitterLink + "'>" + user + "</a><span class='user'>" + username + "</span></div>" + followLink + "<img class='avatar' src='" + profile + "' /><p class='bio'>" + player.position.positionName + " " + player.nflteam.teamName + "</p></div></div>");
            }

            return plyHtml;
        }

        public static DateTime GetDate(string timeValue)
        {
            const string format = "ddd, dd MMM yyyy HH:mm:ss zzzz";
            return DateTime.ParseExact(timeValue, format, CultureInfo.InvariantCulture);
        }

        public static string GetRelativeTime( string timeValue )
        {
            DateTime parsedDate = GetDate(timeValue);
            return GetRelativeTime(parsedDate);
        }

        public static string GetRelativeTime(DateTime datePosted, bool useRelative = true)
        {
            string relativeTime = string.Empty;

            DateTime relativeTo = (useRelative) ? DateTime.UtcNow.GetEasternTime() : DateTime.Now;
            TimeSpan deltaMinutes = relativeTo.Subtract(datePosted);
            var delta = deltaMinutes.TotalSeconds;

            if (delta < 60)
            {
                relativeTime =  Math.Truncate(delta).ToString() + " s";
            }
            else if (delta < 120)
            {
                relativeTime = "1 m";
            }
            else if (delta < (60 * 60))
            {
                relativeTime = Math.Truncate((delta / 60)).ToString() + " m";
            }
            else if (delta < (120 * 60))
            {
                relativeTime = "1 h";
            }
            else if (delta < (24 * 60 * 60))
            {
                relativeTime = Math.Truncate((delta / 3600)).ToString() + " h";
            }
            else if (delta < (48 * 60 * 60))
            {
                relativeTime = "1 d";
            }
            else
            {
                relativeTime = Math.Truncate((delta / 86400)).ToString() + " d";
            }

            return relativeTime;
        }

        private static string getTimeAgo(DateTime dateCreated)
        {
            string tweetTime = string.Empty;

            TimeSpan timeDiff = DateTime.UtcNow.GetEasternTime().Subtract(dateCreated);
            if (timeDiff.Days > 0) // more than 24hrs so display date
                tweetTime = dateCreated.Day.ToString() + " " + String.Format("{0:MMMM}", dateCreated).ToString();
            else if (timeDiff.Hours == 0)
                tweetTime = timeDiff.Minutes.ToString() + " minutes ago";
            else
                tweetTime = timeDiff.Hours.ToString() + " hours ago";

            return tweetTime;
        }

        /// <summary>
        /// Returns the status text with HTML links to users, urls, and hashtags.
        /// </summary>
        /// <returns></returns>
        protected static string HtmlFormatTweet(string item)
        {
            string newPost = item;// item.Replace(String.Format("{0}:", ProfileName), String.Empty);

            List<KeyValuePair<string, string>> regExRules = new List<KeyValuePair<string, string>>();
            regExRules.Add(new KeyValuePair<string, string>(@"(http:\/\/([\w.]+\/?)\S*)", "<a href=\"$1\" target=\"_blank\">$1</a>"));
            regExRules.Add(new KeyValuePair<string, string>("(@\\w+)", "<a href=\"http://twitter.com/$1\" target=\"_blank\">$1</a> "));
            regExRules.Add(new KeyValuePair<string, string>("(#)(\\w+)", "<a href=\"http://search.twitter.com/search?q=$2\" target=\"_blank\">$1$2</a>"));

            foreach (var regExRule in regExRules)
            {
                Regex urlregex = new Regex(regExRule.Key, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                newPost = urlregex.Replace(newPost, regExRule.Value);
            }

            return newPost;
        }
    }

    /// <summary>
    /// Extends the LinqToTwitter Library
    /// </summary>
    public static class TwitterExtensions
    {
        private static readonly Regex _parseUrls = new Regex("\\b(([\\w-]+://?|www[.])[^\\s()<>]+(?:\\([\\w\\d]+\\)|([^\\p{P}\\s]|/)))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _parseMentions = new Regex("(^|\\W)@([A-Za-z0-9_]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _parseHashtags = new Regex("[#]+[A-Za-z0-9-_]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Parse Status Text to HTML equivalent
        /// </summary>
        /// <param name="status">The LinqToTwitter <see cref="Status"/></param>
        /// <returns>Formatted HTML string</returns>
        public static string TextAsHtml(string status)
        {
            string tweetText = status;

            if (!String.IsNullOrEmpty(tweetText))
            {
                // Replace URLs
                foreach (var urlMatch in _parseUrls.Matches(tweetText))
                {
                    Match match = (Match)urlMatch;
                    tweetText = tweetText.Replace(match.Value, String.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>", match.Value));
                }

                // Replace Mentions
                foreach (var mentionMatch in _parseMentions.Matches(tweetText))
                {
                    Match match = (Match)mentionMatch;
                    if (match.Groups.Count == 3)
                    {
                        string value = match.Groups[2].Value;
                        string text = "@" + value;
                        tweetText = tweetText.Replace(text, String.Format("<a href=\"http://twitter.com/{0}\" target=\"_blank\">{1}</a>", value, text));
                    }
                }

                // Replace Hash Tags
                foreach (var hashMatch in _parseHashtags.Matches(tweetText))
                {
                    Match match = (Match)hashMatch;
                    string query = Uri.EscapeDataString(match.Value);
                    tweetText = tweetText.Replace(match.Value, String.Format("<a href=\"http://search.twitter.com/search?q={0}\" target=\"_blank\">{1}</a>", query, match.Value));
                }
            }

            return tweetText;
        }
    }

    public class TwitterEntry : IComparable<TwitterEntry>
    {
        public string Id { get; set; }
        public DateTime Published { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Updated { get; set; }
        public string Source { get; set; }
        public string AuthorName { get; set; }
        public string AuthorURI { get; set; }
        public string TimeAgo { get; set; }

        public TwitterEntry(string id, string published, string title, string content, string updated, string source, string authorName, string authorURI, string timeAgo)
        {
            Id = id;
            Published = DateTime.Parse(published);
            Title = title;
            Content = content;
            Updated = DateTime.Parse(updated);
            Source = source;
            AuthorName = authorName;
            AuthorURI = authorURI;
            TimeAgo = timeAgo;
        }

        public int CompareTo(TwitterEntry other)
        {
            return Published.CompareTo(other.Published);
        }
    }

    public class TweetContent
    {
        public TweetContent()
        {
            this.PlayerMentions = new List<Player>();
        }

        public string ID { get; set; }
        public string Message { get; set; }
        public List<Player> PlayerMentions { get; set; }
    }

    //classes for converting json to obj
    public class Url
    {
        public string url { get; set; }
        public string expanded_url { get; set; }
        public string display_url { get; set; }
        public int[] indices { get; set; }
    }

    public class Entities
    {
        public Url[] urls { get; set; }
    }

    public class Metadata
    {
        public int recent_retweets { get; set; }
        public string result_type { get; set; }
    }

    public class Result
    {
        public string created_at { get; set; }
        public Entities entities { get; set; }
        public string from_user { get; set; }
        public string from_user_name { get; set; }
         public int from_user_id { get; set; }
        public string from_user_id_str { get; set; }
        public object geo { get; set; }
        public object id { get; set; }
        public string id_str { get; set; }
        public string iso_language_code { get; set; }
        public Metadata metadata { get; set; }
        public string profile_image_url { get; set; }
        public string source { get; set; }
        public string text { get; set; }
        public object to_user_id { get; set; }
        public object to_user_id_str { get; set; }
    }

    public class RootObject
    {
        public double completed_in { get; set; }
        public long max_id { get; set; }
        public string max_id_str { get; set; }
        public string next_page { get; set; }
        public int page { get; set; }
        public string query { get; set; }
        public string refresh_url { get; set; }
        public Result[] results { get; set; }
        public int results_per_page { get; set; }
        public int since_id { get; set; }
        public string since_id_str { get; set; }
        public string player { get; set; }
    }
}