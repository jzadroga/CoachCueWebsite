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
                          && plyrs.playerID == playerID && mt.gameDate > DateTime.UtcNow.GetEasternTime() && mt.seasonID == 4
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
                          && mt.gameDate > DateTime.UtcNow.GetEasternTime()
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
                DateTime weekStart = DateTime.UtcNow.GetEasternTime().StartOfWeek(DayOfWeek.Tuesday);
                DateTime weekEnd = weekStart.AddDays(7);

                var ret = from mt in db.gameschedules
                          where mt.nflseason.year == DateTime.UtcNow.GetEasternTime().Year
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

        public static void ImportSchedule(int seasonID, int week)
        {
            //get the schedule from espn.com and parse the html
            //string scheduleURL = "http://espn.go.com/nfl/schedule/_/week/" + week.ToString();

            string scheduleURL = "http://espn.go.com/nfl/schedule/_/seasontype/2/week/" + week.ToString();
            HttpWebRequest request = WebRequest.Create(scheduleURL) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Load data into a htmlagility doc   
                var receiveStream = response.GetResponseStream();
                if (receiveStream != null)
                {
                    var stream = new StreamReader(receiveStream);
                    HtmlDocument roster = new HtmlDocument();
                    roster.Load(stream);

                    foreach (HtmlNode weeklyTable in roster.DocumentNode.SelectNodes("//table[@class='schedule has-team-logos align-left']"))
                    {
                        DateTime gameDate = new DateTime();

                        //get the date
                        HtmlNode caption = weeklyTable.SelectSingleNode("caption");
                        string date = caption.InnerText;
                        gameDate = DateTime.ParseExact(date.Split(',')[1].Trim() + " 2015 00:00", "MMMM d yyyy HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);

                        foreach (HtmlNode gameRow in weeklyTable.SelectNodes("//tr"))
                        {
                            if (gameRow.Attributes["class"] != null)
                            {
                                string rowClass = gameRow.Attributes["class"].Value;
                                if (rowClass.Contains( "odd" ) || rowClass.Contains( "even" ))
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
                                                HtmlNode awayNodes = cellNodes[i].SelectSingleNode("a[@class='team-name']/span");
                                                if (awayNodes != null)
                                                    awayTeam = nflteam.GetIDByEspnName(awayNodes.InnerText);
                                                break;
                                            case 1:
                                                HtmlNode homeNodes = cellNodes[i].SelectSingleNode("a[@class='team-name']/span");
                                                if (homeNodes != null)
                                                    homeTeam = nflteam.GetIDByEspnName(homeNodes.InnerText);
                                                break;
                                            case 2:
                                                //string time = cellNodes[i].SelectSingleNode("a").InnerText;
                                                //int hour = Convert.ToInt32(time.Split(':')[0]) + 12;
                                                //TimeSpan ts = new TimeSpan(hour, Convert.ToInt32(time.Split(':')[1].Replace(" AM", "").Replace(" PM", "")), 0);
                                                TimeSpan ts = new TimeSpan(13, 0, 0);
                                                gameDate = gameDate.Date + ts;
                                                addGame = true;
                                                break;
                                        }
                                    }

                                    if (addGame)
                                    {
                                        if (homeTeam.HasValue && awayTeam.HasValue)
                                            SaveGame(homeTeam.Value, awayTeam.Value, gameDate, week, seasonID);
                                        
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

        public static DateTime GetEasternTime(this DateTime date)
        {
            //DateTime timeUtc = date.UtcNow;
            TimeZoneInfo easterZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime eastTime = TimeZoneInfo.ConvertTimeFromUtc(date, easterZone);

            return eastTime;
        }
    }

    public class GameWeek
    {
        public string Label { get; set; }
        public int ID { get; set; }
    }
}