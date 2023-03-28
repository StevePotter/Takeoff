using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;

namespace Takeoff.Models
{
    public class FileTransferInfo
    {
        public int DownloadCount;
        public string DownloadSize;
        public int UploadCount;
        public string UploadSize;


        public static FileTransferInfo GetTransferInfo(DataModel db, DateTime fromD, DateTime toD)
        {

            var info = new FileTransferInfo();
            info.DownloadCount = (from f in db.FileDownloadLogs
                                  where f.Date >= fromD && f.Date <= toD
                                  select f).Count();

            if (info.DownloadCount > 0)
            {
                info.DownloadSize = new FileSize((from f in db.FileDownloadLogs
                                                  where f.Date >= fromD && f.Date <= toD
                                                  select (long)f.Bytes).Sum()).ToString();
            }
            else
            {
                info.DownloadSize = "0mb";
            }


            info.UploadCount = (from f in db.FileUploadLogs
                                where f.Date >= fromD && f.Date <= toD
                                select f).Count();

            if (info.UploadCount > 0)
            {
                info.UploadSize = new FileSize((from f in db.FileUploadLogs
                                                where f.Date >= fromD && f.Date <= toD
                                                select (long)f.Bytes).Sum()).ToString();
            }
            else
            {
                info.UploadSize = "0mb";
            }
            return info;
        }

    }

}