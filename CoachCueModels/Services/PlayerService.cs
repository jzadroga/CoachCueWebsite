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
using System.Threading.Tasks;

namespace CoachCue.Service
{
    public static class PlayerService
    {
        //inmports the roster from the nfl.com site by team
        //todo - fix the cleanup process and remove nflteam reference
        public static async Task<List<string>> ImportRoster(nflteam team)
        {
            List<string> foundPlayers = new List<string>();

            //get the roster from nfl.com and parse the html
            HttpWebRequest request = WebRequest.Create("http://www.nfl.com/teams/buffalobills/roster?team=" + team.teamSlug) as HttpWebRequest;
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

                        var player = await SavePlayer(firstName, lastName, position, number, college, years, team);
                        foundPlayers.Add(player.Id);
                    }

                    //clean out any players not on the roster
                    /*foreach(nflplayer ply in nflteam.Get(teamID).nflplayers)
                    {
                        if (ply.lastName == "Defense")
                            continue;

                        if(!foundPlayers.Contains(ply.playerID))
                        {
                            Delete(ply.playerID);
                        }
                    }*/
                }
            }

            return foundPlayers;
        }

        //save a player document to the Players collection
        public static async Task<Player> SavePlayer(string firstName, string lastName, string position, string number, string college,
                                     string years, nflteam team)
        {
            Player player = new Player();

            if (position == "QB" || position == "TE" || position == "RB" || position == "WR" || position == "K" || position == "FB")
            {
                var players = await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active, "Players");

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
                    Team plyTeam = new Team();
                    plyTeam.Name = team.teamName;
                    plyTeam.Slug = team.teamSlug;
                    player.Team = plyTeam;

                    //twitter info - default to empty
                    TwitterAccount twitter = new TwitterAccount();                   
                    player.Twitter = twitter;

                    await DocumentDBRepository<Player>.CreateItemAsync(player, "Players");
                }
                else
                {
                    //update the player
                    /*player.firstName = firstName;
                    player.lastName = lastName;
                    player.teamID = teamID;
                    player.positionID = CoachCue.Model.position.GetID(position);
                    player.number = string.IsNullOrEmpty(number) ? null : (int?)Convert.ToInt32(number);
                    player.college = college;
                    player.yearsExperience = Convert.ToInt32(years);
                    player.statusID = status.GetID("Active", "nflplayers");*/
                }

                //remove the player cache
                //HttpContext.Current.Cache.Remove("nflplayer" + player.playerID.ToString());
            }
            return player;
        }

        public static async Task<IEnumerable<Player>> GetList()
        {
            return await DocumentDBRepository<Player>.GetItemsAsync(d => d.Active == true, "Players");
        }

        public static async Task<IEnumerable<Player>> GetListByIds(List<string> playerIds)
        {
            return await DocumentDBRepository<Player>.GetItemsAsync(d => playerIds.Contains(d.Id), "Players");
        }
    }
}
