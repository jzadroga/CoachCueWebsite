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
using CoachCue.Helpers;
using CoachCue.Repository;
using System.Threading.Tasks;

namespace CoachCue.Controllers
{
    public class ContactUsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            PageViewModel pgVM = new PageViewModel();
            await LoadBaseViewModel(pgVM);
            ViewData["sent"] = false;
            return View(pgVM);
        }

        [HttpPost]
        public async Task<ActionResult> Index(string email, string message)
        {
            PageViewModel pgVM = new PageViewModel();
            await LoadBaseViewModel(pgVM);
            ViewData["sent"] = true;

            string bodyMsg = "Message from: " + email;
            bodyMsg += " Message: " + message;

            string msg = EmailHelper.Send("info@coachcue.com", bodyMsg, "Contact from site", "Info");

            return View(pgVM);
        }
    }
}
