using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using HtmlAgilityPack;

namespace CoachCue.Model
{
    public partial class gameschedule
    {
        public static int GetPlayerGame(int playerID)
        {
            int gameID = 0;

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = (from mt in db.gameschedules
                          from plyrs in db.nflplayers 
                          where ( mt.nflTeamAway == plyrs.teamID || mt.nflTeamHome == plyrs.teamID )  
                          && plyrs.playerID == playerID && mt.gameDate > DateTime.Now && mt.seasonID == 2
                          select mt).FirstOrDefault();

                if( ret != null )
                    gameID = ret.gamescheduleID;
            }
            catch (Exception)
            {
            }

            return gameID;
        }

        public static List<gameschedule> List(int year)
        {
            List<gameschedule> schedule = new List<gameschedule>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.gameschedules
                          where mt.nflseason.year == year
                          && mt.gameDate > DateTime.Now
                          select mt;

                schedule = ret.ToList();
            }
            catch (Exception)
            {
            }

            return schedule;
        }

        public static int GetCurrentWeekID()
        {
            int weekID = 0;

            gameschedule week = GetCurrentWeek();
            if (week.weekNumber != 0)
                weekID = week.weekNumber;

            return weekID;
        }

        public static gameschedule GetCurrentWeek()
        {
            gameschedule week = new gameschedule();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                //get the gameschedules that fall within this week
                DateTime weekStart = DateTime.Now.StartOfWeek(DayOfWeek.Tuesday);
                DateTime weekEnd = weekStart.AddDays(7);

                var ret = from mt in db.gameschedules
                          where mt.nflseason.year == DateTime.Now.Year
                          && (mt.gameDate >= weekStart && mt.gameDate <= weekEnd)
                          select mt;

                if (ret.Count() > 0)
                    week = ret.FirstOrDefault();
            }
            catch (Exception) { }

            return week;
        }

        public static void SaveGame(int homeTeam, int awayTeam, DateTime gameDate, int weekNumber, int seasonID)
        {
            gameschedule game = new gameschedule();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                game.nflTeamHome = homeTeam;
                game.nflTeamAway = awayTeam;
                game.gameDate = gameDate;
                game.weekNumber = weekNumber;
                game.seasonID = seasonID;

                db.gameschedules.InsertOnSubmit(game);
                db.SubmitChanges();
            }
            catch (Exception) { }
        }

        public static void ImportSchedule(int seasonID)
        {
            //get the schedule from espn.com and parse the html
            HttpWebRequest request = WebRequest.Create("http://espn.go.com/nfl/schedule") as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Load data into a htmlagility doc   
                var receiveStream = response.GetResponseStream();
                if (receiveStream != null)
                {
                    var stream = new StreamReader(receiveStream);
                    HtmlDocument roster = new HtmlDocument();
                    roster.Load(stream);

                    int weekNumber = 0;
                    foreach (HtmlNode weeklyTable in roster.DocumentNode.SelectNodes("//table[@class='tablehead']"))
                    {
                        weekNumber++;

                        DateTime gameDate = new DateTime();

                        foreach (HtmlNode gameRow in weeklyTable.SelectNodes("tr"))
                        {
                            if (gameRow.Attributes["class"] != null)
                            {
                                string rowClass = gameRow.Attributes["class"].Value;
                                if (rowClass == "colhead") //get the game date
                                {
                                    string date = string.Empty;
                                    HtmlNodeCollection cellNodes = gameRow.SelectNodes("td");
                                    for (int i = 0; i < cellNodes.Count; i++)
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                date = cellNodes[i].InnerText;
                                                break;
                                        }
                                    }

                                    gameDate = DateTime.ParseExact(date.Split(',')[1].Trim() + " 2013 00:00", "MMM d yyyy HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                                }
                                else if (rowClass.Contains( "oddrow team" ) || rowClass.Contains( "evenrow team" ))
                                {
                                    bool addGame = false;
                                    int? homeTeam = null;
                                    int? awayTeam = null;
                                    //get the teams and time
                                    HtmlNodeCollection cellNodes = gameRow.SelectNodes("td");
                                    for (int i = 0; i < cellNodes.Count; i++)
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                HtmlNodeCollection linkNodes = cellNodes[i].SelectNodes("a");
                                                for (int ii = 0; ii < linkNodes.Count; ii++)
                                                {
                                                    switch (ii)
                                                    {
                                                        case 0:
                                                            homeTeam = nflteam.GetIDByEspnName(linkNodes[ii].InnerText);
                                                            break;
                                                        case 1:
                                                            awayTeam = nflteam.GetIDByEspnName(linkNodes[ii].InnerText);
                                                            break;
                                                    }
                                                }
                                                break;
                                            case 1:
                                                string time = cellNodes[i].InnerText;
                                                int hour = Convert.ToInt32(time.Split(':')[0]) + 12;
                                                TimeSpan ts = new TimeSpan(hour, Convert.ToInt32(time.Split(':')[1].Replace(" PM", "")), 0);
                                                gameDate = gameDate.Date + ts;
                                                addGame = true;
                                                break;
                                        }
                                    }

                                    if (addGame)
                                    {
                                        if (homeTeam.HasValue && awayTeam.HasValue)
                                            SaveGame(homeTeam.Value, awayTeam.Value, gameDate, weekNumber, seasonID);
                                        
                                        addGame = false;
                                    }
                                }
                            }
                        }
    
                        //SavePlayer(firstName, lastName, position, number, college, years, teamID);
                    }
                }
            }
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }
    }

    public class GameWeek
    {
        public string Label { get; set; }
        public int ID { get; set; }
    }
}