using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security;
using Takeoff.Data;
using Takeoff.Models;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections;
using System.Data.Linq;
using System.Dynamic;
using Data = Takeoff.Models.Data;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using System.Collections.Specialized;

namespace Takeoff.Models
{
    /*
     * The addition of ThingProperties will provide:
     * - automatic setting of properties from database models
     * - automatic serialization
     * - automatic setting of database model properties when a property is set
     * - auto change tracking and reporting
     * - auto assignment to view objects
     * 
     * 
     */

    public partial class ThingBase : IThingPropertyContainer, ITypicalEntity
    {

        protected ThingPropertyContainerMetaData PropertiesMetaData
        {
            get
            {
                if (_PropertiesMetaData == null)
                {
                    _PropertiesMetaData = ThingPropertyContainerMetaData.GetMetaData(GetType());
                }
                return _PropertiesMetaData;
            }
        }
        ThingPropertyContainerMetaData _PropertiesMetaData;

        public void TrackChanges()
        {
            IsTrackingPropertyChanges = true;
        }

        public bool IsTrackingPropertyChanges
        {
            get; private set;
        }


        BitVector32 IThingPropertyContainer.PropertiesChangedSinceTracking
        {
            get;
            set;
        }

        BitVector32 IThingPropertyContainer.SetPropertyBits
        {
            get;
            set; 
        }


    }

}
