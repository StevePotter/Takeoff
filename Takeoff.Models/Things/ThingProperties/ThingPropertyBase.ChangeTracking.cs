using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Linq.Expressions;


namespace Takeoff.Models
{

    partial class ThingProperty
    {
        /// <summary>
        /// Indicates whether this property will track whether or not it has been changed since its container started tracking changes.
        /// </summary>
        public bool TrackChanges
        {
            get
            {
                return _TrackChanges.GetValueOrDefault(TrackIfIsSet);
            }
            set
            {
                if (Sealed)
                    throw new CannotModifySealedObjectException();

                _TrackChanges = value;
            }
        }
        bool? _TrackChanges;


        /// <summary>
        /// Indicates whether this property has been set while the container was tracking changes.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public bool IsChanged(IThingPropertyContainer container)
        {
            //todo: maybe enforce things better with exceptions checking for whether it's tracking changes or 
            return container.PropertiesChangedSinceTracking[BitForIsSet];
        }

    }

}