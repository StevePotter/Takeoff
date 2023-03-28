using System;
using System.IO;

namespace Takeoff.Data
{

    public interface ITypicalEntity
    {
        /// <summary>
        /// The account ID this thing belongs to.  This implementation changes per thing type and the data is not stored in the database.
        /// </summary>
        int AccountId { get; set; }

        /// <summary>
        /// The unique global ID for this thing.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// The ID of the UserThing that created this.
        /// </summary>
        int CreatedByUserId { get; set; }

        /// <summary>
        /// The ID of the person that currently owns this.  By default, this is the same as the creator.
        /// </summary>
        int OwnerUserId { get; set; }

        /// <summary>
        /// UTC date when this was created.
        /// </summary>
        DateTime CreatedOn { get; set; }


    }


    /// <summary>
    /// Implemented by entities that know about the last time they changed.
    /// </summary>
    public interface ILatestChangeAware
    {
        /// <summary>
        /// The ID in ChangeDetails that is the latest change to happen to this thing or its descendants.
        /// </summary>
        int? LastChangeId { get; set; }

        /// <summary>
        /// The date of the latest change that occured for this thing.
        /// </summary>
        DateTime? LastChangeDate { get; set; }
    }
}