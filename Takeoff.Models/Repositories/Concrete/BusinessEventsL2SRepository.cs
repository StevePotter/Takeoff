using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using Takeoff.Models;
using Takeoff.Models.Data;

namespace Takeoff.ThingRepositories
{
    public class BusinessEventsL2SRepository : IBusinessEventsRepository
    {

        public void Insert(BusinessEventInsertParams data)
        {
            using ( var db = new DataModel())
            {
                var businessEventRow = new BusinessEvent
                                        {
                                            Id = data.Id.GetValueOrDefault(Guid.NewGuid()),
                                            Type = data.Type,
                                            AccountId = data.AccountId,
                                            OccuredOn = data.OccuredOn,
                                            RequestId = data.RequestId,
                                            TargetId = data.TargetId,
                                            UserId = data.UserId,
                                        };
                db.BusinessEvents.InsertOnSubmit(businessEventRow);

                if ( data.Parameters.HasItems())
                {
                    foreach (var parameterKeyValue in data.Parameters)
                    {
                        var businessEventParameterRow = new BusinessEventParameter
                                                            {
                                                                BusinessEventId = businessEventRow.Id,
                                                                Id = Guid.NewGuid(),
                                                                Key = parameterKeyValue.Key,
                                                            };
                        if ( parameterKeyValue.Value != null)
                        {
                            switch (Type.GetTypeCode(parameterKeyValue.Value.GetType()))
                            {
                                case TypeCode.String:
                                    businessEventParameterRow.ValueString = (string)parameterKeyValue.Value;
                                    break;
                                case TypeCode.DateTime:
                                    businessEventParameterRow.ValueDate = (DateTime)parameterKeyValue.Value;
                                    break;
                                case TypeCode.Boolean:
                                    businessEventParameterRow.ValueBool = (bool)parameterKeyValue.Value;
                                    break;
                                case TypeCode.Int16:
                                case TypeCode.Int32:
                                    businessEventParameterRow.ValueInt = Convert.ToInt32(parameterKeyValue.Value);
                                    break;
                                case TypeCode.Double:
                                    businessEventParameterRow.ValueFloat = (double)parameterKeyValue.Value;
                                    break;
                                default:
                                    if (parameterKeyValue.Value is Guid)
                                    {
                                        businessEventParameterRow.ValueGuid = (Guid) parameterKeyValue.Value;
                                        break;
                                    }
                                    throw new ArgumentException("Business event parameter type {0} is not supported.".FormatString(parameterKeyValue.Value.GetType().FullName));
                            }
                        }

                        db.BusinessEventParameters.InsertOnSubmit(businessEventParameterRow);
                    }
                }

                
                db.SubmitChanges();
            }
            
        }
    }
}
