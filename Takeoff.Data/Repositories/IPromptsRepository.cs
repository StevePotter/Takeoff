using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Takeoff.Data
{
    public interface IPromptRepository
    {
        IViewPrompt CreateInstance();
        IViewPrompt Add(IViewPrompt entity, IUser forUser);
        void Delete(IViewPrompt entity);
    }
}
