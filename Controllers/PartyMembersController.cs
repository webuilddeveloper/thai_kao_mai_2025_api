using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class PartyMembersController : Controller
    {
        public PartyMembersController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] PartyMembers value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("partyMembers");

                //check duplicate
                value.code = "".toCode();
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                var filterIDCard = Builders<BsonDocument>.Filter.Eq("idcard", value.idcard) & Builders<BsonDocument>.Filter.Ne("status", "D");
                if (col.Find(filterIDCard).Any())
                {

                    return new Response { status = "N", message = "เลขบัตรปนะชาชนนี้มีอยู่ในระบบแล้ว" };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "idcard", value.idcard },
                    { "prefixName", value.prefixName },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    { "birthDay", value.birthDay },
                    { "phone", value.phone },
                    { "email", value.email},

                    { "religion", value.religion},
                    { "age", value.age},
                    { "provinceBirthCode", value.provinceBirthCode},
                    { "provinceBirth", value.provinceBirth},
                    { "nationality", value.nationality},
                    { "issueDate", value.issueDate},
                    { "expiryDate", value.expiryDate},
                    { "provinceIssueCode", value.provinceIssueCode},
                    { "provinceIssue", value.provinceIssue},
                    { "districtIssueCode", value.districtIssueCode},
                    { "districtIssue", value.districtIssue},
                    { "highestLevelEducation", value.highestLevelEducation},
                    { "faculty_major", value.faculty_major},
                    { "institute", value.institute},
                    { "currentOccupation", value.currentOccupation},
                    { "partyOfficials", value.partyOfficials},
                    
                    { "position", value.position},
                    { "workplace", value.workplace},
                    { "telephone", value.telephone},
                    { "fax", value.fax},
                    { "partyRegisterHistory", value.partyRegisterHistory},
                    { "partyOldName", value.partyOldName},
                    { "copyIDCard", value.copyIDCard},
                    { "copyHouseRegistration", value.copyHouseRegistration},
                    { "nameChangeCertificate", value.nameChangeCertificate},
                    { "onFilePhoto1_5", value.onFilePhoto1_5},
                    { "photoSelfie", value.photoSelfie},

                    { "address", value.address },
                    { "moo", value.moo },
                    { "soi", value.soi },
                    { "road", value.road },
                    { "provinceCode", value.provinceCode },
                    { "province", value.province },
                    { "amphoeCode", value.amphoeCode },
                    { "amphoe", value.amphoe },
                    { "tambonCode", value.tambonCode },
                    { "tambon", value.tambon },
                    { "postnoCode", value.postnoCode },

                    { "registerType", value.registerType },
                    { "lineID", value.lineID },
                    { "slipPay", value.slipPay },

                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.status == "A" ? true: false },
                    { "status", value.status },
                };
                col.InsertOne(doc);

                Task.Run(async () =>
                {
                    var pdfBytes = await getReportAsync(value);
                    new SendMailService($"https://gateway.we-builds.com/thai-kao-mai-api/partyMembers/updateVerify?code={value.code}&email={value.email}"
                        , "ยืนยันตัวตน"
                        , value.email
                        , $"{value.title} {value.firstName} {value.lastName}"
                        , pdfBytes);
                });

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<PartyMembers>("partyMembers");
                var filter = Builders<PartyMembers>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<PartyMembers>.Filter.Regex("firstName", value.keySearch)) | (filter & Builders<PartyMembers>.Filter.Regex("lastName", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<PartyMembers>.Filter.Eq("code", value.code); }

                }
                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c =>
                new
                {
                    c.code,
                    c.idcard,
                    c.prefixName,
                    c.firstName,
                    c.lastName,
                    c.birthDay,
                    c.phone,
                    c.email,

                    c.religion,
                    c.age,
                    c.provinceBirthCode,
                    c.provinceBirth,
                    c.nationality,
                    c.issueDate,
                    c.expiryDate,
                    c.provinceIssueCode,
                    c.provinceIssue,
                    c.districtIssueCode,
                    c.districtIssue,
                    c.highestLevelEducation,
                    c.faculty_major,
                    c.institute,
                    c.currentOccupation,
                    c.position,
                    c.workplace,
                    c.telephone,
                    c.fax,
                    c.partyRegisterHistory,
                    c.partyOldName,
                    c.copyIDCard,
                    c.copyHouseRegistration,
                    c.nameChangeCertificate,
                    c.onFilePhoto1_5,
                    c.partyOfficials,
                    c.photoSelfie,

                    c.address,
                    c.moo,
                    c.soi,
                    c.road,
                    c.provinceCode,
                    c.province,
                    c.amphoeCode,
                    c.amphoe,
                    c.tambonCode,
                    c.tambon,
                    c.postnoCode,

                    c.registerType,
                    c.lineID,
                    c.slipPay,
                    
                    c.createBy,
                    c.createDate,
                    c.createTime,
                    c.updateBy,
                    c.updateDate,
                    c.updateTime,
                    c.docDate,
                    c.docTime,
                    c.isActive,
                    c.status,
                }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] PartyMembers value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("partyMembers");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);

                if (!string.IsNullOrEmpty(value.idcard)) { doc["idcard"] = value.idcard; }
                if (!string.IsNullOrEmpty(value.prefixName)) { doc["prefixName"] = value.prefixName; }
                if (!string.IsNullOrEmpty(value.firstName)) { doc["firstName"] = value.firstName; }
                if (!string.IsNullOrEmpty(value.lastName)) { doc["lastName"] = value.lastName; }
                if (!string.IsNullOrEmpty(value.birthDay)) { doc["birthDay"] = value.birthDay; }
                if (!string.IsNullOrEmpty(value.phone)) { doc["phone"] = value.phone; }
                if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.imageIdCardUrl)) { doc["imageIdCardUrl"] = value.imageIdCardUrl; }


                doc["address"] = value.address;
                doc["moo"] = value.moo;
                doc["soi"] = value.soi;
                doc["road"] = value.road;
                doc["provinceCode"] = value.provinceCode;
                doc["province"] = value.province;
                doc["amphoeCode"] = value.amphoeCode;
                doc["amphoe"] = value.amphoe;
                doc["tambonCode"] = value.tambonCode;
                doc["tambon"] = value.tambon;
                doc["postnoCode"] = value.postnoCode;

                doc["religion"] = value.religion;
                doc["age"] = value.age;
                if (!string.IsNullOrEmpty(value.provinceBirthCode)) { doc["provinceBirthCode"] = value.provinceBirthCode; }
                if (!string.IsNullOrEmpty(value.provinceBirth)) { doc["provinceBirth"] = value.provinceBirth; }
                doc["nationality"] = value.nationality;
                doc["issueDate"] = value.issueDate;
                doc["expiryDate"] = value.expiryDate;
                if (!string.IsNullOrEmpty(value.provinceIssueCode)) { doc["provinceIssueCode"] = value.provinceIssueCode; }
                if (!string.IsNullOrEmpty(value.provinceIssue)) { doc["provinceIssue"] = value.provinceIssue; }
                if (!string.IsNullOrEmpty(value.districtIssueCode)) { doc["districtIssueCode"] = value.districtIssueCode; }
                if (!string.IsNullOrEmpty(value.districtIssue)) { doc["districtIssue"] = value.districtIssue; }
                doc["highestLevelEducation"] = value.highestLevelEducation;
                if (!string.IsNullOrEmpty(value.faculty_major)) { doc["faculty_major"] = value.faculty_major; }
                doc["institute"] = value.institute;
                doc["currentOccupation"] = value.currentOccupation;
                doc["position"] = value.position;
                doc["workplace"] = value.workplace;
                doc["telephone"] = value.telephone;
                doc["fax"] = value.fax;
                doc["partyRegisterHistory"] = value.partyRegisterHistory;
                if (!string.IsNullOrEmpty(value.partyOldName)) { doc["partyOldName"] = value.partyOldName; }
                doc["copyIDCard"] = value.copyIDCard;
                doc["copyHouseRegistration"] = value.copyHouseRegistration;
                doc["nameChangeCertificate"] = value.nameChangeCertificate;
                doc["onFilePhoto1_5"] = value.onFilePhoto1_5;
                if (!string.IsNullOrEmpty(value.partyOfficials)) { doc["partyOfficials"] = value.partyOfficials; }
                if (!string.IsNullOrEmpty(value.registerType)) { doc["registerType"] = value.registerType; }
                if (!string.IsNullOrEmpty(value.lineID)) { doc["lineID"] = value.lineID; }
                if (!string.IsNullOrEmpty(value.slipPay)) { doc["slipPay"] = value.slipPay; }
                if (!string.IsNullOrEmpty(value.photoSelfie)) { doc["photoSelfie"] = value.photoSelfie; }

                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.status == "A" ? true : false;
                doc["status"] = value.status;

                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] PartyMembers value)
        {
            try
            {
                var col = new Database().MongoClient("partyMembers");
                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {

                    var filter = Builders<BsonDocument>.Filter.Eq("code", code);
                    var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate()).Set("updateTime", DateTime.Now.toTimeStringFromDate());
                    col.UpdateOne(filter, update);

                }

                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region gallery

        // POST /create
        [HttpPost("gallery/create")]
        public ActionResult<Response> GalleryCreate([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("partyMembersGallery");

                value.code = "".toCode();

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "imageUrl", value.imageUrl },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "reference", value.reference },
                    { "isActive", true }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /create
        [HttpPost("gallery/delete")]
        public ActionResult<Response> GalleryDelete([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("partyMembersGallery");

                {
                    //disable all
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("reference", value.code);
                        var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                        col.UpdateMany(filter, update);
                    }
                }

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion

        [HttpGet("memberCount")]
        public ActionResult<Response> memberCount()
        {
            try
            {
                //new SendMailService("ยืนยันตัวตนอีกครั้ง", "ยืนยันตัวตน", "");
                var col = new Database().MongoClient<PartyMembers>("partyMembers");
                var filter = Builders<PartyMembers>.Filter.Eq("status", "A");

                var docs = col.CountDocuments(filter);

                return new Response { status = "S", message = "success", objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        [HttpGet("testEmail")]
        public ActionResult<Response> testEmail()
        {
            try
            {
                new SendMailService("https://www.google.com/", "คุณได้สมัครเป็นสมาชิกพรรคไทยก้าวใหม่ กรุณากดปุ่มด้านล่างเพื่อยืนยันการสมัคร", "");

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        [HttpGet("updateVerify")]
        public ActionResult<Response> updateVerify([FromQuery] string code, [FromQuery] string email)
        {
            var doc = new BsonDocument();
            try
            {

                var col = new Database().MongoClient<BsonDocument>("partyMembers");
                var filter = (Builders<BsonDocument>.Filter.Eq("code", code) & Builders<BsonDocument>.Filter.Eq("email", email));

                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);

                doc["isVerify"] = true;

                col.ReplaceOne(filter, doc);

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        private async Task<byte[]> getReportAsync(PartyMembers data)
        {
            var date = DateTime.Now;
            var thaiCulture = new CultureInfo("th-TH");
            // 1. เตรียม object ที่จะส่งเข้า API
            var requestObject = new
            {
                day = date.ToString("dd", thaiCulture),
                month = date.ToString("MMMM", thaiCulture),
                year = (date.Year + 543).ToString(),
                fullName = $"{data.firstName} {data.lastName}",
                address = data.address,
                soi = data.soi,
                road = data.road,
                moo = data.moo,
                tambon = data.tambon,
                amphoe = data.amphoe,
                province = data.province,
                postnoCode = data.postnoCode,
                isYearly = data.registerType == "yearly" ? true : false,
                isLifetime = data.registerType == "lifetime" ? true : false,
                cash = data.registerType == "yearly" ? "20" : "200",
                cashDate = "01/09/2567",
                no = "",
                number = "",
                idcard = data.idcard,
                money = data.registerType == "yearly" ? "20" : "200",
                moneyth = data.registerType == "yearly" ? "ยี่สิบบาทถ้วน" : "สองร้อยบาทถ้วน",
            };

        var httpClient = new HttpClient();

            // ถ้าต้องใช้ Authorization
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_token");

            // แปลง object เป็น JSON string
            string jsonContent = JsonSerializer.Serialize(requestObject);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // 2. เรียก API แบบ POST
            //var response = await httpClient.PostAsync("https://gateway.we-builds.com/report-wb-api/api/report/getReportTKM", content);
            var response = await httpClient.PostAsync("http://localhost:8080/wb-api/api/report/getReportTKM", content);
            response.EnsureSuccessStatusCode();

            // 3. รับ PDF เป็น byte[]
            byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();
            
            return pdfBytes;
        }
    }
}