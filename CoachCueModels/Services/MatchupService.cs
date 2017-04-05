using CoachCue.Model;
using CoachCue.Models;
using CoachCue.Repository;
using HtmlAgilityPack;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
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
                string link = type.Replace(" ", "").Replace("?", "").Replace("(", "").Replace(")", "");

                matchup.CreatedBy = userData.UserId;
                matchup.Type = type;
                matchup.Active = true;
                matchup.Completed = false;
                matchup.DateCreated = DateTime.UtcNow.GetEasternTime();

                matchup.UserName = userData.UserName;
                matchup.Name = userData.Name;
                matchup.ProfileImage = userData.ProfileImage;
                matchup.Email = userData.Email;

                var playerList = await PlayerService.GetListByIds(players);
                var matchupPlayers = playerList.ToList();

                for(int i=0; i < matchupPlayers.Count; i++)
                {
                    var gameWeek = await GameScheduleService.GetCurrentWeek(matchupPlayers[i].Team.Slug);

                    matchup.Players.Add(new MatchupPlayer()
                    {
                        Id = matchupPlayers[i].Id,
                        Number = matchupPlayers[i].Number,
                        FirstName = matchupPlayers[i].FirstName,
                        LastName = matchupPlayers[i].LastName,
                        Position = matchupPlayers[i].Position,
                        Team = matchupPlayers[i].Team,
                        Twitter = matchupPlayers[i].Twitter,
                        GameWeek = gameWeek
                    });

                    if (i == 0)
                        link += "/" + gameWeek.Week + "/";

                    link += (i < (matchupPlayers.Count - 1)) ? matchupPlayers[i].Link + "-or-" : matchupPlayers[i].Link;
                }

                matchup.Link = link.ToLower();
                var result = await DocumentDBRepository<Matchup>.CreateItemAsync(matchup, "Matchups");
                matchup.Id = result.Id;

                await UserService.UpdateMatchupCount(userData.UserId);
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
                vote.UserName = userData.UserName;
            }

            matchup.Votes.Add(vote);

            await DocumentDBRepository<Matchup>.UpdateItemAsync(matchupId, matchup, "Matchups");

            //add voted notification
            if (userData != null)
            {
                await UserService.UpdateVoteCount(userData.UserId);

                if (userData.UserId != matchup.CreatedBy)
                {
                    var toUser = await UserService.Get(matchup.CreatedBy);
                    await NotificationService.Save(userData, toUser, userData.Name + " voted on your CoachCue matchup.", "vote", matchup);
                }
            }

            return matchup;
        }

        public static async Task<Matchup> GetByLink(string link)
        {
            var matchup = await DocumentDBRepository<Matchup>.GetItemsAsync(d => d.Active == true && d.Link == link, "Matchups");
            return matchup.FirstOrDefault();
        }

        public static async Task<IEnumerable<Matchup>> GetList(DateTime endDate, bool includeCompleted = true)
        {
            var matchups = await DocumentDBRepository<Matchup>.GetItemsAsync(d => d.Active == true, "Matchups");

            if (!includeCompleted)
                matchups = matchups.Where(mt => mt.Completed = false);

            return matchups.OrderByDescending(d => d.DateCreated).ThenBy(d => d.Votes.Count);
        }

        public static IEnumerable<Matchup> GetListByPlayer(DateTime endDate, string playerId)
        {
            return DocumentDBRepository<Matchup>.GetPlayerMatchups(playerId).OrderByDescending( mtch => mtch.DateCreated);
        }

        public static IEnumerable<Matchup> GetListByPosition(DateTime endDate, string position)
        {
            return DocumentDBRepository<Matchup>.GetPositionMatchups(position).OrderByDescending(mtch => mtch.DateCreated);
        }

        public static async Task<IEnumerable<Matchup>> GetListByUser(DateTime endDate, string userId)
        {
            var matchups = await DocumentDBRepository<Matchup>.GetItemsAsync(d => d.Active == true && d.CreatedBy == userId, "Matchups");
            return matchups.OrderByDescending(d => d.DateCreated).ThenBy(d => d.Votes.Count);
        }

        public static async Task<IEnumerable<Matchup>> GetRelatedList(DateTime endDate, Matchup matchup)
        {
            var matchups = await DocumentDBRepository<Matchup>.GetItemsAsync(d => d.Active == true && d.Type == matchup.Type && d.Id != matchup.Id, "Matchups");

            matchups = matchups.OrderByDescending(d => d.DateCreated).ThenBy(d => d.Votes.Count).Take(15);

            return matchups;
        }

        public static async Task<Matchup> Get(string id)
        {
            return await DocumentDBRepository<Matchup>.GetItemAsync(id, "Matchups");
        }  
    }
}
