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

namespace CoachCue.Model
{
    public class search
    {
        public static List<AccountData> TypeAheadSearch(string query, int? userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<AccountData> accounts = new List<AccountData>();

            try
            {
                List<AccountData> acnts = nflplayer.TypeAheadSearch(query, userID);
                List<UserData> coaches = user.TypeAheadSearch(query, userID);

                acnts.AddRange(coaches.Select(coach => new AccountData
                {
                    username = coach.username,
                    fullName = coach.fullName,
                    accountID = coach.userID,
                    following = coach.following,
                    profileImg = coach.profileImg,
                    accountType = "users"
                }).ToList());

                if (acnts.Count() > 0)
                    accounts = acnts.ToList();
            }
            catch (Exception) { }

            return accounts;
        }
    }
}
