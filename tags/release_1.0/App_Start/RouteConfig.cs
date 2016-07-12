using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CoachCue
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               "Matchups",
               "Matchup/List/{week}",
               new { controller = "Matchup", action = "List" }
            );

            routes.MapRoute(
               "LeaderBoards",
               "Coach/LeaderBoard/{week}",
               new { controller = "Coach", action = "LeaderBoard" }
            );

            routes.MapRoute(
               "Players",
               "Player/{id}/{name}",
               new { controller = "Player", action = "Index" }
            );

            routes.MapRoute(
              "PlayerPods",
              "PlayerPod/{id}/{name}",
              new { controller = "PlayerPod", action = "Index" }
           );

            routes.MapRoute(
              "Coaches",
              "Coach/{id}/{name}",
              new { controller = "Coach", action = "Index" }
           );

            routes.MapRoute(
              "Followers",
              "Followers/{id}/{type}/{name}",
              new { controller = "Followers", action = "Index" }
           );

            routes.MapRoute(
               "Default", // Route name
               "{controller}/{action}/{playerName}", // URL with parameters
               new { controller = "Home", action = "Index", playerName = UrlParameter.Optional } // Parameter defaults
           );
        }
    }
}