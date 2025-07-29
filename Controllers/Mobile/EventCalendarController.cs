using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace mobile_api.Controllers
{
    [Route("m/[controller]")]
    public class EventCalendarController : Controller
    {
        public EventCalendarController() { }

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {
            try
            {
                //value.statisticsCreateAsync("eventCalendar");
                var col = new Database().MongoClient<EventCalendar>("eventCalendar");
                var filter = Builders<EventCalendar>.Filter.Eq("status", "A");
                //var filter = (Builders<EventCalendar>.Filter.Eq("status", "A"));
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = filter & Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.keySearch), "i"));

                    //BEGIN : Statistic
                    try
                    {
                        var value1 = new Criteria();

                        value1.title = value.keySearch;
                        value1.updateBy = value.updateBy;
                        value1.platform = value.platform;

                        if (!string.IsNullOrEmpty(value.code))
                            value1.reference = value.code;

                        value1.statisticsCreateAsync("eventCalendarKeySearch");
                    }
                    catch { }
                    //END : Statistic
                }

                if (!string.IsNullOrEmpty(value.code))
                {
                    filter = filter & Builders<EventCalendar>.Filter.Eq("code", value.code);

                    //new NotificationController().Update(new Notification
                    //{
                    //    code = value.code,
                    //    profileCode = value.profileCode
                    //});
                }
                if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<EventCalendar>.Filter.Regex("title", new BsonRegularExpression(string.Format(".*{0}.*", value.title), "i")); }
                if (!string.IsNullOrEmpty(value.category)) { filter = filter & Builders<EventCalendar>.Filter.Eq("category", value.category); }
                if (!string.IsNullOrEmpty(value.status)) { filter = filter & Builders<EventCalendar>.Filter.Eq("status", value.status); }
                if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<EventCalendar>.Filter.Regex("description", new BsonRegularExpression(string.Format(".*{0}.*", value.description))); }
                if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<EventCalendar>.Filter.Regex("language", value.language); }
                if (value.isHighlight) { filter = filter & Builders<EventCalendar>.Filter.Eq("isHighlight", value.isHighlight); }

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", ds.start) & Builders<EventCalendar>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<EventCalendar>.Filter.Gt("docDate", de.start) & Builders<EventCalendar>.Filter.Lt("docDate", de.end); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));


                List<EventCalendar> docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.docDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new EventCalendar
                {
                    code = c.code,
                    isActive = c.isActive,
                    createBy = c.createBy,
                    imageUrlCreateBy = c.imageUrlCreateBy,
                    createDate = c.createDate,
                    description = c.description,
                    imageUrl = c.imageUrl,
                    title = c.title,
                    language = c.language,
                    updateBy = c.updateBy,
                    updateDate = c.updateDate,
                    category = c.category,
                    confirmStatus = c.confirmStatus,
                    linkFacebook = c.linkFacebook,
                    linkYoutube = c.linkYoutube,
                    dateStart = c.dateStart,
                    dateEnd = c.dateEnd,
                    view = c.view,
                    linkUrl = c.linkUrl,
                    textButton = c.textButton,
                    fileUrl = c.fileUrl,
                }).ToList();

                //BEGIN :update view >>>>>>>>>>>>>>>>>>>>>>>>>>>>
                if (!string.IsNullOrEmpty(value.code))
                {
                    var view = docs[0].view;

                    var doc = new BsonDocument();
                    var colUpdate = new Database().MongoClient("eventCalendar");

                    var filterUpdate = Builders<BsonDocument>.Filter.Eq("code", value.code);
                    doc = colUpdate.Find(filterUpdate).FirstOrDefault();
                    var model = BsonSerializer.Deserialize<object>(doc);
                    doc["view"] = view + 1;
                    colUpdate.ReplaceOne(filterUpdate, doc);

                    docs[0].view += 1;

                    //docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.isActive, c.createBy, c.imageUrlCreateBy, c.createDate, c.description, c.imageUrl, c.title, c.language, c.updateBy, c.updateDate, c.sequence, c.category, c.confirmStatus, c.linkFacebook, c.linkYoutube, c.dateStart, c.dateEnd, c.view, c.linkUrl, c.textButton, c.fileUrl }).ToList();
                }
                //END :update view <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                //BEGIN : Statistic
                try
                {
                    if (!string.IsNullOrEmpty(value.code))
                    {
                        //Get Category
                        var colCategory = new Database().MongoClient<Contact>("eventCalendarCategory");
                        var filterCategory = Builders<Contact>.Filter.Eq("code", docs[0].category);
                        Category docCategory = colCategory.Find(filterCategory).Project(c => new Category { code = c.code, title = c.title }).FirstOrDefault();

                        value.reference = value.code;
                        value.title = docs.Count > 0 ? docs[0].title : "";
                        value.category = docCategory.title;

                        value.statisticsCreateAsync("eventCalendar");
                    }

                }
                catch { }

                //endStatistic

                // get User: firstname lastname, imageurl

                var colNotiSend = new Database().MongoClient<NotificationSend>("notificationSend");
                var filterNotiSend = Builders<NotificationSend>.Filter.Eq("profileCode", value.profileCode)
                    & Builders<NotificationSend>.Filter.Eq("page", "eventPage")
                    & Builders<NotificationSend>.Filter.Ne("status", "A");
                var docNotiSend = colNotiSend.Find(filterNotiSend).Project(c => c.reference).ToList();

                var colNoti = new Database().MongoClient<Notification>("notification");
                var filterNoti = Builders<Notification>.Filter.In("code", docNotiSend) & Builders<Notification>.Filter.Eq("status", "A");
                var docNoti = colNoti.Find(filterNoti).Project(c => new { code = c.code, reference = c.reference }).ToList();

                docs.ForEach(c =>
                {
                    try
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var filterRegister = Builders<Register>.Filter.Eq("username", c.updateBy);
                        var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                        c.imageUrlCreateBy = docRegister.imageUrl;
                        c.createBy = docRegister.firstName + " " + docRegister.lastName;

                    }
                    catch
                    {

                    }
                });

                // where by date
                var filterData = new List<object>();

                if (!string.IsNullOrEmpty(value.date))
                {
                    string dateSub = value.date.Substring(0, 10);
                    string[] dateArray = dateSub.Split("-");
                    string date = dateArray[0] + dateArray[1] + dateArray[2];
                    bool dateBetween = false;

                    foreach (var c in docs)
                    {
                        dateBetween = Between(date.toDateFromString(), c.dateStart.toDateFromString(), c.dateEnd.toDateFromString());
                        if (dateBetween)
                        {
                            filterData.Add(c);
                        }
                    }

                    return new Response { status = "S", message = "success", jsonData = filterData.ToJson(), objectData = filterData };
                }

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs, totalData = col.Find(filter).ToList().Count() };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("gallery/read")]
        public ActionResult<Response> GalleryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Gallery>("eventCalendarGallery");

                var filter = Builders<Gallery>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Gallery>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                var docs = col.Find(filter).Skip(value.skip).Limit(value.limit).Project(c => new { c.imageUrl }).ToList();

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /read
        [HttpPost("comment/read")]
        public ActionResult<Response> CommentRead([FromBody] Criteria value)
        {
            try
            {

                var col = new Database().MongoClient<Comment>("eventCalendarComment");

                var filter = Builders<Comment>.Filter.Ne("status", "D") & Builders<Comment>.Filter.Eq(x => x.isActive, true);
                if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Comment>.Filter.Regex("reference", value.code); }
                //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));

                List<Comment> docs = col.Aggregate().Match(filter).SortByDescending(o => o.docDate).ThenByDescending(o => o.docTime).Skip(value.skip).Limit(value.limit).ToList();

                docs.ForEach(c =>
                {
                    if (!string.IsNullOrEmpty(c.profileCode))
                    {
                        //Get Profile
                        var colRegister = new Database().MongoClient<Register>("register");
                        var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", c.profileCode);
                        var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                        c.createBy = docRegister.firstName + " " + docRegister.lastName;
                        c.imageUrlCreateBy = docRegister.imageUrl;
                    }
                });

                //var list = new List<object>();
                //docs.ForEach(doc => { list.Add(BsonSerializer.Deserialize<object>(doc)); });
                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
        }

        // POST /create
        [HttpPost("comment/create")]
        public ActionResult<Response> CommentCreate([FromBody] Comment value)
        {
            value.code = "".toCode();
            var doc = new BsonDocument();
            try
            {

                //Get Profile
                var colRegister = new Database().MongoClient<Register>("register");
                var filterRegister = Builders<Register>.Filter.Ne(x => x.status, "D") & Builders<Register>.Filter.Eq("code", value.profileCode);
                var docRegister = colRegister.Find(filterRegister).Project(c => new { c.code, c.username, c.password, c.category, c.imageUrl, c.firstName, c.lastName }).FirstOrDefault();

                var word = value.description.verifyRude();

                var col = new Database().MongoClient("eventCalendarComment");

                //check duplicate
                var filter = Builders<BsonDocument>.Filter.Eq("code", value.code);
                if (col.Find(filter).Any())
                {
                    return new Response { status = "E", message = $"code: {value.code} is exist", jsonData = value.ToJson(), objectData = value };
                }

                doc = new BsonDocument
                {
                    { "code", value.code },
                    { "description", word },
                    { "original", value.description },
                    { "imageUrlCreateBy", value.imageUrlCreateBy },
                    { "createBy", value.createBy },
                    { "profileCode", value.profileCode },
                    { "createDate", DateTime.Now.toStringFromDate() },
                    { "createTime", DateTime.Now.toTimeStringFromDate() },
                    { "updateBy", value.updateBy },
                    { "updateDate", DateTime.Now.toStringFromDate() },
                    { "updateTime", DateTime.Now.toTimeStringFromDate() },
                    { "docDate", DateTime.Now.Date.AddHours(7) },
                    { "docTime", DateTime.Now.toTimeStringFromDate() },
                    { "isActive", true },
                    { "reference", value.reference }
                };
                col.InsertOne(doc);

                return new Response { status = "S", message = "success", jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message, jsonData = doc.ToJson(), objectData = BsonSerializer.Deserialize<object>(doc) };
            }
        }

        #region category

        // POST /read
        [HttpPost("category/read")]
        public ActionResult<Response> CategoryRead([FromBody] Criteria value)
        {
            try
            {
                var col = new Database().MongoClient<Category>("eventCalendarCategory");

                var filter = Builders<Category>.Filter.Eq(x => x.status, "A");
                if (!string.IsNullOrEmpty(value.keySearch))
                {
                    filter = (filter & Builders<Category>.Filter.Regex("title", value.keySearch)) | (filter & Builders<Category>.Filter.Regex("description", value.keySearch));
                }
                else
                {
                    if (!string.IsNullOrEmpty(value.code)) { filter = filter & Builders<Category>.Filter.Regex("code", value.code); }
                    if (!string.IsNullOrEmpty(value.title)) { filter = filter & Builders<Category>.Filter.Regex("title", value.title); }
                    if (!string.IsNullOrEmpty(value.description)) { filter = filter & Builders<Category>.Filter.Regex("description", value.description); }
                    if (!string.IsNullOrEmpty(value.language)) { filter = filter & Builders<Category>.Filter.Regex("language", value.language); }
                    //if (!string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<eventCalendarCategory>.Filter.Regex("dateStart", value.startDate); }
                    //if (!string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<eventCalendarCategory>.Filter.Regex("dateEnd", value.endDate); }

                    var ds = value.startDate.toDateFromString().toBetweenDate();
                    var de = value.endDate.toDateFromString().toBetweenDate();
                    if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", ds.start) & Builders<Category>.Filter.Lt("docDate", ds.end); }
                    else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Category>.Filter.Gt("docDate", de.start) & Builders<Category>.Filter.Lt("docDate", de.end); }
                    //filter = filter & (Builders<BsonDocument>.Filter.Eq(x => x.B, "4") | Builders<User>.Filter.Eq(x => x.B, "5"));
                }

                var docs = col.Find(filter).SortBy(o => o.sequence).ThenByDescending(o => o.updateDate).ThenByDescending(o => o.updateTime).Skip(value.skip).Limit(value.limit).Project(c => new { c.code, c.title, c.language, c.imageUrl, c.createBy, c.createDate, c.isActive }).ToList();

                return new Response { status = "S", message = "success", jsonData = docs.ToJson(), objectData = docs };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }

        }

        #endregion

        public static bool Between(DateTime input, DateTime date1, DateTime date2)
        {
            return (input >= date1 && input <= date2);
        }
    }
}