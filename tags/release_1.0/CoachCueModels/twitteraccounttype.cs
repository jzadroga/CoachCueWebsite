using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoachCue.Model
{
    public partial class twitteraccounttype
    {
        public static List<twitteraccounttype> List()
        {
            List<twitteraccounttype> types = new List<twitteraccounttype>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.twitteraccounttypes
                          select mt;

                types = ret.ToList();
            }
            catch (Exception)
            {
            }

            return types;
        }

        public static int GetTypeID(string type)
        {
            int typeID = 0;

            CoachCueDataContext db = new CoachCueDataContext();
            var ps = db.twitteraccounttypes.Where(actType => actType.accountType.ToLower() == type.ToLower());

            if (ps.Count() > 0)
                typeID = ps.FirstOrDefault().twitterAccountTypeID;

            return typeID;
        }
    }
}