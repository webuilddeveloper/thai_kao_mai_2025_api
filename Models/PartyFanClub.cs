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
            other = "";
            lineId = "";
        }

        public string idcard { get; set; }
        public string prefixName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDay { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string lineId { get; set; }

        public bool brainBank { get; set; }
        public bool applySenator { get; set; }
        public bool applyMpConstituency { get; set; }
        public bool applyMpPartylist { get; set; }
        public bool volunteerField { get; set; }
        public bool joinWorkshop { get; set; }
        public string other { get; set; }

    }
}
