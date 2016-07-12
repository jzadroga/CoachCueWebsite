using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoachCue.Model
{
    public partial class signup
    {
        public static void Save(string email)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                signup info = new signup();

                info.email = email;
                info.dateCreated = DateTime.Now;
                db.signups.InsertOnSubmit(info);
                db.SubmitChanges();
            }
            catch (Exception) { }
        }
    }
}