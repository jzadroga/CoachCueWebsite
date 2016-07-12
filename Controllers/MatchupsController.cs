using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;
using System.ComponentModel;
using CoachCue.ViewModels;
using System.Net;
using System.IO;
using System.Web.Routing;
using CoachCue.Utility;

namespace CoachCue.Controllers
{
    public class MatchupsController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home", new  { tb="y" });
        }
    }
}
