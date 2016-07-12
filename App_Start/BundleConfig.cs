using System.Web;
using System.Web.Optimization;

namespace CoachCue
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/assets/js/bundle").Include(
                     "~/assets/js/hogan.js",
                     "~/assets/js/mention.js",
                      "~/assets/js/main.js",
                      "~/assets/js/spin.js",
                      "~/assets/js/helpers.js",
                      "~/assets/js/tasks.js"));
            
            bundles.Add(new StyleBundle("~/assets/css/bundle").Include(
                      "~/assets/css/bootstrap-tagsinput.css",
                      "~/assets/css/typeahead.css",
                      "~/assets/css/global.css"));
        }
    }
}
