using System;
namespace cms_api.Models
{
    public class PartyFanClub : Identity
    {
        public PartyFanClub()
        {
            idcard = "";
            prefixName = "";
            firstName = "";
            lastName = "";
            birthDay = "";
            phone = "";
            email = "";
      

        }

        public string idcard { get; set; }
        public string prefixName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDay { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
 

    }
}
