using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CoachCue.Model
{
    class OpenGraph
    {
        public static OpenGraphResponse Parse(string url)
        {
            OpenGraphResponse ogResponse = new OpenGraphResponse();
            ogResponse.IsOpenGraph = false;

            try
            {
                //get the roster from nfl.com and parse the html
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Load data into a htmlagility doc   
                    var receiveStream = response.GetResponseStream();
                    if (receiveStream != null)
                    {
                        var stream = new StreamReader(receiveStream);
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.Load(stream);

                        foreach (HtmlNode metaRow in htmlDoc.DocumentNode.SelectNodes("//head/meta/@property"))
                        {
                            string propertyType = metaRow.GetAttributeValue("property", string.Empty);
                            switch( propertyType.ToLower() )
                            {
                                case "og:title":
                                    ogResponse.IsOpenGraph = true;
                                    ogResponse.Title = metaRow.GetAttributeValue("content", string.Empty);
                                    break;
                                case "og:url":
                                    ogResponse.IsOpenGraph = true;
                                    ogResponse.URL = metaRow.GetAttributeValue("content", string.Empty);
                                    break;
                                case "og:image":
                                case "og:image:url":
                                    ogResponse.IsOpenGraph = true;
                                    ogResponse.Image = metaRow.GetAttributeValue("content", string.Empty);
                                    ogResponse.MediaTypeID = 2;
                                    break;
                                case "og:type":
                                    ogResponse.IsOpenGraph = true;
                                    ogResponse.Type = metaRow.GetAttributeValue("content", string.Empty);
                                    break;
                                case "og:site_name":
                                    ogResponse.IsOpenGraph = true;
                                    ogResponse.SiteName = metaRow.GetAttributeValue("content", string.Empty);
                                    break;
                                case "og:video":
                                    ogResponse.MediaTypeID = 1;
                                    ogResponse.IsOpenGraph = true;
                                    ogResponse.Video = metaRow.GetAttributeValue("content", string.Empty);
                                    break;
                                case "og:description":
                                    ogResponse.IsOpenGraph = true;
                                    ogResponse.Description = metaRow.GetAttributeValue("content", string.Empty);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                string err = ex.Message;
            }

            return ogResponse;
        }
    }

    public class OpenGraphResponse
    {
        public int MediaTypeID { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public string URL { get; set; }
        public string SiteName { get; set; }
        public string Video { get; set; }
        public bool IsOpenGraph { get; set; }
        public string Description { get; set; }

        public OpenGraphResponse()
        {
            this.MediaTypeID = 0;
        }
    }
}
