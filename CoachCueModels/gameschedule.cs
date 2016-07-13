using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using HtmlAgilityPack;
using System.Net.Http;
using System.Xml;
using System.Globalization;

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
                          && plyrs.playerID == playerID && mt.gameDate > DateTime.UtcNow.GetEasternTime() && mt.seasonID == 5
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

        public static void ImportSchedule(int seasonID)
        {
            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "5b3576f351cb417a853030e21265dabc");

            var uri = "https://api.fantasydata.net/nfl/v2/{format}/Schedules/2016";

            HttpResponseMessage response = client.GetAsync(uri).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsStringAsync().Result;
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(dataObjects);

                foreach( XmlNode schedule in doc.SelectNodes("//Schedule") )
                {
                    int week = Convert.ToInt32(schedule.SelectSingleNode("Week").InnerText);
                    if (week > 3)
                    {
                        string date = schedule.SelectSingleNode("Date").InnerText.Replace("T", " ");
                        if (!string.IsNullOrEmpty(date))
                        {
                            DateTime gameDate = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                            int homeTeam = nflteam.GetID(schedule.SelectSingleNode("AwayTeam").InnerText);
                            int awayTeam = nflteam.GetID(schedule.SelectSingleNode("HomeTeam").InnerText);

                            SaveGame(homeTeam, awayTeam, gameDate, week, seasonID);
                        }
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