using System;
using Takeoff.Data;

namespace Takeoff.Data
{
    /// <summary>
    /// Provides access to various data repositories.
    /// </summary>
    /// <remarks>Note that repositories never maintain thread-specific state.  Therefore they can be kept statically and instantiated via IoC.Get. </remarks>
    public static class Repos
    {
        public static IUsersRepository Users
        {
            get { return _users.Value; }
        }
        private static readonly Lazy<IUsersRepository> _users = new Lazy<IUsersRepository>(IoC.Get<IUsersRepository>);  

        public static IPlansRepository Plans
        {
            get { return _plans.Value; }
        }
        private static readonly Lazy<IPlansRepository> _plans = new Lazy<IPlansRepository>(IoC.Get<IPlansRepository>);  

        public static IPromptRepository Prompts
        {
            get { return _prompts.Value; }
        }
        private static readonly Lazy<IPromptRepository> _prompts = new Lazy<IPromptRepository>(IoC.Get<IPromptRepository>);

        public static IAccountsRepository Accounts
        {
            get { return _Accounts.Value; }
        }
        private static readonly Lazy<IAccountsRepository> _Accounts = new Lazy<IAccountsRepository>(IoC.Get<IAccountsRepository>);

        public static IVideosRepository Videos
        {
            get { return _Videos.Value; }
        }
        private static readonly Lazy<IVideosRepository> _Videos = new Lazy<IVideosRepository>(IoC.Get<IVideosRepository>);

        public static IProductionsRepository Productions
        {
            get { return _Productions.Value; }
        }
        private static readonly Lazy<IProductionsRepository> _Productions = new Lazy<IProductionsRepository>(IoC.Get<IProductionsRepository>);

        public static ISemiAnonymousUsersRepository SemiAnonymousUsers
        {
            get { return _SemiAnonymousUsers.Value; }
        }
        private static readonly Lazy<ISemiAnonymousUsersRepository> _SemiAnonymousUsers = new Lazy<ISemiAnonymousUsersRepository>(IoC.Get<ISemiAnonymousUsersRepository>);  

    }
}