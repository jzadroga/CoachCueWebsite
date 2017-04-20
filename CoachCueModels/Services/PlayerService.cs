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
using System.Text;
using System.Threading.Tasks;

namespace CoachCue.Service
{
    public static class PlayerService
    {
        //inmports the roster from the nfl.com site by team
        //todo - fix the cleanup process and remove nflteam reference
        public static async Task<List<string>> ImportRoster(string slug)
        {
            List<string> foundPlayers = new List<string>();

            //get the roster from nfl.com and parse the html
            HttpWebRequest request = WebRequest.Create("http://www.nfl.com/teams/buffalobills/roster?team=" + slug) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Load data into a htmlagility doc   
                var receiveStream = response.GetResponseStream();
                if (receiveStream != null)
                {
                    var stream = new StreamReader(receiveStream);
                    HtmlDocument roster = new HtmlDocument();
                    roster.Load(stream);

                    foreach (HtmlNode playerRow in roster.DocumentNode.SelectNodes("//table/tbody/tr"))
                    {
                        string number = string.Empty;
                        string firstName = string.Empty;
                        string lastName = string.Empty;
                        string position = string.Empty;
                        string years = string.Empty;
                        string college = string.Empty;

                        HtmlNodeCollection cellNodes = playerRow.SelectNodes("td");
                        for (int i = 0; i < cellNodes.Count; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    number = cellNodes[i].InnerText;
                                    break;
                                case 1:
                                    string[] fullName = cellNodes[i].InnerText.Split(',');
                                    firstName = fullName[1].Trim();
                                    lastName = fullName[0].Trim();
                                    break;
                                case 2:
                                    position = cellNodes[i].InnerText;
                                    break;
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                    break;
                                case 7:
                                    years = cellNodes[i].InnerText;
                                    break;
                                case 8:
                                    college = cellNodes[i].InnerText;
                                    break;
                            }
                        }

                        var player = await SavePlayer(firstName, lastName, position, number, college, years, slug);
                        foundPlayers.Add(player.Id);
                    }

                    //clean out any players not on the roster
                    foreach(var ply in await GetByTeam(slug))
                    {
                        if (ply.LastName == "Defense")
                            continue;

                        if(!foundPlayers.Contains(ply.Id))
                        {
                            await Delete(ply);
                        }
                    }
                }
            }

            return foundPlayers;
        }

        //save a player document to the Players collection
        public static async Task<Player> SavePlayer(string firstName, string lastName, string position, string number, string college,
                                     string years, string slug)
        {
            Player player = new Player();

            if (position == "QB" || position == "TE" || position == "RB" || position == "WR" || position == "K" || position == "FB")
            {
                var players = await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active || !d.Active, "Players");

                //first see if we need to update or add
                player = players.Where(ply => ply.LastName.ToLower() == lastName.ToLower()
                     && ply.FirstName.ToLower() == firstName.ToLower()
                     && ply.Position.ToLower() == position.ToLower()).FirstOrDefault();

                if (player == null)
                {
                    //add new
                    player = new Player();
                    player.FirstName = firstName;
                    player.LastName = lastName;
                    player.Position = position;
                    player.Number = string.IsNullOrEmpty(number) ? null : (int?)Convert.ToInt32(number);
                    player.Active = true;

                    //team info
                    player.Team = Team.GetTeamBySlug(slug);

                    //twitter info - default to empty
                    TwitterAccount twitter = new TwitterAccount();                   
                    player.Twitter = twitter;

                    var result = await DocumentDBRepository<Player>.CreateItemAsync(player, "Players");
                    player.Id = result.Id;
                }
                else
                {
                    //update the player
                    player.FirstName = firstName;
                    player.LastName = lastName;
                    player.Team = Team.GetTeamBySlug(slug);
                    player.Position = position;
                    player.Number = string.IsNullOrEmpty(number) ? null : (int?)Convert.ToInt32(number);
                    player.Active = true;

                    await DocumentDBRepository<Player>.UpdateItemAsync(player.Id, player, "Players");
                }
            }
            return player;
        }

        public static async Task<Player> UpdateTwitterAccount(string playerID, string twitternName, string twitterImage)
        {
            var player = await Get(playerID);
            player.Twitter = new TwitterAccount() { Active = true, Name = twitternName, ProfileImage = twitterImage };

            await DocumentDBRepository<Player>.UpdateItemAsync(player.Id, player, "Players");

            return player;
        }

        public static async Task<Player> Get(string id)
        {
            return await DocumentDBRepository<Player>.GetItemAsync(id, "Players");
        }

        public static async Task<Player> GetByDescription(string slug, string link)
        {
            var players = await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true 
                && d.Link.ToLower() == link.ToLower() 
                && d.Team.Slug.ToLower() == slug.ToLower(), "Players");

            return players.FirstOrDefault();
        }

        public static async Task<Player> GetByLink(string link)
        {
            var players = await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true
                && d.Link.ToLower() == link.ToLower(), "Players");

            return players.FirstOrDefault();
        }

        public static async Task Delete(Player player)
        {
            //mark as not active - if on another team it will get activated then
            player.Active = false;

            await DocumentDBRepository<Player>.UpdateItemAsync(player.Id, player, "Players");
        }

        public static async Task<List<string>> ListJournalistsByTeam(string teamSlug)
        {
            var players = await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true && d.Team.Slug == teamSlug, "Players");

            return players.First().BeatWriters;
        }

        public static async Task<bool> DeleteBeatWriter(string teamSlug, string username)
        {
            var players = await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true && d.Team.Slug == teamSlug, "Players");
            foreach (var player in players)
            {
                player.BeatWriters.Remove(username);
                await DocumentDBRepository<Player>.UpdateItemAsync(player.Id, player, "Players");
            }

            return true;
        }

        public static async Task<bool> AddBeatWriter(string teamSlug, string username)
        {
            var players = await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true && d.Team.Slug == teamSlug, "Players");
            foreach( var player in players )
            {
                player.BeatWriters.Add(username);
                await DocumentDBRepository<Player>.UpdateItemAsync(player.Id, player, "Players");
            }

            return true;
        }

        public static async Task<IEnumerable<Player>> GetByTeam(string teamSlug)
        {
            return await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true && d.Team.Slug == teamSlug, "Players");
        }

        public static async Task<IEnumerable<VotedPlayer>> GetTopVoted()
        {
            List<VotedPlayer> players = new List<VotedPlayer>();
            var matchups = await DocumentDBRepository<Matchup>.GetItemsAsync(d => d.Active == true, "Matchups");

            //get the 250 most recent
            matchups = matchups.OrderByDescending(d => d.DateCreated).Take(250);

            var votedPlayers = matchups.SelectMany(mt => mt.Votes, (mt, votes) => new { mt, votes })
                        .GroupBy(ply => ply.votes.PlayerId, pair => pair.mt).Select(ply =>
                           new { id = ply.Key, count = ply.Count() }).ToList();

            var playerList = await GetListByIds(votedPlayers.Select(ply => ply.id).ToList());

            if (votedPlayers.Count() > 0)
            { 
                int topCount = votedPlayers.OrderByDescending(ply => ply.count).First().count;

                foreach (var player in playerList)
                {
                    int count = votedPlayers.Where(ply => ply.id == player.Id).First().count;
                    string percent = ((Convert.ToDecimal(count) / topCount) * 100).ToString() + "%";

                    players.Add(new VotedPlayer { Percent = percent, Votes = count, Player = player });
                }
            }
           
            return players.OrderByDescending(ply => ply.Votes);
        }

        public static async Task<IEnumerable<Player>> GetMostVoted()
        {
            var matchups = await DocumentDBRepository<Matchup>.GetItemsAsync(d => d.Active == true, "Matchups");

            //get the 250 most recent
            matchups = matchups.OrderByDescending(d => d.DateCreated).Take(250);

            var votedPlayers = matchups.SelectMany(mt => mt.Votes, (mt, votes) => new { mt, votes })
                        .GroupBy(ply => ply.votes.PlayerId, pair => pair.mt).Select(ply => ply.Key).ToList();

            return await GetListByIds(votedPlayers.Distinct().ToList());
        }

        public static async Task<IEnumerable<Player>> GetTrending()
        {
            var matchups = await DocumentDBRepository<Matchup>.GetItemsAsync(d => d.Active == true, "Matchups");

            //get the 50 most recent
            matchups = matchups.OrderByDescending(d => d.DateCreated).Take(50);

            var votedPlayers = matchups.SelectMany(mt => mt.Votes, (mt, votes) => new { mt, votes })
                        .GroupBy(ply => ply.votes.PlayerId, pair => pair.mt).Select(ply => ply.Key).ToList();

            var mentionPlayers = matchups.SelectMany(mt => mt.Players, (mt, players) => new { mt, players })
                        .GroupBy(ply => ply.players.Id, pair => pair.mt).Select( ply => ply.Key).ToList();

            votedPlayers.AddRange(mentionPlayers);

            return await GetListByIds(votedPlayers.Distinct().ToList());
        }

        public static async Task<IEnumerable<Player>> GetList()
        {
            return await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true, "Players");
        }

        public static async Task<IEnumerable<Player>> GetJsonList()
        {
            var players = await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true, "Players");

            foreach( var player in players)
            {
                player.Team.Slug = player.Team.Slug.ToLower();
            }

            return players;
        }

        public static async Task<IEnumerable<Player>> GetListByIds(List<string> playerIds)
        {
            return await DocumentDBRepository<Player>.GetItemsAsync(d => playerIds.Contains(d.Id), "Players");
        }

        public static async Task<List<TwitterAccount>> GetTwitterAccounts(string playerID, TwitterContext twitterCtx)
        {
            Player playerAccount = await Get(playerID);
            List<TwitterAccount> accounts = new List<TwitterAccount>();

            try
            {
                /* if (playerAccount.twitterAccountID.HasValue)
                 {
                     //ulong accountID = Convert.ToUInt64(twitteraccount.Get(playerAccount.twitterAccountID.Value).twitterID);
                     twitteraccount acnt = twitteraccount.Get(playerAccount.twitterAccountID.Value);

                     twitterUsers = (from usr in twitterCtx.User
                                     where usr.Type == UserType.Lookup &&
                                     usr.ScreenNameList == acnt.twitterUsername
                                     select usr).ToList();
                 }
                 else
                 {*/
                //see if the player has a twitter account, find the player account on twitter
                string playerName = playerAccount.FirstName.Replace(".", "").Replace("-", " ").Replace("'", " ") + " " + playerAccount.LastName.Replace(".", "").Replace("-", " ").Replace("'", " ");
                var twitterUsers = (from usr in twitterCtx.User
                                where usr.Type == UserType.Search &&
                                usr.Query == playerName && usr.Verified == true
                                select usr).ToList();
                //}

                if (twitterUsers.Count() > 0)
                {
                    foreach (var user in twitterUsers)
                    {
                        accounts.Add(new TwitterAccount
                        {
                            // twitterAccountID = (playerAccount.twitterAccountID.HasValue) ? (int)playerAccount.twitterAccountID : 0,
                            // twitterID = user.UserID.ToString(),
                            Name = user.ScreenNameResponse,
                            Active = true,
                           // twitterName = user.Name,
                            ProfileImage = user.ProfileImageUrl
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }

            return accounts;
        }

        public static async Task<List<string>> GetPlayerNotConditions(string firstName, string lastName)
        {
            List<string> names = new List<string>();
            try
            {
                //find all players with the same lastname but different last names and players with the first name of last name of the player
                var fn = await DocumentDBRepository<Player>.GetItemsAsync( ply => (ply.LastName == lastName && ply.FirstName != firstName) || ply.FirstName == ply.LastName, "Players");
                
                if (fn.Count() > 0)
                {
                    foreach (var player in fn.ToList())
                    {
                        if (!names.Contains(player.LastName) && !names.Contains(player.FirstName))
                            names.Add((player.FirstName == lastName) ? player.LastName : player.FirstName);

                        //also add the team city and mascot
                        //names.Add(player.nflteam.Mascot);
                        //names.Add(player.nflteam.ShortCity);
                    }
                }
            }
            catch (Exception) { }

            return names;
        }
    }
}
