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
using System.Web;

namespace CoachCue.Service
{
    public static class MatchupService
    { 
        //save a Matchup document to the Matchups collection
        public static async Task<Matchup> Save(CoachCueUserData userData, List<string> players, string type)
        {
            Matchup matchup = new Matchup();

            try
            {
                matchup.CreatedBy = userData.UserId;
                matchup.Type = type;
                matchup.Active = true;
                matchup.Completed = false;
                matchup.DateCreated = DateTime.UtcNow.GetEasternTime();

                matchup.UserName = userData.UserName;
                matchup.Name = userData.Name;
                matchup.ProfileImage = userData.ProfileImage;
                matchup.Email = userData.Email;

                foreach( var player in await PlayerService.GetListByIds(players) )
                {
                    matchup.Players.Add(new MatchupPlayer()
                    {
                        Id = player.Id,
                        Number = player.Number,
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        Position = player.Position,
                        Team = player.Team,
                        Twitter = player.Twitter,
                        GameId = "0"
                    });
                }

                var result = await DocumentDBRepository<Matchup>.CreateItemAsync(matchup, "Matchups");
                matchup.Id = result.Id;   
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
              
            return matchup;
        }

        public static async Task<Matchup> AddVote(string playerId, string playerName, string matchupId, CoachCueUserData userData)
        {
            var matchup = await Get(matchupId);
                
            MatchupVote vote = new MatchupVote();
            vote.DateCreated = DateTime.UtcNow.GetEasternTime();
            vote.PlayerId = playerId;
            vote.PlayerName = playerName;

            if (userData != null)
            {
                vote.UserId = userData.UserId;
                vote.ProfileImage = userData.ProfileImage;
                vote.Email = userData.Email;
                vote.Name = userData.Name;
            }

            matchup.Votes.Add(vote);

            await DocumentDBRepository<Matchup>.UpdateItemAsync(matchupId, matchup, "Matchups");

            return matchup;
        }

        public static async Task<IEnumerable<Matchup>> GetList(DateTime endDate)
        {
            return await DocumentDBRepository<Matchup>.GetItemsAsync(d => d.Active == true, "Matchups");

        }

        public static async Task<Matchup> Get(string id)
        {
            return await DocumentDBRepository<Matchup>.GetItemAsync(id, "Matchups");
        }  
    }
}
