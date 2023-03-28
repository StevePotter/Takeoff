using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Takeoff.Data;
using Takeoff.Models;



using Takeoff.Models.Data;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Takeoff.Models
{
    
    [ThingType("User")]
    [Serializable]
    public class UserThing : ThingBase, IUser, IContainerThing
    {
        #region Constructors

        public UserThing()
        {
            EntityMemberships = new Dictionary<int, IMembership>();
            AccountMemberships = new Dictionary<int, IAccountMembership>();
        }

        protected UserThing(SerializationInfo info, StreamingContext context):base(info, context)
        {
            EntityMemberships = new Dictionary<int, IMembership>();
            AccountMemberships = new Dictionary<int, IAccountMembership>();
        }

        #endregion

        #region Properties

        public string Email
        {
            get
            {
                return EmailProperty.GetValue(this);
            }
            set
            {
                EmailProperty.SetValue(this, value);
            }
        }
        private string _Email;
        public static readonly ThingDataProperty<string, UserThing, Data.User> EmailProperty = new ThingDataProperty<string, UserThing, Data.User>()
        {
            Name = "Email",
            SetField = (o, v) => o._Email = v,
            GetField = o => o._Email,
            GetData = o => o.Email,
            SetData = (o, v) => o.Email = v,
        }.Once(prop => {
            prop.BeforeSet += (e, args) =>
            {
                if (EmailProperty.IsSet(args.Owner) && !args.Owner.Email.EqualsCaseSensitive(args.NewValue))
                {
                    args.Owner.RemoveEmailFromCache();//calling this during RemoveFromCache ain't good enough because we don't have the old email address.  so this will make sure the cache is correct
                }
            };
        }).Register();
	

        /// <summary>
        /// The full display name for this user.  This will, in Takeoff 2, default to first name + last name.  But it should still be customizable by users.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return DisplayNameProperty.GetValue(this);
            }
            set
            {
                DisplayNameProperty.SetValue(this, value);
            }
        }
        private string _DisplayName;
        private static readonly ThingDataProperty<string, UserThing, Data.User> DisplayNameProperty = new ThingDataProperty<string, UserThing, Data.User>()
        {
            Name = "DisplayName",
            SetField = (o, v) => o._DisplayName = v,
            GetField = o => o._DisplayName,
            GetData = o => o.Name,
            SetData = (o, v) => o.Name = v,
        }.Register();
	

        public string FirstName
        {
            get
            {
                return FirstNameProperty.GetValue(this);
            }
            set
            {
                FirstNameProperty.SetValue(this, value);
            }
        }
        private string _FirstName;
        private static readonly ThingDataProperty<string, UserThing, Data.User> FirstNameProperty = new ThingDataProperty<string, UserThing, Data.User>()
        {
            Name = "FirstName",
            SetField = (o, v) => o._FirstName = v,
            GetField = o => o._FirstName,
            GetData = o => o.FirstName,
            SetData = (o, v) => o.FirstName = v,
        }.Register();


        public string LastName
        {
            get
            {
                return LastNameProperty.GetValue(this);
            }
            set
            {
                LastNameProperty.SetValue(this, value);
            }
        }
        private string _LastName;
        private static readonly ThingDataProperty<string, UserThing, Data.User> LastNameProperty = new ThingDataProperty<string, UserThing, Data.User>()
        {
            Name = "LastName",
            SetField = (o, v) => o._LastName = v,
            GetField = o => o._LastName,
            GetData = o => o.LastName,
            SetData = (o, v) => o.LastName = v,
        }.Register();


        public bool IsVerified
        {
            get
            {
                return IsVerifiedProperty.GetValue(this);
            }
            set
            {
                IsVerifiedProperty.SetValue(this, value);
            }
        }
        private bool _IsVerified;
        private static readonly ThingDataProperty<bool, UserThing, Data.User> IsVerifiedProperty = new ThingDataProperty<bool, UserThing, Data.User>()
        {
            Name = "IsVerified",
            SetField = (o, v) => o._IsVerified = v,
            GetField = o => o._IsVerified,
            GetData = o => o.IsVerified,
            SetData = (o, v) => o.IsVerified = v,
        }.Register();


        public string VerificationKey
        {
            get
            {
                return VerificationKeyProperty.GetValue(this);
            }
            set
            {
                VerificationKeyProperty.SetValue(this, value);
            }
        }
        private string _VerificationKey;
        private static readonly ThingDataProperty<string, UserThing, Data.User> VerificationKeyProperty = new ThingDataProperty<string, UserThing, Data.User>()
        {
            Name = "VerificationKey",
            SetField = (o, v) => o._VerificationKey = v,
            GetField = o => o._VerificationKey,
            GetData = o => o.VerificationKey,
            SetData = (o, v) => o.VerificationKey = v,
        }.Register();


        public string Password
        {
            get
            {
                return PasswordProperty.GetValue(this);
            }
            set
            {
                PasswordProperty.SetValue(this, value);
            }
        }
        private string _Password;
        private static readonly ThingDataProperty<string, UserThing, Data.User> PasswordProperty = new ThingDataProperty<string, UserThing, Data.User>()
        {
            Name = "Password",
            SetField = (o, v) => o._Password = v,
            GetField = o => o._Password,
            GetData = o => o.Password,
            SetData = (o, v) => o.Password = v,
        }.Register();


        public string PasswordSalt
        {
            get
            {
                return PasswordSaltProperty.GetValue(this);
            }
            set
            {
                PasswordSaltProperty.SetValue(this, value);
            }
        }
        private string _PasswordSalt;
        private static readonly ThingDataProperty<string, UserThing, Data.User> PasswordSaltProperty = new ThingDataProperty<string, UserThing, Data.User>()
        {
            Name = "PasswordSalt",
            SetField = (o, v) => o._PasswordSalt = v,
            GetField = o => o._PasswordSalt,
            GetData = o => o.PasswordSalt,
            SetData = (o, v) => o.PasswordSalt = v,
        }.Register();


        public string PasswordResetKey
        {
            get
            {
                return PasswordResetKeyProperty.GetValue(this);
            }
            set
            {
                PasswordResetKeyProperty.SetValue(this, value);
            }
        }
        private string _PasswordResetKey;
        private static readonly ThingDataProperty<string, UserThing, Data.User> PasswordResetKeyProperty = new ThingDataProperty<string, UserThing, Data.User>()
        {
            Name = "PasswordResetKey",
            SetField = (o, v) => o._PasswordResetKey = v,
            GetField = o => o._PasswordResetKey,
            GetData = o => o.PasswordResetKey,
            SetData = (o, v) => o.PasswordResetKey = v,
        }.Register();


        /// <summary>
        /// The value from javascript's getTimezoneOffset, which is difference between local and UTC in minutes.  This will eventually be set for all users, but can be null for historical users.  Null uses the server's timezone.
        /// </summary>
        public int? TimezoneOffset
        {
            get
            {
                return TimezoneOffsetProperty.GetValue(this);
            }
            set
            {
                TimezoneOffsetProperty.SetValue(this, value);
            }
        }
        private int? _TimezoneOffset;
        private static readonly ThingDataProperty<int?, UserThing, Data.User> TimezoneOffsetProperty = new ThingDataProperty<int?, UserThing, Data.User>()
        {
            Name = "TimezoneOffset",
            SetField = (o, v) => o._TimezoneOffset = v,
            GetField = o => o._TimezoneOffset,
            ShouldGetData = r => r.TimezoneOffset != null,
            GetData = o => o.TimezoneOffset,
            SetData = (o, v) => o.TimezoneOffset = v,
        }.Register();
        

        /// <summary>
        /// A json-encoded string that indicates where they came from.  If they were invited or requested access, it indicates that.  
        /// </summary>
        public dynamic SignupSource
        {
            get
            {
                return SignupSourceProperty.GetValue(this);
            }
            set
            {
                SignupSourceProperty.SetValue(this, value);
            }
        }
        private dynamic _SignupSource;
        public static readonly ThingDataProperty<dynamic, UserThing, Data.User> SignupSourceProperty = new ThingDataProperty<dynamic, UserThing, Data.User>()
        {
            Name = "SignupSource",
            SetField = (o, v) => o._SignupSource = v,
            GetField = o => o._SignupSource,
            ShouldGetData = r => r.SignupSource != null,
            GetData = o =>
                          {
                              //return null;
                              if (!o.SignupSource.HasChars() || o.SignupSource.EqualsCaseInsensitive("null"))
                                  return null;
                              try
                              {
                                  return JObject.Parse(o.SignupSource);//this will hardly ever fail
                              }
                              catch
                              {
                                  var ob = new JObject();
                                  ob["Type"] = o.SignupSource;
                                  return ob;
                              }
                          },
            SetData = (o, v) =>
            {
                o.SignupSource = v == null ? null : Newtonsoft.Json.JsonConvert.SerializeObject(v, Newtonsoft.Json.Formatting.None);
            },
            ToJson = (v) =>
                         {
                             return (v == null ? null : (v is JObject ? (JObject) v : JObject.FromObject(v)));
                         },
            ToJsonComplex2 = (v,w) =>
                                 {
                                     string json = JsonConvert.SerializeObject(v);
                                  w.WriteRawValue(json);   
                                 },
            FromJson = (v) => (dynamic)v,
            CustomSerialize = (v) => Newtonsoft.Json.JsonConvert.SerializeObject(v, Newtonsoft.Json.Formatting.None),
        }.Register();

        /// <summary>
        /// Indicates whether the user was created during the Takeoff beta period.
        /// </summary>
        public DateTime? ConvertedFromBetaOn
        {
            get
            {
                return ConvertedFromBetaOnProperty.GetValue(this);
            }
            set
            {
                ConvertedFromBetaOnProperty.SetValue(this, value);
            }
        }
        private DateTime? _ConvertedFromBetaOn;
        private static readonly ThingDataProperty<DateTime?, UserThing, Data.User> ConvertedFromBetaOnProperty = new ThingDataProperty<DateTime?, UserThing, Data.User>()
        {
            Name = "ConvertedFromBetaOn",
            SetField = (o, v) => o._ConvertedFromBetaOn = v,
            GetField = o => o._ConvertedFromBetaOn,
            GetData = o => o.ConvertedFromBetaOn,
            SetData = (o, v) => o.ConvertedFromBetaOn = v,
        }.Register();


        public List<ISetting> Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new List<ISetting>();
                return _settings;
            }
        }
        [NonSerialized]
        private List<ISetting> _settings;


        public List<IViewPrompt> PendingPrompts
        {
            get
            {
                if (_PendingPrompts == null)
                    _PendingPrompts = new List<IViewPrompt>();
                return _PendingPrompts;
            }
        }
        [NonSerialized]
        private List<IViewPrompt> _PendingPrompts;

        public bool HasPendingPrompts
        {
            get
            {
                return _PendingPrompts != null && _PendingPrompts.Count > 0;
            }
        }

        /// <summary>
        /// Gets a table of account memberships for this user, indexed by account Id.  Note the AccountMembershipThing are isolated and not part of a Thing tree (no Parent or children)
        /// </summary>
        public Dictionary<int, IAccountMembership> AccountMemberships
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a table of things that the user is a member of, indexed by thing Id.  This includes accounts. Note the MembershipThing are isolated and not part of a Thing tree (no Parent or children)
        /// </summary>
        public Dictionary<int, IMembership> EntityMemberships
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the account owned by this user.  If they don't own an account, null is returned.
        /// </summary>
        /// <returns></returns>
        public AccountThing Account
        {
            get
            {
                foreach (var acct in AccountMemberships.Where(a=>a.Value.AccountId.IsPositive()))
                {
                    var account = Things.GetOrNull<AccountThing>(acct.Value.AccountId);
                    if (account != null && account.OwnerUserId == Id)
                    {
                        return account;
                    }
                }
                return null;
            }
        }


        IAccount IUser.Account
        {
            get { return (IAccount) Account; }
        }


        #endregion

        protected override void OnChildAdded(ThingBase child)
        {
            base.OnChildAdded(child);
            child.IfType<ISetting>(s => Settings.Add(s));
            child.IfType<IViewPrompt>(s => PendingPrompts.Add(s));
        }

        protected override void OnChildRemoved(ThingBase child)
        {
            base.OnChildRemoved(child);
            child.IfType<ISetting>(s => Settings.Remove(s));
            child.IfType<IViewPrompt>(s => PendingPrompts.Remove(s));
        }

        ///// <summary>
        ///// Takes the date passed as 
        ///// </summary>
        ///// <param name="date"></param>
        ///// <returns></returns>
        //public DateTime UtcToLocal(DateTime date)
        //{
        //    if (date.Kind == DateTimeKind.Local)//database returned unspecified so treat that as utc
        //        throw new ArgumentException("Date must be UTC");

        //    if (TimezoneOffset.HasValue)
        //    {
        //        return new DateTime(date.Ticks - TimeSpan.FromMinutes(TimezoneOffset.Value).Ticks, DateTimeKind.Local);
        //    }
        //    else
        //    {
        //        return date.ToLocalTime();
        //    }
        //}

        /// <summary>
        /// Indicates whether the user is a member of the account or thing passed.
        /// </summary>
        /// <param name="thingId"></param>
        /// <returns></returns>
        public bool IsMemberOf(int thingId)
        {
            if (EntityMemberships != null && EntityMemberships.ContainsKey(thingId))
                return true;
            if (AccountMemberships != null && AccountMemberships.ContainsKey(thingId))
                return true;
            return false;
        }

        public bool IsMemberOf(ThingBase thing)
        {
            return IsMemberOf(thing.Id);
        }

     
        public bool IsCreatorOf(ThingBase thing)
        {
            return thing.CreatedByUserId == this.Id;
        }



        /// <summary>
        /// Gets the user Id for the given email.  If the email doesn't exist, 0 will be returned.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static int UserIdFromEmail(string email)
        {
            email = Args.HasCharsLower(email);
            var cacheKey = CacheKeyForEmail(email);
            return CacheUtil.GetCachedWithStringSerialization<int>(cacheKey, true, () =>
            {
                using (var db = DataModel.ReadOnly)
                {
                    var user = (from o in db.Users where o.Email == email select new { o.ThingId }).FilterDeletedThings(db, t=>t.ThingId).SingleOrDefault();
                    return user == null ? 0 : user.ThingId;
                }
            });
        }


        public override JObject SerializeJson()
        {
            dynamic json = base.SerializeJson();
            if (EntityMemberships != null && EntityMemberships.Count > 0)
            {
                json.ThingMemberships = new JArray(EntityMemberships.Values.Cast<ThingBase>().Select(v => v.SerializeJson()).ToArray());
            }
            //if (AccountMemberships != null && AccountMemberships.Count > 0)
            //{
            //    json.AccountMemberships = new JArray(AccountMemberships.Values.Cast<ThingBase>().Select(v => v.SerializeJson()).ToArray());
            //}
            return json;
        }

        public override void SerializeJsonProperties2(Newtonsoft.Json.JsonWriter writer)
        {
            base.SerializeJsonProperties2(writer);

            if (EntityMemberships != null && EntityMemberships.Count > 0)
            {
                writer.WritePropertyName("ThingMemberships");
                writer.WriteStartArray();
                foreach (var thing in EntityMemberships.Values)
                {
                    thing.CastTo<ThingBase>().SerializeJson2(writer);
//                    json.ThingMemberships = new JArray(EntityMemberships.Values.Cast<ThingBase>().Select(v => v.SerializeJson()).ToArray());
                    
                }
                writer.WriteEndArray();
            }

        }

        public override void DeserializeJson(JObject json)
        {
            base.DeserializeJson(json);
            EntityMemberships = new Dictionary<int, IMembership>();
            AccountMemberships = new Dictionary<int, IAccountMembership>();
            json["ThingMemberships"].IfNotNull(v =>
            {
                var thingsJson = (JArray)v;
                foreach (JObject thingJson in thingsJson)
                {
                    var membershipThing = (IMembership)CreateThingFromJson(thingJson);
                    EntityMemberships[membershipThing.ContainerId.Value] = membershipThing;

                    var asAccountMember = membershipThing as IAccountMembership;
                    if (asAccountMember != null)
                    {
                        AccountMemberships[membershipThing.AccountId] = asAccountMember;
                    }
                }
            });
            //json["AccountMemberships"].IfNotNull(v =>
            //{
            //    var thingsJson = (JArray)v;
            //    foreach (JObject thingJson in thingsJson)
            //    {
            //        var membershipThing = (IAccountMembership)CreateThingFromJson(thingJson);
            //        AccountMemberships[membershipThing.AccountId] = membershipThing;
            //    }
            //});
        }

        public override void Serialize(SerializationInfo info)
        {
            base.Serialize(info);

            if (EntityMemberships != null && EntityMemberships.Count > 0)
            {
                EntityMemberships.Each(k =>
                    {
                        k.Value.TargetId = k.Key;
                    });
                info.AddValue("ThingMemberships", EntityMemberships.Values.ToArray(), typeof(MembershipThing[]));
            }
        }

        public override void Deserialize(SerializationInfo info)
        {
            AccountMemberships = new Dictionary<int, IAccountMembership>();
            EntityMemberships = new Dictionary<int, IMembership>();
            base.Deserialize(info);
        }

        public override bool DeserializeProperty(string name, object value, SerializationInfo info)
        {
            if (base.DeserializeProperty(name, value, info))
                return true;

            if (name.EqualsCaseSensitive("ThingMemberships"))
            {
                foreach (var membershipThing in (IMembership[])value)
                {
                    EntityMemberships[membershipThing.ContainerId.Value] = membershipThing;

                    var asAccountMember = membershipThing as IAccountMembership;
                    if (asAccountMember != null)
                    {
                        AccountMemberships[membershipThing.AccountId] = asAccountMember;
                    }
                }
                return true;
            }

            return false;
        }



        protected internal override IEnumerable<IThingAuxDataFiller> FillAuxillaryData(DataModel db)
        {
            foreach (var d in base.FillAuxillaryData(db))
                yield return d;

            yield return this.CreateAuxDataFiller<Data.User>(db);

            //memberships are not within the user's Thing tree.  So we have to create them separately.  We have to execute one DB command right now but the things will fill their auxillary data with the combined command.
            AccountMemberships = new Dictionary<int, IAccountMembership>();
            EntityMemberships = new Dictionary<int, IMembership>();

            foreach (var membershipData in (from mt in db.Things join m in db.Memberships on mt.Id equals m.ThingId where mt.DeletedOn == null && m.UserId == this.Id && mt.ParentId != Id select mt).ToArray())
            {
                MembershipThing membershipThing;
                try
                {
                    membershipThing = Things.CreateInstance<MembershipThing>(membershipData.Type); //could throw an error in the case of screwed up data
                }
                catch
                {
                    continue;
                }
                membershipThing.FillPropertiesWithRecord(membershipData);

                var asAccountMember = membershipThing as AccountMembershipThing;
                if (asAccountMember != null)
                {
                    AccountMemberships[membershipThing.AccountId] = asAccountMember;
                }

                EntityMemberships[membershipThing.ContainerId.Value] = membershipThing;
                foreach (var ad in membershipThing.FillAuxillaryData(db))
                    yield return ad;
            }
        }


        public override void AddDataFillers(List<IThingAuxDataFiller> fillers, Dictionary<int, ThingBase> things, DataModel db)
        {
            base.AddDataFillers(fillers, things, db);

            (from data in db.Users
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
            ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);



            (
                from data in db.Settings
                select data
                ).Fill(data =>
                {
                    FillThingData(data.ThingId, data, things);
                    if (things.ContainsKey(data.ThingId))
                    {
                        var setting = things[data.ThingId].CastTo<SettingThing>();
                        setting.SetValueFromData(data.Value);
                    }
                }).AddTo(fillers);


            (from data in db.ViewPrompts
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            //memberships kinda suck.  an option could be to execute a db command immediately, then fill those records accordingly.  
            //first 
            (from thing in db.Things
             join membership in db.Memberships on thing.Id equals membership.ThingId
             where thing.DeletedOn == null && membership.UserId == this.Id
             // && thing.ParentId != Id
             select thing).FillAll(data =>
                                       {
                                           this.EntityMemberships = new Dictionary<int, IMembership>();
                                           this.AccountMemberships = new Dictionary<int, IAccountMembership>();
                                           foreach (var record in data)
                                           {
                                               MembershipThing membershipThing;
                                               try
                                               {
                                                   membershipThing = Things.CreateInstance<MembershipThing>(record.Type);
                                                       //could throw an error in the case of screwed up data
                                               }
                                               catch
                                               {
                                                   continue;
                                               }
                                               membershipThing.FillPropertiesWithRecord(record);

                                               var asAccountMember = membershipThing as AccountMembershipThing;
                                               if (asAccountMember != null)
                                               {
                                                   AccountMemberships[membershipThing.AccountId] = asAccountMember;
                                               }

                                               EntityMemberships[membershipThing.ContainerId.Value] = membershipThing;
                                               things.Add(record.Id, membershipThing);
                                           }

                                       }).AddTo(fillers);


            (from data in db.AccountMemberships
             join membership in db.Memberships on data.ThingId equals membership.ThingId
             join thing in db.Things on data.ThingId equals thing.Id
             where thing.DeletedOn == null && membership.UserId == this.Id && thing.ParentId != Id
             select data
            ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.Memberships
             join thing in db.Things on data.ThingId equals thing.Id
             where thing.DeletedOn == null && data.UserId == this.Id && thing.ParentId != Id
             select data
                ).Fill(data => FillThingData(data.ThingId, data, things)).AddTo(fillers);

            (from data in db.AccountMemberships
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
            ).Fill(data => { throw new Exception("no way"); }).AddTo(fillers);

            (from data in db.Memberships
             join thing in db.Things on data.ThingId equals thing.Id
             where (thing.Id == Id || thing.ContainerId == Id) && thing.DeletedOn == null
             select data
                ).Fill(data => { throw new Exception("no way"); }).AddTo(fillers);

        }

        private static void FillThingData(int id, object data, Dictionary<int, ThingBase> things)
        {
            ThingBase thing;
            if (things.TryGetValue(id, out thing))
            {
                thing.FillPropertiesWithRecord(data);
            }
        }


        protected override void QueueAuxillaryInsert(CommandBatcher batcher)
        {
            base.QueueAuxillaryInsert(batcher);
            batcher.QueueChainedInsert(FillRecordWithProperties(new Data.User()), InsertIdParam);
        }


        protected override void UpdateAuxillaryData(DataModel db)
        {
            base.UpdateAuxillaryData(db);
            var data = (from d in db.Users where d.ThingId == Id select d).Single();
            FillRecordWithProperties(data);
        }

        protected override void QueueDeleteData(CommandBatcher batcher)
        {
            base.QueueDeleteData(batcher);
            batcher.QueueDeleteViaPrimaryKey(new Data.User
            {
                ThingId = Id
            });
        }

        protected override ThingModelView CreateViewInstance(string viewName)
        {
            return new UserView();
        }

        protected override void FillView(string viewName, ThingModelView view, Identity identity)
        {
            base.FillView(viewName, view, identity);
            var typedView = (UserView)view;
            typedView.Email = this.Email;
            typedView.Name = this.DisplayName;
        }

        protected override void AddToCacheOverride()
        {
            base.AddToCacheOverride();

            var email = Email;
            if (email.HasChars())
            {
                CacheUtil.SetInContext(CacheKeyForEmail(email), Id);
                CacheUtil.SetAppCacheValue(CacheKeyForEmail(email), Id.ToInvariant());//redis
            }
        }

        protected override void RemoveFromCacheOverride()
        {
            base.RemoveFromCacheOverride();
            RemoveEmailFromCache();
        }

        private void RemoveEmailFromCache()
        {
            var email = Email;
            if (email.HasChars())
            {
                CacheUtil.RemoveFromContext(CacheKeyForEmail(email));
                CacheUtil.RemoveFromAppCache(CacheKeyForEmail(email));
            }
        }

        /// <summary>
        /// We do lots of lookups based on email, so I cache the Id per email
        /// </summary>
        /// <returns></returns>
        internal static string CacheKeyForEmail(string email)
        {
            return "u." + CacheUtil.SafeCacheKey(email);
        }
        

        #region Settings

        /// <summary>
        /// Gets the value of the setting for the given user.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetSettingValue(string name)
        {
            if ( !name.HasChars())
                throw new ArgumentNullException("name");
            var settingThing = GetSetting(name);
            if (settingThing == null)
            {
                SettingDefinition definition;
                if ( !SettingDefinitions.Definitions.TryGetValue(name,out definition))
                {
                    throw new Exception("Setting definition '{0}' not found.  Possible definitions are: {1}".FormatString(name, string.Join(",", SettingDefinitions.Definitions.Keys)));                    
                }
                return definition.Default;
            }
            else
            {
                return settingThing.Value;
            }
        }

        public object GetSettingValue(SettingDefinition definition)
        {
            var settingThing = GetSetting(definition.Name);
            if (settingThing == null)
            {
                return definition.Default;
            }
            else
            {
                return settingThing.Value;
            }
        }


        public SettingThing GetSetting(string name)
        {
            var settings = _settings;
            if (settings == null || settings.Count == 0)
                return null;

            var settingThing = settings.Find((t) =>
            {
                return t != null && t.Key.EqualsCaseSensitive(name);
            });
            return (SettingThing)settingThing;
        }


        #endregion
    }


    public class UserView : ThingModelView
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }



    //public class NewageView
    //{
    //    [AutomapViewProperty( ThingPropertyName = "Name" )]
    //    public string Name { get; set; }
    //    [AutomapViewProperty]
    //    public string Email { get; set; }

    //    [AutomapViewChildren(ChildThingType=typeof(SettingThing))]
    //    public Setting[] Settings { get; set; }

    //    public string Setmanually { get; set; }
    //    public string Mapmanually { get; set; }
    //    public bool MapAndConvert { get; set; }

    //    public static void AutoMapProperties<TThing>()
    //    {
    //        MapProperty<NewageView, UserThing, string>(UserThing.EmailProperty, (o, v) =>
    //            {
    //                o.MapAndConvert = v.HasChars();
    //            });
    //    }

    //    private static void testapi()
    //    {
    //        var user = UserThing.Current;
    //        new NewageView(user);
    //    }

    //    public NewageView()
    //    {

    //    }
    //    public NewageView(UserThing user)
    //    {
    //        this.FillProperties(user);
    //    }


    //    public void FillProperties(IThingPropertyContainer thing)
    //    {
    //        foreach (var map in MappedProperties)
    //        {
    //            var property = map.ThingProperty;
    //            if (map.SetValueIfNotSet || property.IsSet(thing))
    //            {
    //                var value = property.GetValueAsObject(thing);
    //                map.ViewPropertySetter(this, value);
    //            }
                
    //            property.GetValueAsObject(thing);
    //        }
    //    }

    //    public static void MapProperty<TView, TDeclarer, TProperty>(ThingProperty<TProperty, TDeclarer> property, Action<TView, TProperty> localPropertySetter)
    //    {
            
    //    }

    //    static ThingPropertyMap[] MappedProperties;

    //    class ThingPropertyMap
    //    {
    //        public ThingProperty ThingProperty { get; set; }

    //        public DynamicProperties.GenericSetter ViewPropertySetter { get; set; }

    //        public bool SetValueIfNotSet { get; set; }
    //    }
    //}


    //public class AutomapViewPropertyAttribute : System.Attribute
    //{
    //    public AutomapViewPropertyAttribute()
    //    { 
        
    //    }

    //    public string ThingPropertyName { get; set; }

    //}

        
    //public class AutomapViewChildrenAttribute : System.Attribute
    //{
    //    public AutomapViewChildrenAttribute()
    //    { 
        
    //    }

    //    public Type ChildThingType { get; set; }

    //    public Type ViewType { get; set; }
    //}

}
