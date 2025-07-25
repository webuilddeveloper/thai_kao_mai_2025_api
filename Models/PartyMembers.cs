using System;
namespace cms_api.Models
{
    public class PartyMembers : Identity
    {
        public PartyMembers()
        {
            idcard = "";
            prefixName = "";
            firstName = "";
            lastName = "";
            birthDay = "";
            phone = "";
            email = "";
            imageUrl = "";
            imageIdCardUrl = "";

            address = "";
            village = "";
            moo = "";
            soi = "";
            road = "";
            provinceCode = "";
            province = "";
            amphoeCode = "";
            amphoe = "";
            tambonCode = "";
            tambon = "";
            postnoCode = "";

            imagePaymentUrl = "";
        }

        public string idcard { get; set; }
        public string prefixName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDay { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string imageUrl { get; set; }
        public string imageIdCardUrl { get; set; }

        public string address { get; set; }
        public string village { get; set; }
        public string moo { get; set; }
        public string soi { get; set; }
        public string road { get; set; }
        public string provinceCode { get; set; }
        public string province { get; set; }
        public string amphoeCode { get; set; }
        public string amphoe { get; set; }
        public string tambonCode { get; set; }
        public string tambon { get; set; }
        public string postnoCode { get; set; }

        public string membershipType { get; set; }
        public string imagePaymentUrl { get; set; }
    }
}
