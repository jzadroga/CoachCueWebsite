﻿using System;
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
using CoachCue.Service;

namespace CoachCue.Controllers
{
    public class SearchController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            SearchResultViewModel srvm = new SearchResultViewModel();
            await LoadBaseViewModel(srvm);

            srvm.SearchTerm = string.Empty;
            srvm.Trending = await StreamService.GetTrendingStream();

            return View(srvm);
        }
    }
}
