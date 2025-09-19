using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class AboutUs : Identity
    {
        public AboutUs()
        {
            imageLogoUrl = "";
            imageBgUrl = "";
            address = "";
            addressEN = "";
            email = "";
            telephone = "";
            site = "";
            youtube = "";
            facebook = "";
            ig = "";
            tiktok = "";
            x = "";
            latitude = "";
            longitude = "";
            lineOfficial = "";
            vision = "";
            visionEN = "";
            mission = "";
            missionEN = "";
            ideologyDes = "";
            ideologyDesEN = "";
            ideologyList = new List<Ideology>();
        }

        public string imageLogoUrl { get; set; }
        public string imageBgUrl { get; set; }
        public string address { get; set; }
        public string addressEN { get; set; }
        public string email { get; set; }
        public string telephone { get; set; }
        public string site { get; set; }
        public string youtube { get; set; }
        public string facebook { get; set; }
        public string ig { get; set; }
        public string tiktok { get; set; }
        public string x { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string lineOfficial { get; set; }
        public string vision { get; set; }
        public string visionEN { get; set; }
        public string mission { get; set; }
        public string missionEN { get; set; }
        public string ideologyDes { get; set; }
        public string ideologyDesEN { get; set; }
        public string membershipApplication { get; set; }
        public List<Ideology> ideologyList { get; set; }

       
    }

    public class Ideology
    {
        public int sequence { get; set; }
        public string title { get; set; }
        public string titleEN { get; set; }
        public string description { get; set; }
        public string descriptionEN { get; set; }
    }
}
