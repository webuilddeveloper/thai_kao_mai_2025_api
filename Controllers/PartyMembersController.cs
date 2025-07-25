using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
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
                    { "imageUrl", value.imageUrl },
                    { "imageIdCardUrl", value.imageIdCardUrl },

                    { "address", value.address },
                    { "village", value.village },
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

                    { "membershipType", value.membershipType },
                    { "imagePaymentUrl", value.imagePaymentUrl },



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
                    c.imageUrl,
                    c.imageIdCardUrl,

                    c.address,
                    c.village,
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

                    c.membershipType,
                    c.imagePaymentUrl,

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
                doc["village"] = value.village;
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

                doc["membershipType"] = value.membershipType;
                if (!string.IsNullOrEmpty(value.imagePaymentUrl)) { doc["imagePaymentUrl"] = value.imagePaymentUrl; }

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

    }
}