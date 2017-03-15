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
using CoachCue.Service;
using System.Threading.Tasks;

namespace CoachCue.Controllers
{
    public class PlayerPodController : Controller
    {
        public async Task<ActionResult> Index(string id, string name)
        {
            PlayerViewModel playerVM = new PlayerViewModel();
            playerVM.LoggedIn = false;

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            playerVM.PlayerDetail = await PlayerService.Get(id);
            playerVM.PlayerStream = new List<StreamContent>(); //stream.GetPlayerStream(id, userID, false).Take(12).ToList();

            return View(playerVM);
        }

        public ActionResult Sample()
        {
            return View();
        }
    }
}
