﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;
using System.ComponentModel;
using CoachCue.ViewModels;
using System.Net;
using System.IO;

namespace CoachCue.Controllers
{
    public class CoachController : BaseController
    {
        public ActionResult Index(int id, string name, [DefaultValue(0)]int mt)
        {
            UserViewModel userVM = new UserViewModel();

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            userVM.UserDetail = user.Get(id);

            userVM.UserStream = (mt == 0) ? stream.GetUserStream(id, false, userID) : stream.GetDetails(mt);
            userVM.MessageDetails = (mt == 0) ? false : true;
            userVM.Followers = user.GetFollowersCount(id);

            return View(userVM);
        }

        public ActionResult Matchup(int id, string name, [DefaultValue(0)]int mt)
        {
            UserViewModel userVM = new UserViewModel();

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            userVM.UserDetail = user.Get(id);

            userVM.UserStream = (mt == 0) ? matchup.GetAllUserMatchups(id, 30, userID) : stream.GetDetails(mt);
            userVM.MessageDetails = (mt == 0) ? false : true;
            userVM.Followers = user.GetFollowersCount(id);

            return View("Index", userVM);
        }
    }
}
