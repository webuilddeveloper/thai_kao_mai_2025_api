using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class PartyExecutive : Identity
    {
        public PartyExecutive()
        {
            imageUrl = "";
        
            gallery = new List<Gallery>();
        }

        public string imageUrl { get; set; }
    

        public List<Gallery> gallery { get; set; }
    }
}
