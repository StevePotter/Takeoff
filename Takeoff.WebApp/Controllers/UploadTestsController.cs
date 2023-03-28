using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

using Mediascend.Web;

namespace Takeoff.Controllers
{
    public class UploadTestsController : BasicController
    {
        //
        // GET: /UploadTests/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AllocateS3(FileToUpload fileToUpload)
        {
            Args.NotNull(fileToUpload, "fileToUpload");
            var location = ConfigurationManager.AppSettings["ProductionBucket"].EndWith(@"/test");
            fileToUpload.PrepareForUpload(new S3FileLocation { Location = location }, FileAccess.PublicRead, true, fileToUpload.bytes);
            return Json(fileToUpload);
        }

        //[HttpPost]
        //public ActionResult UploadLocal(HttpPostedFileBase file)
        //{
        //    return new EmptyResult();
        //}

        //[HttpPost]
        //public ActionResult UploadTest(FormCollection forms)
        //{
        //    int chunk = Request.QueryString["chunk"] != null ? int.Parse(Request.QueryString["chunk"]) : 0;
        //    string fileName = Request.QueryString["name"] != null ? Request.QueryString["name"] : "";

        //    ////open a file, if our chunk is 1 or more, we should be appending to an existing file, otherwise create a new file
        //    //var fs = new FileStream(@"d:\Temp\" + fileName, chunk == 0 ? FileMode.OpenOrCreate : FileMode.Append);

        //    ////write our input stream to a buffer
        //    //Byte[] buffer = new Byte[Request.InputStream.Length];
        //    //Request.InputStream.Read(buffer, 0, buffer.Length);

        //    ////write the buffer to a file.
        //    //fs.Write(buffer, 0, buffer.Length);
        //    //fs.Close();

        //    return this.Empty();
        //}

    }
}
