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

                var schedule = await DocumentDBRepository<Game>.GetItemsAsync(d => d.Season == gameSchedule.Season
                        && (d.HomeTeam.Slug == teamSlug || d.AwayTeam.Slug == teamSlug)
                        && (d.Date >= weekStart && d.Date <= weekEnd), "Games");

                if (schedule.Count() > 0)
                    gameSchedule = schedule.FirstOrDefault();
            }
            catch (Exception) { }

            return gameSchedule;
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
