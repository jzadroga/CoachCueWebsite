using CoachCue.Models;
using CoachCue.Repository;
using HtmlAgilityPack;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace CoachCue.Service
{
    public static class GameScheduleService
    {
        public static async Task<Game> GetCurrentWeek(string teamSlug)
        {
            //no schedule for 2017 yet so default to week 1
            Game gameSchedule = new Game();
            gameSchedule.Week = 1;

            try
            {
                //get the gameschedules that fall within this week
                DateTime weekStart = DateTime.UtcNow.GetEasternTime().StartOfWeek(DayOfWeek.Tuesday);
                DateTime weekEnd = weekStart.AddDays(7);

                var schedule = await DocumentDBRepository<Player>.GetScheduleAsync("schedule-2017");
                    
                var currentGame = schedule.Games.Where(d => (d.HomeTeam == teamSlug || d.AwayTeam == teamSlug)
                        && (d.GameDate >= weekStart && d.GameDate <= weekEnd));

                if (currentGame.Count() > 0)
                    gameSchedule = currentGame.FirstOrDefault();
            }
            catch (Exception) { }

            return gameSchedule;
        }

        public static void ImportSchedule(int year)
        {
            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "5b3576f351cb417a853030e21265dabc");

            var uri = "https://api.fantasydata.net/nfl/v2/{format}/Schedules/" + year.ToString() + "/json";

            HttpResponseMessage response = client.GetAsync(uri).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsStringAsync().Result;
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(dataObjects);

                doc.Save(HttpContext.Current.Request.PhysicalApplicationPath + "assets\\data\\schedule-" + year.ToString());
                /*foreach (XmlNode schedule in doc.SelectNodes("//Schedule"))
                {
                    int week = Convert.ToInt32(schedule.SelectSingleNode("Week").InnerText);
                    if (week > 3)
                    {
                        string date = schedule.SelectSingleNode("Date").InnerText.Replace("T", " ");
                        if (!string.IsNullOrEmpty(date))
                        {
                            DateTime gameDate = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                            //int homeTeam = nflteam.GetID(schedule.SelectSingleNode("AwayTeam").InnerText);
                            //int awayTeam = nflteam.GetID(schedule.SelectSingleNode("HomeTeam").InnerText);

                            //SaveGame(homeTeam, awayTeam, gameDate, week, seasonID);
                        }
                    }
                }*/
            }
            //var test = new List<Game>();
            //await DocumentDBRepository<Player>.CreateItemAsync(test, "Players");
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
