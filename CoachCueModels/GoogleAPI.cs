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
    class GoogleAPI
    {
        const string GOOGLE_API_KEY = "AIzaSyBLdXn9SLZqOYuE0Ij2t2lhhYQvFO70lpc";
        
        public static string GetShortUrl(string url)
        {
            string shortUrl = url;

            try
            {
                WebRequest request = WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=" + GOOGLE_API_KEY);
                request.Method = "POST";
                request.ContentType = "application/json";
                string requestData = string.Format(@"{{""longUrl"": ""{0}""}}", url);
                byte[] requestRawData = Encoding.ASCII.GetBytes(requestData);
                request.ContentLength = requestRawData.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(requestRawData, 0, requestRawData.Length);
                requestStream.Close();

                WebResponse response = request.GetResponse();
                StreamReader responseReader = new StreamReader(response.GetResponseStream());
                string responseData = responseReader.ReadToEnd();
                responseReader.Close();

                var deserializer = new JavaScriptSerializer();
                var results = deserializer.Deserialize<GoogleResponse>(responseData);
                shortUrl = results.Id;
            }
            catch (Exception) { }

            return shortUrl;
        }
    }

    public class GoogleResponse
    {
        public string Kind { get; set; }
        public string Id { get; set; }
        public string LongUrl { get; set; }
    }
}
