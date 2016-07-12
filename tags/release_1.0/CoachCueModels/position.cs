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
    public partial class position
    {
        public static int GetID(string positionName)
        {
            int positionID = 0;

            CoachCueDataContext db = new CoachCueDataContext();
            var ps = db.positions.Where(pos => pos.positionName.ToLower() == positionName.ToLower());

            if (ps.Count() > 0)
                positionID = ps.FirstOrDefault().positionID;

            return positionID;
        }
    }
}
