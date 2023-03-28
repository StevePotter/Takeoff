using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mediascend.Web;
using Takeoff.Models;

namespace Takeoff.ViewModels
{
    /// <summary>
    /// /videos/{id}.
    /// </summary>
    public class Videos_Details
    {

        public ProjectThingView Production { get; set; }

        public VideoThingDetailView Video { get; set; }

        public bool IsMember { get; set; }

        public bool PreferHtml5Video { get; set; }

        ///// <summary>
        ///// Plain text password for semi-anonymous login
        ///// </summary>
        //public string QuickAccessDecryptedPassword { get; set; }

        public string CustomUrl { get; set; }

        public bool CanAddComment { get; set; }
        public bool CanAddCommentReply { get; set; }
        public bool CanDownload { get; set; }

        public string SemiAnonymousUserName { get; set; }

    }

    public class Video_Login : Account_Login
    {
        public int VideoId { get; set; }

        //public string ProductionTitle { get; set; }
        //public string ProductionLogoUrl { get; set; }
        //public VideoThumbnail[] Thumbnails { get; set; }

    }

    public class Videos_Edit
    {
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }

        public bool? IsDownloadable { get; set; }

        public string CustomUrl { get; set; }

        public string GuestPassword { get; set; }

        public bool? GuestsCanComment { get; set; }

    }


}


