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
    public static class MessageService
    { 
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
    }
}
