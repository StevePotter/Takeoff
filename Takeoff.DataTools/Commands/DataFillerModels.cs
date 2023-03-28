using System;

namespace Takeoff.DataTools.Commands.DataFillerModels
{
    //these are used in our big db joins.  they are returned by queries, then used to fill thing objects.  so these are basically intermediate linq2sql necessary evils.
    //you can't do inheritance because linq2sql gave an exception.  so there's lots of repetition but whatever



    public class UserFromData
    {
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string PasswordResetKey { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationKey { get; set; }
        public string SignupSource { get; set; }
        public System.Nullable<int> TimezoneOffset { get; set; }
        public System.Nullable<bool> CreatedDuringBeta { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? AccountId { get; set; }
    }

    public class SettingFromData
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
    }


    public class ViewPromptFromData
    {
        public string View { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public DateTime? StartsOn { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
    }


    public class ProjectFromData
    {
        public string Title { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? AccountId { get; set; }
    }

    public class VideoFromData
    {
        public string Title { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }

    }

    public class VideoStreamFromData
    {
        public string Profile { get; set; }
        public int? AudioBitRate { get; set; }
        public int? VideoBitRate { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }

    }

    public class MembershipFromData
    {
        public int UserId { get; set; }
        public int? TargetId { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }

    }

    public class FileFromData
    {
        public string FileName { get; set; }
        public int? Bytes { get; set; }
        public string OriginalFileName { get; set; }
        public string Location { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class ImageFromData
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public string FileName { get; set; }
        public string Location { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class VideoThumbnailFromData
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public double Time { get; set; }
        public string FileName { get; set; }
        public string Location { get; set; }

        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CommentFromData
    {
        public string Body { get; set; }
        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Type { get; set; }//will be "Comment" or "CommentReply"
    }

    public class VideoCommentFromData
    {
        public double StartTime { get; set; }
        public string Body { get; set; }
        public int Id { get; set; }
        public int? OwnerUserId { get; set; }
        public int? ParentId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOn { get; set; }
    }


}
