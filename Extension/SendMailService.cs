using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;


using MongoDB.Bson;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Linq;
using System.Threading;
using MongoDB.Bson.Serialization;
using cms_api.Models;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Net.Mime;
using SharpCompress.Compressors.Xz;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Security.Policy;

namespace cms_api.Extension
{
    public class SendMailService
    {
        //ใช้สำหรับ....

        public SendMailService(string description, string subject, string emailUser,string fullName ="", byte[] pdfBytes = null)
        {
            _ = this.SendMail(description, subject, emailUser, fullName, pdfBytes);
        }

        private Task SendMail(string description, string subject, string emailUser, string fullName = "", byte[] pdfBytes = null)
        {
            MemoryStream stream = null;
            Attachment attachment = null;
            try
            {
                var email = new List<string>();
                //email.Add("worawan_p@ksp.or.th");
                email.Add("porntavee29@gmail.com");
                //email.Add("saksit.mukdasanit@gmail.com");

                if (!string.IsNullOrEmpty(emailUser))
                    email.Add(emailUser);

                string emailHost = "ex587mail@gmail.com";
                string password = "exlycskapnqnupwb";
                bool enableSsl = true;
                int port = 587;

                SmtpClient client = new SmtpClient("smtp.gmail.com");
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(emailHost, password);
                client.EnableSsl = enableSsl;
                client.Port = port;

          

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(emailHost, "พรรคไทยก้าวใหม่");

                if (pdfBytes != null)
                {
                    stream = new MemoryStream(pdfBytes); 

                    attachment = new Attachment(stream, "receipt.pdf", MediaTypeNames.Application.Pdf);
                    mailMessage.Attachments.Add(attachment);
                }

                email.ForEach(c =>
                {
                    mailMessage.To.Add(c);
                });

                mailMessage.IsBodyHtml = true;
                //mailMessage.Body = "รหัสผ่านใหม่ของคุณคือ  " + value.newPassword;
                //mailMessage.Body = description;
                //mailMessage.Body = $"สวัสดีคุณ {FullName}\n\nขอบคุณที่สมัครสมาชิกพรรคไทยก้าวใหม่\nกรุณาคลิกลิงก์ด้านล่างเพื่อยืนยันตัวตนของคุณ:\n{description}\n\n*ลิงก์นี้จะหมดอายุใน 24 ชั่วโมง\nหากคุณไม่ได้ทำรายการนี้ สามารถละเว้นอีเมลฉบับนี้ได้\n\nขอแสดงความนับถือ\nทีมงานพรรคไทยก้าวใหม่";
        //        < p > กรุณาคลิกลิงก์ด้านล่างเพื่อยืนยันตัวตนของคุณ </ p >
        //< a href = '" + description + @"'
        //   style = '
        //       display: inline - block;
        //    padding: 10px 20px;
        //        font - size: 16px;
        //    color: white;
        //        background - color: #28a745;
        //       text - decoration: none;
        //        border - radius: 5px;
        //        '>ยืนยันการสมัคร</a>
        //     < p > ลิงก์นี้จะหมดอายุใน 24 ชั่วโมง </ p >
                     mailMessage.Body = $@"
<html>
    <body>
        <p>สวัสดีคุณ {fullName}</p>
<p>ขอบคุณที่สมัครสมาชิกพรรคไทยก้าวใหม่</p>

<p>หากคุณไม่ได้ทำรายการนี้ สามารถละเว้นอีเมลฉบับนี้ได้</p>
<p>ขอแสดงความนับถือ</p>
<p>ทีมงานพรรคไทยก้าวใหม่</p>
   <div style='margin-top: 40px;'>
        <img src='https://gateway.we-builds.com/tkm/assets/img/logo-full-c1.png' alt='Logo' style='max-width: 200px; height: auto;' />
    </div>
    </body>
    </html>
";

                //mailMessage.Subject = "ยืนยันการเปลี่ยนรหัสผ่าน";
                mailMessage.Subject = subject;
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                //return new Response { status = "E", message = ex.Message };
            }
            finally
            {
                // ✅ ปิดไฟล์หลังจากส่งเสร็จ
                attachment?.Dispose();
                stream?.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}
