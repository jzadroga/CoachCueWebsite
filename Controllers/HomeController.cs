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
using System.Web.Security;
using System.Configuration;
using CoachCue.Helpers;
using CoachCue.Utility;
using System.Threading.Tasks;
using CoachCue.Repository;
using CoachCue.Models;
using CoachCue.Service;

namespace CoachCue.Controllers
{
    public class HomeController : BaseController
    {
        public string COACHCUE_AUTH_COOKIE = "coachcue_auth";

        public async Task<ActionResult> Index()
        {
            HomeViewModel homeVM = new HomeViewModel();
            await LoadBaseViewModel(homeVM);

            homeVM.ShowWelcome = false;
            homeVM.LoggedIn = false;
            homeVM.LoadNews = false;

            if(!string.IsNullOrEmpty(homeVM.UserData.UserId))
            {
                homeVM.LoggedIn = true;
                await UserService.UpdateLoginStats(homeVM.UserData.UserId);
            }
            else
            {
                //also try the cookie
                HttpCookie cookie = HttpContext.Request.Cookies[COACHCUE_AUTH_COOKIE];
                if (cookie != null)
                {
                    if (!string.IsNullOrEmpty(cookie.Values["userGUID"]))
                    {
                        string userGuid = cookie.Values["userGUID"].ToString();
                        string redirectURL = "~/Home";

                        return Redirect("~/Account/LoginByCookie?usr=" + userGuid + "&url=" + redirectURL);
                    }
                }                
            }

            homeVM.Stream = await StreamService.GetHomeStream(homeVM.UserData);

            return View(homeVM);         
        }

        public async Task<ActionResult> LatestNews()
        {
            HomeViewModel homeVM = new HomeViewModel();
            await LoadBaseViewModel(homeVM);

            homeVM.ShowWelcome = false;
            homeVM.LoggedIn = false;
            homeVM.LoadNews = true;

            if (!string.IsNullOrEmpty(homeVM.UserData.UserId))
            {
                homeVM.LoggedIn = true;
                await UserService.UpdateLoginStats(homeVM.UserData.UserId);
            }
            else
            {
                //also try the cookie
                HttpCookie cookie = HttpContext.Request.Cookies[COACHCUE_AUTH_COOKIE];
                if (cookie != null)
                {
                    if (!string.IsNullOrEmpty(cookie.Values["userGUID"]))
                    {
                        string userGuid = cookie.Values["userGUID"].ToString();
                        string redirectURL = "~/Home";

                        return Redirect("~/Account/LoginByCookie?usr=" + userGuid + "&url=" + redirectURL);
                    }
                }
            }

            homeVM.Stream =  await StreamService.GetTrendingNews();

            return View("Index", homeVM);
            /*return await Task.Run<ActionResult>(() =>
            {
                return RedirectToAction("Index", "Home", new { tb = "y" });
            });*/
        }
    }
}
