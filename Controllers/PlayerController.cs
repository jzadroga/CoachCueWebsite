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
using System.Threading.Tasks;
using CoachCue.Service;
using CoachCue.Repository;

namespace CoachCue.Controllers
{
    public class PlayerController : BaseController
    {
        public async Task<ActionResult> Index(string team, string name)
        {
            PlayerViewModel playerVM = new PlayerViewModel();
            await LoadBaseViewModel(playerVM);

            playerVM.LoggedIn = false;
            playerVM.PlayerDetail = await PlayerService.GetByDescription(team, name);
            if (playerVM.PlayerDetail == null) //can't find the player
                return RedirectToAction("Index", "Home");

            playerVM.PlayerStream = await StreamService.GetPlayerStream(playerVM.UserData, playerVM.PlayerDetail.Id);

            return View(playerVM);
        }
    }
}
