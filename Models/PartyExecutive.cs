using System;
using System.Collections.Generic;

namespace cms_api.Models
{
    public class PartyExecutive : Identity
    {
        public PartyExecutive()
        {
            imageUrl = "";
            view = 0;
            gallery = new List<Gallery>();
        }

        public string imageUrl { get; set; }
        public int view { get; set; }


        public List<Gallery> gallery { get; set; }
    }
}
