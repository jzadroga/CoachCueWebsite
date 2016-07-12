using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoachCue.Model
{
    public partial class avatar
    {
        public static avatar Save(avatar avatarItem)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            avatar avatar = new avatar();

            try
            {
                //first see if we need to update or add
                if (avatarItem.avatarID != 0)
                {
                    avatar updateAvatar = db.avatars.Where(avt => avt.avatarID == avatarItem.avatarID).FirstOrDefault();
                    updateAvatar.imageName = avatarItem.imageName;
                    db.SubmitChanges();
                    avatar = updateAvatar;
                }
                else
                {
                    avatar newAvatar = new avatar();
                    newAvatar.imageName = avatarItem.imageName;
                    newAvatar.statusID = status.GetID("Active", "avatars");
                    db.avatars.InsertOnSubmit(newAvatar);
                    db.SubmitChanges();
                    avatar = newAvatar;
                }  
            }
            catch (Exception) { }

            return avatar;
        }
    }
}