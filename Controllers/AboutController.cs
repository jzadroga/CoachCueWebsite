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
using CoachCue.Repository;
using System.Threading.Tasks;

namespace CoachCue.Controllers
{
    public class AboutController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            PageViewModel pgVM = new PageViewModel();
            pgVM.UserData = await CoachCueUserData.GetUserData(User.Identity.Name);

            return View(pgVM);
        }
    }
}
