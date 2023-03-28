namespace Takeoff.Data
{
    public interface IMembership: ITypicalEntity
    {
        /// <summary>
        /// The user who has access to the thing specified by TargetId.
        /// </summary>
        /// TODO: clear user cache if changed 
        int UserId { get; set; }

        /// <summary>
        /// Indicates the ID of the thing this membership applies to.  If this is not set explicitly, it will default to ParentId.
        /// </summary>
        int? TargetId { get; set; }

        int? ContainerId { get; set;  }
    }
}