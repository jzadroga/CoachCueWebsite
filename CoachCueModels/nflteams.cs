using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoachCueModels;
using HtmlAgilityPack;
using System.Web;

namespace CoachCue.Model
{
    public partial class nflteam
    {
        public string Mascot
        {
            get { return this.teamName.Substring(teamName.LastIndexOf(' ') + 1); }
        }

        public string City
        {
            get { return this.teamName.Substring(0, teamName.LastIndexOf(' ')); }
        }

        public string ShortCity
        {
            get
            {
                string[] city = this.teamName.Split(' ');
                return (city.Count() > 1) ? city[1] : city[0];
            }
        }

        public static nflteam Get(int teamID)
        {
            string cacheID = "team" + teamID.ToString();
            nflteam team = new nflteam();

            if (HttpContext.Current.Cache[cacheID] != null)
                team = (nflteam)HttpContext.Current.Cache[cacheID];
            else
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var tms = db.nflteams.Where(tm => tm.teamID == teamID);

                if (tms.Count() > 0)
                {
                    team = tms.FirstOrDefault();
                    HttpContext.Current.Cache.Insert(cacheID, team, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(800, 0, 0));
                }
            }           

            return team;
        }

        public static int GetID(string teamSlug)
        {
            int teamID = 0;

            CoachCueDataContext db = new CoachCueDataContext();
            var tms = db.nflteams.Where(tm => tm.teamSlug.ToLower() == teamSlug.ToLower());

            if (tms.Count() > 0)
                teamID = tms.FirstOrDefault().teamID;

            return teamID;
        }

        public static List<nflteam> List()
        {
            List<nflteam> teams = new List<nflteam>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var ret = from mt in db.nflteams
                          select mt;

                teams = ret.ToList();
            }
            catch (Exception)
            {
            }

            return teams;
        }

        public static int GetIDByName(string teamName)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            var team = from mt in db.nflteams
                       where mt.teamName.ToLower() == teamName
                       select mt;

            return team.FirstOrDefault().teamID;
        }

        public static int? GetIDByEspnName(string teamName)
        {
            int? teamID = null;
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                if (teamName == "NY Giants")
                    teamName = "New York Giants";
                else if (teamName == "NY Jets")
                    teamName = "New York Jets";
                else if (teamName == "St. Louis")
                    teamName = "St Louis";

                var team = from mt in db.nflteams
                           where mt.teamName.ToLower().Contains(teamName)
                           select mt;

                if( team.Count() > 0 )
                    teamID = team.FirstOrDefault().teamID;
            }
            catch(Exception)
            {
                teamID = null;
            }

            return teamID;
        }

        public static void AddTwitterAccount(int twitteraccountID, int teamID)
        {
            nflteams_twitteraccount teamAcnt = new nflteams_twitteraccount();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                teamAcnt.twitterAccountID = twitteraccountID;
                teamAcnt.teamID = teamID;

                db.nflteams_twitteraccounts.InsertOnSubmit(teamAcnt);
                db.SubmitChanges();

            }
            catch (Exception) { }
        }

        public static void DeleteTwitterAccount(int twitteraccountID)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var ret = from mt in db.nflteams_twitteraccounts
                          where mt.twitterAccountID == twitteraccountID
                          select mt;

                nflteams_twitteraccount accnt = ret.FirstOrDefault();
                if (accnt != null)
                {
                    db.nflteams_twitteraccounts.DeleteOnSubmit(accnt);
                    db.SubmitChanges();
                }
            }
            catch (Exception) { }
        }
    }
}
