using CoachCue.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoachCue.Models
{
    public class Player
    {
        public Player()
        {
            this.BeatWriters = new List<string>();
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "shortName")]
        public string ShortName
        {
            get { return this.FirstName.Substring(0, 1) + ". " + this.LastName; }
        }

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get { return this.FirstName + " " + this.LastName; }
        }

        [JsonProperty(PropertyName = "value")]
        public string Value
        {
            get { return this.FirstName + " " + this.LastName; }
        }

        [JsonProperty(PropertyName = "position")]
        public string Position { get; set; }

        [JsonProperty(PropertyName = "number")]
        public int? Number { get; set; }

        [JsonProperty(PropertyName = "isActive")]
        public bool Active { get; set; }    

        [JsonProperty(PropertyName = "twitter")]
        public TwitterAccount Twitter { get; set; }

        [JsonProperty(PropertyName = "team")]
        public Team Team { get; set; }

        [JsonProperty(PropertyName = "profileImage")]
        public string ProfileImage
        {
            get
            {
                var image = "/assets/img/teams/" + this.Team.Slug + ".jpg";
                if (!string.IsNullOrEmpty(this.Twitter.ProfileImage))
                {
                    image = this.Twitter.ProfileImage.Replace("http", "https");
                }

                return image;
            }
        }

        [JsonProperty(PropertyName = "profileImageLarge")]
        public string ProfileImageLarge
        {
            get
            {
                return this.ProfileImage.Replace("normal", "bigger");
            }
        }

        [JsonProperty(PropertyName = "link")]
        public string Link
        {
            get
            {
                string name = this.Name.Replace(".", "").Replace("-", "").Replace("'", "");
                
                //finally seperate first and last by -
                return name.Replace(" ", "-").ToLower();
            }
        }

        [JsonProperty(PropertyName = "beatWriters")]
        public List<string> BeatWriters { get; set; }
    }

    public class VotedPlayer
    {
        public Player Player { get; set; }
        public int Votes { get; set; }
        public string Percent { get; set; }
    }
}