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

namespace CoachCue.Controllers
{
    public class AboutController : BaseController
    {
        public ActionResult Index()
        {
            return View(new PageViewModel());
        }
    }
}
