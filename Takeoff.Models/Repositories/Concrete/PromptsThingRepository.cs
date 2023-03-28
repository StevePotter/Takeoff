using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Data;
using Takeoff.Models;

namespace Takeoff.ThingRepositories
{
    public class PromptsThingRepository: IPromptRepository
    {

        public IViewPrompt CreateInstance()
        {
            return new ViewPromptThing();
        }

        public IViewPrompt Add(IViewPrompt entity, IUser forUser)
        {
            var user = forUser.CastTo<UserThing>();
            user.LogInsertActivity = false;//no need since it's added by admin
            return user.AddChild(entity.CastTo<ViewPromptThing>()).Insert();
        }

        public void Delete(IViewPrompt entity)
        {
             ((ViewPromptThing) entity).Delete();
        }
    }
}
