using System;
using cms_api.Models;

namespace thai_kao_mai_api.Models
{
	public class Donate : Identity
    {
		public Donate()
		{
			slip = "";
			accountNumber = "";
            amount = 0.0;
            firstName = "";
            lastName = "";
            email = "";
            phone = "";
            cardID = "";
            donateType = "";
            taxIdNumber = "";
            paymentType = "1";
        }

		public string slip { get; set; }
        public string accountNumber { get; set; }
        public string paymentType { get; set; }
        public double amount { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string cardID { get; set; }
        public string donateType { get; set; }
        public string taxIdNumber { get; set; }
    }
}

