using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;
using CoachCue.ViewModels;
using CoachCue.Service;
using System.Threading.Tasks;
using CoachCue.Repository;

namespace CoachCue.Controllers
{
    public class CoachController : BaseController
    {
        public async Task<ActionResult> Index(string name)
        {
            UserViewModel userVM = new UserViewModel();

            userVM.UserData = await CoachCueUserData.GetUserData(User.Identity.Name);

            userVM.UserDetail = await UserService.GetByLink(name);
            userVM.UserStream = await StreamService.GetUserStream(userVM.UserData, userVM.UserDetail.Id, false);
            return View(userVM);
        }

        public async Task<ActionResult> Matchup(string name)
        {
            UserViewModel userVM = new UserViewModel();

            userVM.UserData = await CoachCueUserData.GetUserData(User.Identity.Name);
            userVM.UserDetail = await UserService.GetByLink(name);
            userVM.UserStream = await StreamService.GetUserStream(userVM.UserData, userVM.UserDetail.Id, true);

            return View("Index", userVM);
        }
    }
}
