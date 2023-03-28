using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.Models;

namespace Takeoff.Data
{
    public interface IUsersRepository
    {
        /// <summary>
        /// Returns the given user, or null if it doesn't exist.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IUser Get(int id);

        int GetId(string email);

        IUser GetByEmail(string email);

        EntityExistance GetExistanceByEmail(string email);
        
        IUser Instantiate();

        IUser Insert(IUser entity);

        void BeginUpdate(IUser entity);
        
        IUser Update(IUser entity);
        
        IUser Update(IUser entity, string description);

        /// <summary>
        /// Indicates whether the user is a part of the given 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool IsMemberOf(IUser user, ITypicalEntity entity);

        ISetting GetSetting(IUser user, string name);
    }

}
