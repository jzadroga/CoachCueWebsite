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
using LinqToTwitter;

namespace CoachCue.Model
{
    public partial class accounttype
    {
        public static int GetID(string typeName)
        {
            int typeID = 0;

            CoachCueDataContext db = new CoachCueDataContext();
            var ps = db.accounttypes.Where(acnt => acnt.accountName.ToLower() == typeName.ToLower());

            if (ps.Count() > 0)
                typeID = ps.FirstOrDefault().accounttypeID;

            return typeID;
        }
    }
}
