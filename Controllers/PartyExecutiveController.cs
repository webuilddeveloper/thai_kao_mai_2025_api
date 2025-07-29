using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class PartyExecutiveController : Controller
    {
        public PartyExecutiveController() { }

        #region main

        // POST /create
        [HttpPost("create")]
        public ActionResult<Response> Create([FromBody] PartyExecutive value)
        {
            try
            {
                new Criteria { code = value.code, title = value.title, updateBy = value.updateBy }.WriteLog("Create", "PartyExecutive");
            }
            catch { }
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("partyExecutive");
                var colRegister = new Database().MongoClient<Register>("register");

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

                    { "sequence", value.sequence },
                    { "category", value.category },
                    //{ "title", value.title },
                    { "title", value.title },
                    { "imageUrl", value.imageUrl },


                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.isActive},
                    { "status", value.isActive ? "A" : "N" },

            };
                col.InsertOne(doc);



                return new Response { status = "S", message = "success", objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                new Criteria { code = value.code, title = value.title, updateBy = value.updateBy }.WriteLog("Read", "PartyExecutive");
            }
            catch { }

            try
            {
                var col = new Database().MongoClient<PartyExecutive>("partyExecutive");
                var filter = Builders<PartyExecutive>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<PartyExecutive>.
                        Filter.Regex("email", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<PartyExecutive>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= (value.permission.filterPermission<PartyExecutive>("category"));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<PartyExecutive>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                        filter &= (value.permission.filterPermission<PartyExecutive>("category"));

                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<PartyExecutive>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<PartyExecutive>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<PartyExecutive>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<PartyExecutive>.Filter.Regex("email", new BsonRegularExpression(string.Format(".*{0}.*", value.email), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<PartyExecutive>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    //if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<PartyExecutive>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<PartyExecutive>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<PartyExecutive>.Filter.Gt("docDate", ds.start) & Builders<PartyExecutive>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<PartyExecutive>.Filter.Gt("docDate", ds.start) & Builders<PartyExecutive>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<PartyExecutive>.Filter.Gt("docDate", de.start) & Builders<PartyExecutive>.Filter.Lt("docDate", de.end); }

                }

                //var docs = col.Find(filter).SortByDescending(o => o.docDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.createDate, c.description, c.descriptionEN, c.titleEN, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.view, c.createTime, c.updateTime, c.docDate, c.docTime, c.category, c.sequence, c.status, c.lv0, c.lv1, c.lv2, c.lv3 }).ToList();

                List<PartyExecutive> docs = col.Aggregate().Match(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit)
                                      .Lookup("partyExecutiveCategory", "category", "code", "categoryList")
                                      .As<PartyExecutive>()
                                      .ToList();

                return new Response { status = "S", message = "success", objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /update
        [HttpPost("update")]
        public ActionResult<Response> Update([FromBody] PartyExecutive value)
        {
            try
            {
                new Criteria { code = value.code, title = value.title, updateBy = value.updateBy }.WriteLog("Update", "PartyExecutive");
            }
            catch (Exception ex)
            {

            }
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "partyExecutive");
                var colRegister = new Database().MongoClient<Register>( "register");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);


                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);

                doc["sequence"] = value.sequence;
                if (!string.IsNullOrEmpty(value.category)) { doc["category"] = value.category; }
                doc["title"] = value.title;
                doc["imageUrl"] = value.imageUrl;

                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";

                col.ReplaceOne(filter, doc);

                if (value.isNotification)
                {
                    new NotificationController().CreateSend(new NotificationSend { username = value.updateBy, category = value.category, reference = value.code });
                    _ = new NotificationController().PushTopics(new Notification { to = "/topics/all", title = value.title, description = value.description, data = new DataPush { page = "NEWS", code = value.code } });
                }

                return new Response { status = "S", message = "success",  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message,  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("delete")]
        public ActionResult<Response> Delete([FromBody] PartyExecutive value)
        {
            try
            {
                var col = new Database().MongoClient( "partyExecutive");

                var codeList = value.code.Split(",");

                foreach (var code in codeList)
                {
                    try
                    {
                        new Criteria { code = code, title = value.title, updateBy = value.updateBy }.WriteLog("Delete", "PartyExecutive");
                    }
                    catch (Exception ex)
                    {

                    }
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
                var col = new Database().MongoClient( "partyExecutiveGallery");


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
                    { "isActive", true },
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success",  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message,  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /create
        [HttpPost("gallery/delete")]
        public ActionResult<Response> GalleryDelete([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "partyExecutiveGallery");

                {
                    //disable all
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("reference", value.code);
                        var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                        col.UpdateMany(filter, update);
                    }
                }

                return new Response { status = "S", message = "success",  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message,  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion

        #region galleryFile

        // POST /create
        [HttpPost("galleryFile/create")]
        public ActionResult<Response> GalleryFileCreate([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("partyExecutiveGalleryFile");


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
                    { "title", value.title },
                    { "imageUrl", value.imageUrl },
                    { "type", value.type },
                    { "size", value.size },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "reference", value.reference },
                    { "isActive", true },
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success",  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message,  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /create
        [HttpPost("galleryFile/delete")]
        public ActionResult<Response> GalleryFileDelete([FromBody] Gallery value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient("partyExecutiveGalleryFile");

                {
                    //disable all
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        var filter = Builders<BsonDocument>.Filter.Eq("reference", value.code);
                        var update = Builders<BsonDocument>.Update.Set("isActive", false).Set("updateBy", value.updateBy).Set("updateDate", value.updateDate);
                        col.UpdateMany(filter, update);
                    }
                }

                return new Response { status = "S", message = "success",  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message,  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #endregion

        #region category

        // POST /create
        [HttpPost("category/create")]
        public ActionResult<Response> CategoryCreate([FromBody] Category value)
        {
            
            var doc = new BsonDocument();

            try
            {
                var col = new Database().MongoClient("partyExecutiveCategory");

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
                    { "sequence", value.sequence },
                    { "language", value.language },
                    { "title", value.title },
                    { "titleEN", value.titleEN },
                    { "imageUrl", value.imageUrl },
                    { "createBy", value.updateBy },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", value.isActive },
                    { "status", value.isActive ? "A" : "N" }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success",  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message,  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>("partyExecutiveCategory");

                var filter = Builders<Category>.Filter.Ne("status", "D");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Category>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= value.permission.filterPermission<Category>("code");
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<Category>.Filter.Eq("title", value.category);
                    else
                        if (value.permission != "all")
                            filter &= value.permission.filterPermission<Category>("code");

                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    //if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }

                }

                var docs = col.Find(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive, c.updateBy, c.updateDate, c.sequence,c.titleEN }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        // POST /update
        [HttpPost("category/update")]
        public ActionResult<Response> CategoryUpdate([FromBody] Category value)
        {
            var doc = new BsonDocument();
            try
            {
                var col = new Database().MongoClient( "partyExecutiveCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                doc = col.Find(filter).FirstOrDefault();
                var model = BsonSerializer.Deserialize<object>(doc);
                if (!string.IsNullOrEmpty(value.title)) { doc["title"] = value.title; }
                if (!string.IsNullOrEmpty(value.title)) { doc["titleEN"] = value.titleEN; }
                if (!string.IsNullOrEmpty(value.imageUrl)) { doc["imageUrl"] = value.imageUrl; }
                if (!string.IsNullOrEmpty(value.language)) { doc["language"] = value.language; }
                doc["sequence"] = value.sequence;
                doc["updateBy"] = value.updateBy;
                doc["updateDate"] = DateTime.Now.toStringFromDate();
                doc["updateTime"] = DateTime.Now.toTimeStringFromDate();
                doc["isActive"] = value.isActive;
                doc["status"] = value.isActive ? "A" : "N";
                col.ReplaceOne(filter, doc);

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("partyExecutive");
                    var filterContent = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updateContent = Builders<BsonDocument>.Update.Set("isActive", false).Set("status", "N");
                    collectionContent.UpdateMany(filterContent, updateContent);
                }
                // ------- end ------

                // ------- update register permission ------
                if (!value.isActive)
                {
                    var collectionPermission = new Database().MongoClient("registerPermission");
                    var filterPermission = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updatePermission = Builders<BsonDocument>.Update.Set("partyExecutivePage", false).Set("isActive", false);
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                // ------- end ------

                return new Response { status = "S", message = "success",  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message,  objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        // POST /delete
        [HttpPost("category/delete")]
        public ActionResult<Response> CategoryDelete([FromBody] Category value)
        {
            try
            {
                var col = new Database().MongoClient( "partyExecutiveCategory");

                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                var update = Builders<BsonDocument>.Update.Set("status", "D").Set("updateBy", value.updateBy).Set("updateDate", DateTime.Now.toStringFromDate());
                col.UpdateOne(filter, update);

                // ------- update content ------
                if (!value.isActive)
                {
                    var collectionContent = new Database().MongoClient("partyExecutive");
                    var filterContent = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updateContent = Builders<BsonDocument>.Update.Set("isActive", false).Set("status", "D");
                    collectionContent.UpdateMany(filterContent, updateContent);
                }
                // ------- end ------

                // ------- update register permission ------
                if (!value.isActive)
                {
                    var collectionPermission = new Database().MongoClient("registerPermission");
                    var filterPermission = Builders<BsonDocument>.Filter.Eq("category", value.code);
                    var updatePermission = Builders<BsonDocument>.Update.Set("partyExecutivePage", false).Set("isActive", false);
                    collectionPermission.UpdateMany(filterPermission, updatePermission);
                }
                // ------- end ------

                return new Response { status = "S", message = $"code: {value.code} is delete" };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        #endregion

        #region report
        // POST /read
        [HttpPost("report/read")]
        public ActionResult<Response> ReportRead([FromBody] Criteria value)
        {
            try
            {

                var col = new Database().MongoClient<PartyExecutive>("partyExecutive");
                var filter = Builders<PartyExecutive>.Filter.Ne("status", "D");

                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<PartyExecutive>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"))) | (filter & Builders<PartyExecutive>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i")));

                    if (value.permission != "all")
                        filter &= (value.permission.filterPermission<PartyExecutive>("category"));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.category))
                        filter &= Builders<PartyExecutive>.Filter.Eq("category", value.category);
                    else
                        if (value.permission != "all")
                        filter &= (value.permission.filterPermission<PartyExecutive>("category"));

                    if (!string.IsNullOrEmpty(value.code)) { filter &= Builders<PartyExecutive>.Filter.Eq("code", value.code); }
                    if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<PartyExecutive>.Filter.Eq("status", value.status); }
                    if (!string.IsNullOrEmpty(value.createBy)) { filter = filter & Builders<PartyExecutive>.Filter.Eq("createBy", value.createBy); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<PartyExecutive>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<PartyExecutive>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description), "i")); }
                    //if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<PartyExecutive>.Filter.Regex("language", value.language); }
                    if (!string.IsNullOrEmpty(value.sequence)) { int sequence = Int32.Parse(value.sequence); filter = filter & Builders<PartyExecutive>.Filter.Eq("sequence", sequence); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<PartyExecutive>.Filter.Gt("docDate", ds.start) & Builders<PartyExecutive>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<PartyExecutive>.Filter.Gt("docDate", ds.start) & Builders<PartyExecutive>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<PartyExecutive>.Filter.Gt("docDate", de.start) & Builders<PartyExecutive>.Filter.Lt("docDate", de.end); }
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Project(c => new { c.code, c.center, c.title, c.category, c.isActive, c.createBy, c.createDate, c.updateBy, c.updateDate, c.status }).ToList();

                var result = new List<object>();

                if (docs.Count() > 0)
                {
                    #region get master
                    // get category.
                    var colCategory = new Database().MongoClient<PartyExecutive>("partyExecutiveCategory");
                    var filterCategory = Builders<PartyExecutive>.Filter.Eq("isActive", true);
                    var category = colCategory.Find(filterCategory).ToList();

                    // get category.
                    var colCenter = new Database().MongoClient<PartyExecutive>("mCenter");
                    var filterCenter = Builders<PartyExecutive>.Filter.Eq("isActive", true);
                    var center = colCenter.Find(filterCenter).Project(c => new { c.code, c.title, c.titleEN }).ToList();

                    // get name user role.
                    var colRole = new Database().MongoClient<registerRole>("registerRole");
                    var filterRole = Builders<registerRole>.Filter.Eq(x => x.isActive, true);
                    //if (!string.IsNullOrEmpty(value.username)) { filter &= Builders<Register>.Filter.Eq("username", c.username); }
                    var registerRole = colRole.Find(filterRole).Project(c => new { c.username, c.category }).ToList();
                    #endregion

                    docs.ForEach(c =>
                    {
                        try
                        {
                            int order = 1;
                            // get category.
                            var titleCategory = category.FirstOrDefault(o => o.code == c.category)?.title;
                            var myregex = new Regex("^.+$");
                            // get category.
                            var titleCenter = center.Where(x => myregex.IsMatch(x.title) || myregex.IsMatch(x.title)).FirstOrDefault()?.title;

                            // get name user role.
                            var docsRole = registerRole.FirstOrDefault(f => f.username == c.createBy);

                            result.Add(new
                            {
                                order = order,
                                title = c.title,
                                category = titleCategory,
                                createBy = c.createBy,
                                createDate = c.createDate,
                                updateBy = c.updateBy,
                                updateDate = c.updateDate,
                                status = c.status,
                                isActive = c.isActive,
                                center = titleCenter,
                                centerCode = c.center,
                            });
                            order++;
                        }
                        catch { }
                    });

                }


                return new Response { status = "S", message = "success", objectData = result, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }
        #endregion
    }
}