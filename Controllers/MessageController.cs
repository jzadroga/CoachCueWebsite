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
using System.Threading.Tasks;
using CoachCue.Repository;
using CoachCue.Service;

namespace CoachCue.Controllers
{
    public class MessageController : BaseController
    {
        public async Task<ActionResult> Index(string id)
        {
            UserMessageModel msgVM = new UserMessageModel();
            await LoadBaseViewModel(msgVM);

            msgVM.Message = await StreamService.GetMessageDetailStream(msgVM.UserData, id);

            if (msgVM.Message.MessageItem == null)
                return RedirectToAction("Index", "Home");

            return View(msgVM);
        }  
    }
}
