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
    public partial class status
    {
        public static int GetID(string statusName, string componentName)
        {
            int statusID = 0;

            CoachCueDataContext db = new CoachCueDataContext();
            var stat = db.status.Where(st => st.statusName.ToLower() == statusName.ToLower() && st.component.componentName == componentName);

            if (stat.Count() > 0)
                statusID = stat.FirstOrDefault().statusID;

            return statusID;
        }


    }
}
