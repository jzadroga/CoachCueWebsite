using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoachCue.Model
{
    public partial class users_view
    {
        public static users_view Add(string type, int entityID, int? userID )
        {
            CoachCueDataContext db = new CoachCueDataContext();

            users_view view = new users_view();

            try
            {
                int typeID = accounttype.GetID(type);
                view.dateViewed = DateTime.Now;
                view.viewObjectID = entityID;
                view.accounttypeID = typeID;    
                if( userID.HasValue )
                    view.userID = userID;

                view.ipAddress = HttpContext.Current.Request.UserHostAddress;

                db.users_views.InsertOnSubmit(view);
                db.SubmitChanges();
            }
            catch (Exception ex) 
            { 
                string s = ex.Message; 
            }

            return view;
        }
    }
}