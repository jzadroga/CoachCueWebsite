using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;
using CoachCue.ViewModels;
using System.ComponentModel;

namespace CoachCue.Controllers
{
    public class SignUpController : Controller
    {
        public ActionResult Index([DefaultValue(false)] bool vld)
        {
            SignupViewModel sVM = new SignupViewModel();
            sVM.ShowForm = true;
            sVM.Message = "We'll be relaunching soon. Sign up now!";
            sVM.InvalidLogin = vld;
            return View(sVM);
        }

        [HttpPost]
        public ActionResult Index(string email)
        {
            SignupViewModel sVM = new SignupViewModel();
            sVM.ShowForm = true;
            sVM.Message = "We'll be relaunching soon. Sign up now!";
            
            if( !string.IsNullOrEmpty(email) )
            {
                signup.Save(email);
                sVM.ShowForm = false;
                sVM.Message = "Thank you! We will contact you soon";
            }

            return View(sVM);
        }
    }
}
