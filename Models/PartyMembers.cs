using System;
using System.Drawing;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

            religion = "";
            age = 0;
            provinceBirthCode = "";
            provinceBirth = "";
            nationality = "";
            issueDate = "";
            expiryDate = "";
            provinceIssueCode = "";
            provinceIssue = "";
            districtIssueCode = "";
            districtIssue = "";
            highestLevelEducation = "";
            faculty_major = "";
            institute = "";
            currentOccupation = "";
            position = "";
            workplace = "";
            telephone = "";
            fax = "";
            partyRegisterHistory = "";
            partyOldName = "";
            copyIDCard = "";
            copyHouseRegistration = "";
            nameChangeCertificate = "";
            onFilePhoto1_5 = "";
            photoSelfie = "";

            address = "";
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

            partyOfficials = "";
            registerType = "";
            lineID = "";
            slipPay = "";

            isMail = false;
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

        public string religion { get; set; }
        public decimal age { get; set; }
        public string provinceBirthCode { get; set; }
        public string provinceBirth { get; set; }

        public string nationality { get; set; }
        public string issueDate { get; set; }
        public string expiryDate { get; set; }
        public string provinceIssueCode { get; set; }
        public string provinceIssue { get; set; }
        public string districtIssueCode { get; set; }
        public string districtIssue { get; set; }
        public string highestLevelEducation { get; set; }
        public string faculty_major { get; set; }
        public string institute { get; set; }
        public string currentOccupation { get; set; }
        public string position { get; set; }
        public string workplace { get; set; }
        public string telephone { get; set; }
        public string fax { get; set; }
        public string partyRegisterHistory { get; set; }
        public string partyOldName { get; set; }
        public string copyIDCard { get; set; }
        public string copyHouseRegistration { get; set; }
        public string nameChangeCertificate { get; set; }
        public string onFilePhoto1_5 { get; set; }
        public string photoSelfie { get; set; }

        public string address { get; set; }
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

        public string partyOfficials { get; set; }
        public string registerType { get; set; }
        public string lineID { get; set; }
        public string slipPay { get; set; }

        public bool isMail { get; set; }
    }
}
