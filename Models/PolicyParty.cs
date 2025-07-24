using System;
using cms_api.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace thai_kao_mai_api.Models
{
    [BsonIgnoreExtraElements]
    public class PolicyParty : Identity
    {
        public PolicyParty()
        {
            status2 = "";
            imageUrl = "";
            imageBanner = "";
            imageUrlCreateBy = "";
        }

        public string status2 { get; set; }
        public string imageUrl { get; set; }
        public string imageBanner { get; set; }
        public string imageUrlCreateBy { get; set; }
        public int view { get; set; }
        public int progress { get; set; }
    }
}

