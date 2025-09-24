using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace cms_api.Controllers
{
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        public DashboardController() { }

        public class Dashboard
        {
            public string name { get; set; }
            public int values { get; set; }
        }

        #region main

        // POST /read
        [HttpPost("read")]
        public ActionResult<Response> Read([FromBody] Criteria value)
        {

            try
            {
                var col = new Database().MongoClient<Veterinary2>("partyMembers");
                var filter = Builders<Veterinary2>.Filter.Ne("status", "D");

                var ds = value.startDate.toDateFromString().toBetweenDate();
                var de = value.endDate.toDateFromString().toBetweenDate();
                if (value.startDate != "Invalid date" && value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate) && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Veterinary2>.Filter.Gt("docDate", ds.start) & Builders<Veterinary2>.Filter.Lt("docDate", de.end); }
                else if (value.startDate != "Invalid date" && !string.IsNullOrEmpty(value.startDate)) { filter = filter & Builders<Veterinary2>.Filter.Gt("docDate", ds.start) & Builders<Veterinary2>.Filter.Lt("docDate", ds.end); }
                else if (value.endDate != "Invalid date" && !string.IsNullOrEmpty(value.endDate)) { filter = filter & Builders<Veterinary2>.Filter.Gt("docDate", de.start) & Builders<Veterinary2>.Filter.Lt("docDate", de.end); }

                var docs = new
                {
                    news = col.CountDocuments(filter & Builders<Veterinary2>.Filter.Eq("category", "")),
                    old = col.CountDocuments(filter & Builders<Veterinary2>.Filter.Ne("category", "")),
                };

                return new Response { status = "S", message = "success", objectData = docs, totalData = docs.news + docs.old };
            }
            catch (Exception ex)
            {
                return new Response { status = "E", message = ex.Message };
            }
            
        }

        #endregion

        [HttpGet("registerAll")]
        public ActionResult<IEnumerable<Dashboard>> registerAll()
        {
            try
            {

                var col = new Database().MongoClient<PartyMembers>("partyMembers");
                var filter = Builders<PartyMembers>.Filter.Ne("status", "D");

                var total_all = (int)col.CountDocuments(filter);

                var data = new List<Dashboard>() {
                    new Dashboard { name = "ทั้งหมด" , values = total_all },
                };

                data.Add(new Dashboard { name = "ข้อมูลอาชีพ", values = total_all });
                //Job
                var docJob = col.Find(filter & Builders<PartyMembers>.Filter.Ne("currentOccupation", BsonNull.Value)).Project(c => c.currentOccupation ?? "").ToList();
                var docGroupJob = docJob.GroupBy(g => g).Select(s => new Dashboard { name = s.Key == "" ? "ไม่ระบุอาชีพ" : s.Key, values = s.Count() }).ToList();
                data.AddRange(docGroupJob);

                data.Add( new Dashboard { name = "ข้อมูลจังหวัด" , values = total_all });

                //province
                var docProvince = col.Find(filter & Builders<PartyMembers>.Filter.Ne("province", BsonNull.Value)).Project(c => c.province ?? "").ToList();
                var docGroupProvince = docProvince.GroupBy(g => g).Select(s => new Dashboard { name = s.Key == "" ? "ไม่ระบุจังหวัด" : s.Key, values = s.Count() }).ToList();
                data.AddRange(docGroupProvince);

                return data;

            }
            catch (Exception ex)
            {
                return new List<Dashboard>();
            }
        }

        // GET /registerJob
        [HttpGet("registerJob")]
        public ActionResult<IEnumerable<Dashboard>> registerJob()
        {
            try
            {
                var col = new Database().MongoClient<PartyMembers>("partyMembers");
                var filter = Builders<PartyMembers>.Filter.Ne("status", "D");

                var doc = col.Find(filter & Builders<PartyMembers>.Filter.Ne("currentOccupation", BsonNull.Value)).Project(c => c.currentOccupation ?? "").ToList();

                var docGroup = doc.GroupBy(g => g).Select(s => new Dashboard { name = s.Key == "" ? "ไม่ระบุอาชีพ" : s.Key, values = s.Count() }).ToList();

                return docGroup;
            }
            catch (Exception ex)
            {
                return new List<Dashboard>();
            }
        }

        // GET /registerProvince
        [HttpGet("registerProvince")]
        public ActionResult<IEnumerable<Dashboard>> registerProvince()
        {
            try
            {
                var col = new Database().MongoClient<PartyMembers>("partyMembers");
                var filter = Builders<PartyMembers>.Filter.Ne("status", "D");

                var doc = col.Find(filter & Builders<PartyMembers>.Filter.Ne("province", BsonNull.Value)).Project(c => c.province ?? "").ToList();

                var docGroup = doc.GroupBy(g => g).Select(s => new Dashboard { name = s.Key == "" ? "ไม่ระบุจังหวัด" : s.Key, values = s.Count() }).ToList();

                return docGroup;
            }
            catch (Exception ex)
            {
                return new List<Dashboard>();
            }
        }

    }
}