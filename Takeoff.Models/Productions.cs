using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Security;
using System.Web.Script.Serialization;
using System.Collections;
using System.Globalization;
using Takeoff.Data;
using Takeoff.Models;
using System.Diagnostics;
using System.Dynamic;
using System.Configuration;

namespace Takeoff.Models
{
    /// <summary>
    /// Helper methods for Productions.  This does not check permissions...that's done by controllers.
    /// </summary>
    public static class Productions
    {
        #region Productions

        #region Sample Production

        private static int SampleClientUserId
        {
            get
            {
                if (!_SampleClientUserId.HasValue)
                {
                    _SampleClientUserId = ConfigUtil.GetRequiredAppSetting<int>("SampleClientId");
                }
                return _SampleClientUserId.Value;
            }
        }
        private static int? _SampleClientUserId;

        #endregion


        /// <summary>
        /// Gets an html A tag with a link to the production.  
        /// </summary>
        /// <returns></returns>
        public static string HtmlLink(this IProduction production)
        {
            return "<a href='" + Url(production) + "'>" + production.Title.HtmlEncode() + "</a>";
        }

        /// <summary>
        /// Gets a url, relative to app root, for the project.
        /// </summary>
        /// <returns></returns>
        public static string Url(this IProduction production)
        {
            return "/productions/" + production.Id.ToInvariant();
        }


        /// <summary>
        /// Gets all the projects the user has access to.  
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int[] GetProjectIdsWithAccess(UserThing user)
        {
            List<int> accountsForAny = new List<int>();
            List<int> accountsForMembers = new List<int>();
            foreach (var membership in user.AccountMemberships)
            {
                var account = Things.GetOrNull<AccountThing>(membership.Key).EnsureExists(membership.Key);
                var acctPermissions = Permissions.GetPermissionSet(membership.Value.Role);
                if (acctPermissions.HasPermission(account, typeof(ProjectThing), Permissions.List, user))
                {
                    accountsForAny.Add(membership.Key);
                }
                else if (acctPermissions.GetPermission(null, Things.ThingType(typeof(ProjectThing)), Permissions.Details) != null)
                {
                    accountsForMembers.Add(membership.Key);
                }
            }

            using (var db = DataModel.ReadOnly)
            {
                return (from t in db.Things
                        where t.DeletedOn == null && t.Type == Things.ThingType(typeof(ProjectThing)) && (
                           accountsForAny.ToArray().Contains(t.AccountId.Value) ||
                           (accountsForMembers.ToArray().Contains(t.AccountId.Value) && user.EntityMemberships.Keys.ToArray().Contains(t.Id))
                        )
                        select t.Id).ToArray();
            }
        }


        public static List<ProjectThing> GetProjectsWithAccess(UserThing user)
        {
            var productionIds = GetProjectIdsWithAccess(user);
            var productions = new List<ProjectThing>();
            foreach( var id in productionIds)
            {
                var production = Things.GetOrNull<ProjectThing>(id);
                if ( production != null)
                    productions.Add(production);
            }
            return productions;
        }

        /// <summary>
        /// Gets the ID of the sample production created for the given account.  If 0, no sample was found.  Useful for avoiding duplicates during signup.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static int GetExistingSampleProductionId(IAccount account)
        {
            using (var db = DataModel.ReadOnly)
            {
                return (from t in db.Things
                            join p in db.Projects on t.Id equals p.ThingId
                            where t.AccountId == account.Id && (bool)p.IsSample
                            select t.Id).FirstOrDefault();
            }
        }

        public static bool HasExistingSampleProductionId(IAccount account)
        {
            return GetExistingSampleProductionId(account).IsPositive();
        }
        /// <summary>
        /// Creates the sample production used when people first sign up.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accountId"></param>
        /// <remarks>It would be nice to refactor this into AddProduction and those methods.
        /// </remarks>
        public static ProjectThing CreateSampleProduction(IUser user, IAccount account, string clientName, string V1Comments, string V1FileName, string V1MobileFileName, string V2Comments, string V2FileName, string V2MobileFileName, string VideoLocation)
        {
            var date = DateTime.Now.AddDays(-7).ToUniversalTime();
            var production = new ProjectThing
            {
                CreatedByUserId = user.Id,
                CreatedOn = date,
                IsContainer = true,
                AccountId = account.Id,
                ContainerId = account.Id,
                IsComplimentary = true,
                IsSample = true,
                Title = "Sneakers to the Ceiling (Sample)"
            };
            production.LogInsertActivity = false;//avoids 
            var membership = new MembershipThing
            {
                CreatedByUserId = user.Id,
                UserId = user.Id,
            };
            production.AddChild(membership);

            var videoLocation = ConfigUtil.GetRequiredAppSetting("ProductionBucket").EndWith("/") + VideoLocation;

            date = date.AddDays(1);
            var video = new VideoThing
            {
                CreatedByUserId = user.Id,
                CreatedOn = date,
                Title = "V1 - Greenscreen",
                IsComplimentary = true,
                IsSample = true,
            };
            production.AddChild(video);

            video.AddChild(new VideoStreamThing
            {
                IsComplimentary = true,
                IsSample = true,
                CreatedByUserId = user.Id,
                CreatedOn = date,
                Location = videoLocation,
                FileName = V1FileName,
                DeletePhysicalFile = false,
                Profile = "Web",
            });
            video.AddChild(new VideoStreamThing
            {
                IsComplimentary = true,
                IsSample = true,
                CreatedByUserId = user.Id,
                CreatedOn = date,
                Location = videoLocation,
                FileName = V1MobileFileName,
                DeletePhysicalFile = false,
                Profile = "Mobile",
            });

            for (var i = 0; i < 5; i++)
            {
                video.AddChild(new VideoThumbnailThing
                {
                    CreatedByUserId = user.Id,
                    CreatedOn = date,
                    Location = videoLocation,
                    FileName = V1FileName + "-" + i.ToInvariant() + ".jpg",
                    Time = i,
                    Width = 180,
                    Height = 101,
                    DeletePhysicalFile = false,
                });
            }

            CreateSampleVideoComments(video, user, V1Comments, clientName);

            date = date.AddDays(1);
            video = new VideoThing
            {
                CreatedByUserId = user.Id,
                CreatedOn = date,
                Title = "Version 2",
                IsComplimentary = true,
                IsSample = true,
            };
            production.AddChild(video);

            video.AddChild(new VideoStreamThing
            {
                IsComplimentary = true,
                IsSample = true,
                CreatedByUserId = user.Id,
                CreatedOn = date,
                Location = videoLocation,
                FileName = V2FileName,
                DeletePhysicalFile = false,
                Profile = "Web",
            });
            video.AddChild(new VideoStreamThing
            {
                IsComplimentary = true,
                IsSample = true,
                CreatedByUserId = user.Id,
                CreatedOn = date,
                Location = videoLocation,
                FileName = V2MobileFileName,                
                DeletePhysicalFile = false,
                Profile = "Mobile",
            });

            for (var i = 0; i < 5; i++)
            {
                video.AddChild(new VideoThumbnailThing
                {
                    CreatedByUserId = user.Id,
                    CreatedOn = date,
                    Location = videoLocation,
                    FileName = V2FileName + "-" + i.ToInvariant() + ".jpg",
                    Time = i,
                    Width = 180,
                    Height = 101,
                    IsComplimentary = true,
                    DeletePhysicalFile = false,
                });
            }

            CreateSampleVideoComments(video, user, V2Comments, clientName);

            production.Insert();
            Productions.SetFilesLocationForAfterInsert(production);

            //create a copy of the membership underneath the user
            //user.AddChild(membership.CreateLinkedThing<MembershipThing>()).Insert();

            return production;
        }

        private static void CreateSampleVideoComments(VideoThing video, IUser newUser, string commentsJson, string clientName)
        {
            var currTime = video.CreatedOn;
            foreach (Dictionary<string, object> comment in new JavaScriptSerializer().DeserializeObject(commentsJson) as IEnumerable)
            {
                currTime = currTime.AddMinutes(30);
                var creatorName = ((Dictionary<string, object>)comment["Creator"])["Name"].ToString().Trim();

                var isClient = creatorName.Equals(clientName.Trim(), StringComparison.OrdinalIgnoreCase);

                var body = (string)comment["Body"];
                var commentThing = AddVideoComment(video, isClient ? SampleClientUserId : newUser.Id, body, Convert.ToDouble(comment["StartTime"]), currTime);

                foreach (Dictionary<string, object> reply in comment["Replies"] as IList)
                {
                    currTime = currTime.AddMinutes(30);
                    creatorName = ((Dictionary<string, object>)reply["Creator"])["Name"].ToString().Trim();
                    isClient = creatorName.Equals(clientName.Trim(), StringComparison.OrdinalIgnoreCase);

                    var replybody = (string)reply["Body"];
                    AddCommentReply(commentThing, isClient ? SampleClientUserId : newUser.Id, replybody, currTime);
                }

            }
        }

        //this is a patch and should eventually be eliminated
        public static void SetFilesLocationForAfterInsert(ProjectThing production)
        {
            //this is a strange exception
            production.FilesLocation = ConfigUtil.GetRequiredAppSetting("ProductionBucket").EndWith("/") + production.Id.ToInvariant();
            using (var db = new DataModel())
            {
                var data = (from d in db.Projects where d.ThingId == production.Id select d).Single();
                data.FilesLocation = production.FilesLocation;
                db.SubmitChanges();
            }
            production.RemoveFromCache();
        }
        #endregion

        #region Comments


        /// <summary>
        /// Adds a comment for the project version specified.  Does NOT call insert, so either call it yourself or call it on a thing up the tree.
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="comment"></param>
        /// <param name="startTime">When >= 0, specifies the time.</param>
        /// <returns></returns>
        public static CommentThing AddVideoComment(ThingBase videoEntity, int userId, string body, double? startTime, DateTime? createdOn = null)
        {
            Args.NotNull(body, "body");
            body = body.Trim();
            Args.HasChars(body, "body");
            CommentThing comment;
            if (startTime.HasValue)
            {
                Args.Compare(startTime.Value, 0, ArgComparison.GreaterOrEqualTo, "startTime");
                startTime = Math.Round(startTime.Value, 2);
                comment = new VideoCommentThing();
                ((VideoCommentThing)comment).StartTime = startTime.Value;
            }
            else
            {
                comment = new CommentThing();
            }
            comment.CreatedByUserId = userId;
            if ( createdOn.HasValue )
                comment.CreatedOn = createdOn.Value;
            comment.Body = body;
            videoEntity.AddChild(comment);
            return comment;
        }


        /// <summary>
        /// Adds a reply to the comment specified.  Does NOT call insert, so either call it yourself or call it on a thing up the tree.
        /// </summary>
        public static CommentReplyThing AddCommentReply(ThingBase comment, int userId, string body, DateTime? createdOn = null)
        {
            Args.NotNull(comment, "comment");
            Args.HasChars(body, "body");

            var reply = new CommentReplyThing
            {
                Body = body,
                CreatedByUserId = userId,
            };
            if (createdOn.HasValue)
                reply.CreatedOn = createdOn.Value;
            comment.AddChild(reply);
            return reply;

        }


        #endregion

        #region Members


        /// <summary>
        /// Adds a new member, either an existing Takeoff user (who should already be a member of this account) or a new invitee, to the given production.
        /// </summary>
        /// <param name="productionId"></param>
        /// <param name="userIdOrEmail"></param>
        /// <param name="role">Only applies if user is new.</param>
        /// <returns>
        /// An object with the following properties:
        /// 
        /// Result.  Type: String
        /// Values:
        /// "AlreadyHasAccess" - bad input, happens if they enter the name of someone who already has access to the production.  This indicates bad input.
        /// "NewUser" - the user didn't exist in the system.  An invitation was sent to the email address and they have to confirm before entering into the system.
        /// "ExistingUser" - the user already existed in the system, so they are automatically added to the project.  In the future we should require some form of confirmation.
        /// 
        /// 
        /// Membership.  Type: MemberThing
        /// The actual record of the user's membership.  This will exist underneath the ProjectThing.  Will be null if the user was already a membery.
        /// 
        /// Member.  Type: UserThing
        /// The user that was added.
        /// 
        /// </returns>
        public static object AddMember(ThingBase production, int addedBy, object userIdOrEmail, string role)
        {
            Args.NotNull(userIdOrEmail, "user");

            IUser member;
            ThingBase membership;
            string result;

            var account = Things.GetOrNull<AccountThing>(production.AccountId).EnsureExists(production.AccountId);
            if (userIdOrEmail is int)
            {
                member = Repos.Users.Get((int)userIdOrEmail);
            }
            else
            {
                Debug.Assert(userIdOrEmail is string);
                member = Repos.Users.GetByEmail((string)userIdOrEmail);
            }

            if (member == null)
            {
                var email = userIdOrEmail as string;
                if ( email == null )
                    throw new ArgumentException("New user requires a string for userIfOrThing user argument.");

                dynamic signupSource = new ExpandoObject();
                signupSource.Type = "Invitation";
                signupSource.ProjectId = production.Id;

                member = Users.AddInvitedUser(addedBy, email, null, signupSource, true);
                result = "NewUser";
                Accounts.AddUserAccess(account, member, role);
                membership = production.AddMember(member, addedBy);
            }
            else
            {
                if (member.IsMemberOf(production))
                {
                    result = "AlreadyHasAccess";
                    membership = null;
                }
                else
                {
                    if ( !member.IsMemberOf(account) )//they weren't on the account, so add em!
                        Accounts.AddUserAccess(account, member, role);
                    result = "ExistingUser";
                    membership = production.AddMember(member, addedBy);
                }
            }

            return new
            {
                Result = result,
                Member = member,
                Membership = membership
            };
        }

        #endregion
    }

}


