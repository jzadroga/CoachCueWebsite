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
            routes.LowercaseUrls = true;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               "Matchups",
               "Matchup/List/{week}",
               new { controller = "Matchup", action = "List" }
            );

            routes.MapRoute(
               "Players",
               "Player/{team}/{name}",
               new { controller = "Player", action = "Index" }
            );

            routes.MapRoute(
                "WhoDoIAddWaiverWire",
                "WhoDoIAddWaiverWire/{week}/{players}",
                new { controller = "WhoDoIAddWaiverWire", action = "Index" }
            );

            routes.MapRoute(
               "WhoDoIDraft",
               "WhoDoIDraft/{week}/{players}",
               new { controller = "WhoDoIDraft", action = "Index" }
            );

            routes.MapRoute(
               "WhoDoIDropWaiverWire",
               "WhoDoIDropWaiverWire/{week}/{players}",
               new { controller = "WhoDoIDropWaiverWire", action = "Index" }
            );

            routes.MapRoute(
              "WhoDoIKeep",
              "WhoDoIKeep/{week}/{players}",
              new { controller = "WhoDoIKeep", action = "Index" }
           );

            routes.MapRoute(
             "WhoDoIStartDailyFantasy",
             "WhoDoIStartDailyFantasy/{week}/{players}",
             new { controller = "WhoDoIStartDailyFantasy", action = "Index" }
           );

            routes.MapRoute(
             "WhoDoIStartPPR",
             "WhoDoIStartPPR/{week}/{players}",
             new { controller = "WhoDoIStartPPR", action = "Index" }
           );

            routes.MapRoute(
              "WhoDoIStartStandard",
              "WhoDoIStartStandard/{week}/{players}",
              new { controller = "WhoDoIStartStandard", action = "Index" }
            );

            routes.MapRoute(
              "PlayerPods",
              "PlayerPod/{id}/{name}",
              new { controller = "PlayerPod", action = "Index" }
           );

            routes.MapRoute(
              "Coaches",
              "Coach/{name}",
              new { controller = "Coach", action = "Index" }
           );

            routes.MapRoute(
              "CoacheMatchups",
              "Coach/Matchup/{name}",
              new { controller = "Coach", action = "Matchup" }
           );

            routes.MapRoute(
              "Followers",
              "Followers/{id}/{type}/{name}",
              new { controller = "Followers", action = "Index" }
           );

            routes.MapRoute(
              "Mesages",
              "Message/{id}",
              new { controller = "Message", action = "Index" }
           );

            routes.MapRoute(
               "Default", // Route name
               "{controller}/{action}/{playerName}", // URL with parameters
               new { controller = "Home", action = "Index", playerName = UrlParameter.Optional } // Parameter defaults
           );
        }
    }
}