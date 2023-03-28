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
    /// /productions/create.
    /// </summary>
    public class Productions_Create
    {
        /// <summary>
        /// The title of the production that will be created.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }

        public string CustomUrl { get; set; }

        public HttpPostedFileBase Logo { get; set; }


        /// <summary>
        /// Plain text password for semi-anonymous login
        /// </summary>
        public string SemiAnonymousDecryptedPassword { get; set; }

        /// <summary>
        /// Whether users can add anonymous comments.
        /// </summary>
        public bool SemiAnonymousUsersCanComment { get; set; }


    }

    /// <summary>
    /// /productions/create.
    /// </summary>
    public class Productions_Edit
    {
        /// <summary>
        /// The title of the production that will be created.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }

        public HttpPostedFileBase Logo { get; set; }

        public string CustomUrl { get; set; }

        public string QuickAccessDecryptedPassword { get; set; }

        public bool? QuickAccessAllowCommenting { get; set; }
    }


    public class Productions_Login : Account_Login
    {
        public int ProductionId { get; set; }

        //public string ProductionTitle { get; set; }
        //public string ProductionLogoUrl { get; set; }
        //public VideoThumbnail[] Thumbnails { get; set; }

    }



    /// <summary>
    /// /productions/details when someone isn't a member of the production
    /// </summary>
    public class Productions_Details_RequestAccess
    {
        public int ProductionId { get; set; }

        /// <summary>
        /// A note to the owner of the production.
        /// </summary>
        public string Note { get; set;  }
    }


    public class Production_Details : StartupMessage
    {
        /// <summary>
        /// The production data to fill with.  Note that eventually you should create a viewmodel for this to rely less on data.
        /// </summary>
        public ProjectThingView Data { get; set; }

        public bool EnableChangeSyncing { get; set; }

        public TimeSpan ActivityInterval { get; set; }
        
        public bool IsMember { get; set; }

        public bool PreferHtml5Video { get; set; }

        public string InitialFocus { get; set; }

        /// <summary>
        /// Plain text password for semi-anonymous login
        /// </summary>
        public string QuickAccessDecryptedPassword { get; set; }

        /// <summary>
        /// Whether users can add anonymous comments.
        /// </summary>
        public bool QuickAccessAllowCommenting { get; set; }

        public string CustomUrl { get; set; }

        public string SemiAnonymousUserName { get; set; }

        public string FilePickerApiKey { get; set; }
    }


    public class Production_Details_Invitation
    {
        public string InvitedByName { get; set; }
        public string InvitedByEmail { get; set; }
        public VideoThumbnail[] Thumbnails { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// The ID of the invitation.
        /// </summary>
        public int MembershipRequestId { get; set; }

        /// <summary>
        /// When the request was created.  This should already be converted to the current user's timezone.
        /// </summary>
        public DateTime RequestCreatedOn { get; set; }

        public string ProductionTitle { get; set; }
    }

    /// <summary>
    /// A thumbnail for a spot in a video.
    /// </summary>
    public class VideoThumbnail
    {
        /// <summary>
        /// The url to the thumbnail
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The position, in seconds, within the video that this thumbnail was generated from.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// The width, in pixels, of the thumbnail.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height, in pixels, of the thumbnail.
        /// </summary>
        public int Height { get; set; }
    }
}


