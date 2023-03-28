using System;
using System.Collections.Generic;

using System.Text;
using System.Reflection;

using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Specialized;


namespace Takeoff.Models
{


    /// <summary>
    /// The interface required by objects that contain ThingProperties
    /// </summary>
    public interface IThingPropertyContainer
    {
        /// <summary>
        /// Gets a value indicating if change tracking is occuring.  If so, properties will be marked as changed when they are set.
        /// </summary>
        bool IsTrackingPropertyChanges { get; }

        /// <summary>
        /// Stores which ThingProperty values have been set while IsTrackingPropertyChanges was true.
        /// </summary>
        BitVector32 PropertiesChangedSinceTracking { get; set; }

        /// <summary>
        /// Stores which ThingProperty values have been set.
        /// </summary>
        BitVector32 SetPropertyBits { get; set; }

    }

}
