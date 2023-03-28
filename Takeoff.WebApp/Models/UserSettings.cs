using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Takeoff.Resources;

namespace Takeoff.Models
{
    public static class UserSettings
    {
        public static void Init()
        {
            NotifyWhenNewVideo = SettingDefinitions.AddDefinition("NotifyWhenNewVideo", true, typeof(bool));
            NotifyWhenNewComment = SettingDefinitions.AddDefinition("NotifyWhenNewComment", false, typeof(bool));
            NotifyWhenNewCommentReply = SettingDefinitions.AddDefinition("NotifyWhenNewCommentReply", false, typeof(bool));
            NotifyWhenNewReplyToAuthoredComment = SettingDefinitions.AddDefinition("NotifyWhenNewReplyToAuthoredComment", true, typeof(bool));
            NotifyWhenNewFile = SettingDefinitions.AddDefinition("NotifyWhenNewFile", false, typeof(bool));
            NotifyWhenNewMember = SettingDefinitions.AddDefinition("NotifyWhenNewMember", true, typeof(bool));
            NotifyForNewFeatures = SettingDefinitions.AddDefinition("NotifyForNewFeatures", true, typeof(bool));
            NotifyForMaintenance = SettingDefinitions.AddDefinition("NotifyForMaintenance", true, typeof(bool));
            NotifyForPlanChanges = SettingDefinitions.AddDefinition("NotifyForPlanChanges", true, typeof(bool));
            NotifyForSpecials = SettingDefinitions.AddDefinition("NotifyForSpecials", true, typeof(bool));
            DigestEmailFrequency = SettingDefinitions.AddDefinition("DigestEmailFrequency", "Daily", typeof(string));
            EnableMembershipRequests = SettingDefinitions.AddDefinition("EnableMembershipRequests", true, typeof(bool));
            EnableInvitations = SettingDefinitions.AddDefinition("EnableInvitations", true, typeof(bool));
        }


        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when a video is available in a production they are a member of.
        /// </summary>
        public static SettingDefinition NotifyWhenNewVideo;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when a comment is written in a production they are a member of.
        /// </summary>
        public static SettingDefinition NotifyWhenNewComment;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when a reply is written in a production they are a member of.
        /// </summary>
        public static SettingDefinition NotifyWhenNewCommentReply;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when a reply is written to a comment the user authored.
        /// </summary>
        public static SettingDefinition NotifyWhenNewReplyToAuthoredComment;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when a new file is added to a production they are a member of.
        /// </summary>
        public static SettingDefinition NotifyWhenNewFile;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when a new member is added to a production they are a member of.
        /// </summary>
        public static SettingDefinition NotifyWhenNewMember;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when a we announce a new feature.
        /// </summary>
        public static SettingDefinition NotifyForNewFeatures;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when takeoff will be down for scheduled maintainance
        /// </summary>
        public static SettingDefinition NotifyForMaintenance;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when takeoff plans/pricing changes.
        /// </summary>
        public static SettingDefinition NotifyForPlanChanges;

        /// <summary>
        /// Boolean setting that indicates whether or not the user should get an email when we are running a special.
        /// </summary>
        public static SettingDefinition NotifyForSpecials;

        /// <summary>
        /// String setting that indicates the frequency of digest emails to send to the user.  Valid values are "Never", "Hourly", "BeforeDuringWorkDay", and "Daily".
        /// </summary>
        public static SettingDefinition DigestEmailFrequency;

        /// <summary>
        /// Boolean setting that indicates whether or not people are allowed to request membership to the projects on this user's account.
        /// </summary>
        public static SettingDefinition EnableMembershipRequests;

        /// <summary>
        /// Boolean setting that indicates whether or not people are allowed to invite this user into their productions.
        /// </summary>
        public static SettingDefinition EnableInvitations;

    }
}