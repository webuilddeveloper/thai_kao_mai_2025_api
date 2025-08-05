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
    public class PartyFanClubController : Controller
    {
        public PartyFanClubController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] PartyFanClub value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("partyFanClub");

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

                    //{ "idcard", value.idcard },
                    { "prefixName", value.prefixName },
                    { "firstName", value.firstName },
                    { "lastName", value.lastName },
                    //{ "birthDay", value.birthDay },
                    //{ "phone", value.phone },
                    { "email", value.email},

                    { "volunteerWorkArea", value.volunteerWorkArea},
                    { "thinkTank", value.thinkTank},
                    { "trainingDemocrats", value.trainingDemocrats},
                    { "applyPartyMembership", value.applyPartyMembership},



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
                var col = new Database().MongoClient<PartyFanClub>("partyFanClub");
                var filter = Builders<PartyFanClub>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<PartyFanClub>.Filter.Regex("firstName", value.keySearch)) | (filter & Builders<PartyFanClub>.Filter.Regex("lastName", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<PartyFanClub>.Filter.Eq("code", value.code); }

                }
                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c =>
                new
                {
                    c.code,
                    //c.idcard,
                    c.prefixName,
                    c.firstName,
                    c.lastName,
                    //c.birthDay,
                    //c.phone,
                    c.email,

                    c.volunteerWorkArea,
                    c.thinkTank,
                    c.trainingDemocrats,
                    c.applyPartyMembership,


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
        public ActionResult<Response> Update([FromBody] PartyFanClub value)
        {
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("partyFanClub");
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);

                //if (!string.IsNullOrEmpty(value.idcard)) { doc["idcard"] = value.idcard; }
                if (!string.IsNullOrEmpty(value.prefixName)) { doc["prefixName"] = value.prefixName; }
                if (!string.IsNullOrEmpty(value.firstName)) { doc["firstName"] = value.firstName; }
                if (!string.IsNullOrEmpty(value.lastName)) { doc["lastName"] = value.lastName; }
                //if (!string.IsNullOrEmpty(value.birthDay)) { doc["birthDay"] = value.birthDay; }
                //if (!string.IsNullOrEmpty(value.phone)) { doc["phone"] = value.phone; }
                if (!string.IsNullOrEmpty(value.email)) { doc["email"] = value.email; }

                doc["volunteerWorkArea"] = value.volunteerWorkArea;
                doc["thinkTank"] = value.thinkTank;
                doc["trainingDemocrats"] = value.trainingDemocrats;
                doc["applyPartyMembership"] = value.applyPartyMembership;

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
        public ActionResult<Response> Delete([FromBody] PartyFanClub value)
        {
            try
            {
                var col = new Database().MongoClient("partyFanClub");
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
                var col = new Database().MongoClient("partyFanClubGallery");

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
                var col = new Database().MongoClient("partyFanClubGallery");

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