using System.Collections.Generic;
using System.Linq;
using System;
using System.Xml;
using System.Web.Script.Serialization;
using System.Globalization;
using Takeoff.Data;
using Takeoff.Models;
using System.Data.Linq;
using System.Linq.Expressions;
using System.Data.Linq.Mapping;
using System.Data.Common;
using System.Data;
using System.Reflection;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;



namespace Takeoff.Models.Data
{

    [Serializable]//so it can be cached
    partial class Permission
    {

    }

    [Serializable]//so it can be cached
    partial class ActionSource : IChange
    {

    }


    partial class Plan : IPlan
    {
        public bool IsFree
        {
            get
            {
                return PriceInCents <= 0;
            }
            set
            {
                if (value)
                    PriceInCents = 0;
            }
        }

        public FileSize? VideoFileMaxSize
        {
            get
            {
                if (!VideoFileSizeMaxBytes.HasValue)
                    return new FileSize?();
                return new FileSize(VideoFileSizeMaxBytes.Value);
            }
            set {
                VideoFileSizeMaxBytes = value.HasValue ? value.Value.Bytes : new long?(); 
            }
        }

        public FileSize? AssetFileMaxSize
        {
            get
            {
                if (!AssetFileSizeMaxBytes.HasValue)
                    return new FileSize?();
                return new FileSize(AssetFileSizeMaxBytes.Value);
            }
            set
            {
                AssetFileSizeMaxBytes = value.HasValue ? value.Value.Bytes : new long?();
            }
        }

        public FileSize? AssetsTotalMaxSize
        {
            get
            {
                if (!AssetsTotalMaxBytes.HasValue)
                    return new FileSize?();
                return new FileSize(AssetsTotalMaxBytes.Value);
            }
            set
            {
                AssetsTotalMaxBytes = value.HasValue ? value.Value.Bytes : new long?();
            }
        }


        public PlanInterval TrialIntervalType
        {
            get
            {
                return Enum<PlanInterval>.Parse(_TrialIntervalUnit, true);
            }
            set
            {
                _TrialIntervalUnit = value.ToString();
            }
        }

        public PlanInterval BillingIntervalType
        {
            get
            {
                return Enum<PlanInterval>.Parse(_BillingIntervalUnit, true);
            }
            set
            {
                _BillingIntervalUnit = value.ToString();
            }
        }

        public static DateTime AddInterval(DateTime date, int length, PlanInterval unit)
        {
            if (unit == PlanInterval.Days)
                return date.AddDays(length);
            else
                return date.AddMonths(length);
        }

    }

    partial class SemiAnonymousUser: ISemiAnonymousUser
    {
        
    }
}

namespace Takeoff.Models
{
    public partial class DataModel: DataModelBase
    {
        public DataModel() :
            base(CreateConnection())
        {
        }

        public DataModel(System.Data.IDbConnection connection) : base(connection)
		{
		}

        /// <summary>
        /// Creates a profilable data connection for the data model.
        /// </summary>
        /// <returns></returns>
        static IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeoffConnectionString"].ConnectionString);
            return new StackExchange.Profiling.Data.ProfiledDbConnection(connection, StackExchange.Profiling.MiniProfiler.Current);
        }

        /// <summary>
        /// Shortcut to new DataModel without object tracking.  Use for simple queries yielding immediate results (no deferred loading).
        /// </summary>
        public static DataModel ReadOnly
        {
            get
            {
                return new DataModel { ObjectTrackingEnabled = false, DeferredLoadingEnabled = false };
            }
        }

        /// <summary>
        /// A nice code saver for writing one liners.  Avoids indentation with using statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryFactory"></param>
        /// <returns></returns>
        public static T ReadOnlyQuery<T>(Func<DataModel, T> queryFactory)
        {//MvcMiniProfiler.Data.ProfiledDbConnection.Get(//System.Configuration.ConfigurationManager.ConnectionStrings["TakeoffConnectionString"].ConnectionString
            using (var db = DataModel.ReadOnly)
            {
                return queryFactory(db);
            }
        }

        public event EventHandler SubmittedChanges;

        public override void SubmitChanges(ConflictMode failureMode)
        {
            base.SubmitChanges(failureMode);
            if (SubmittedChanges != null)
                SubmittedChanges(this, EventArgs.Empty);
        }

    }

    /// <summary>
    /// Taken from PLINQO project.
    /// </summary>
    public static class DataContextExtensions
    {
        /// <summary>
        /// Executes a SQL statement against a <see cref="DataContext"/> connection. 
        /// </summary>
        /// <param name="context">The <see cref="DataContext"/> to execute the batch select against.</param>
        /// <param name="command">The DbCommand to execute.</param>
        /// <returns>The number of rows affected.</returns>
        /// <remarks>The DbCommand is not disposed by this call, the caller must dispose the DbCommand.</remarks>
        public static int ExecuteCommand(this DataContext context, DbCommand command)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (command == null)
                throw new ArgumentNullException("command");

            LogCommand(context, command);

            command.Connection = context.Connection;
            if (command.Connection == null)
                throw new InvalidOperationException("The DataContext must contain a valid SqlConnection.");

            if (context.Transaction != null)
                command.Transaction = context.Transaction;

            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Batches together multiple <see cref="IQueryable"/> queries into a single <see cref="DbCommand"/> and returns all data in
        /// a single round trip to the database.
        /// </summary>
        /// <param name="context">The <see cref="DataContext"/> to execute the batch select against.</param>
        /// <param name="queries">Represents a collections of SELECT queries to execute.</param>
        /// <returns>Returns an <see cref="IMultipleResults"/> object containing all results.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or queries are null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when context.Connection is invalid.</exception>
        public static IMultipleResults ExecuteQuery(this DataContext context, params object[] queries)
        {
            return ExecuteQuery(context, queries.ToList());
        }

        /// <summary>
        /// Batches together multiple <see cref="IQueryable"/> queries into a single <see cref="DbCommand"/> and returns all data in
        /// a single round trip to the database.
        /// </summary>
        /// <param name="context">The <see cref="DataContext"/> to execute the batch select against.</param>
        /// <param name="queries">Represents a collections of SELECT queries to execute.</param>
        /// <returns>Returns an <see cref="IMultipleResults"/> object containing all results.</returns>
        /// <exception cref="ArgumentNullException">Thrown when context or queries are null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when context.Connection is invalid.</exception>
        public static IMultipleResults ExecuteQuery(this DataContext context, IEnumerable<object> queries)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (queries == null)
                throw new ArgumentNullException("queries");

            var commandList = new List<object>();

            foreach (var query in queries)
            {
                var asIQueryable = query as IQueryable;
                if (asIQueryable != null)
                    commandList.Add(context.GetCommand(asIQueryable, true));
                else
                    commandList.Add((string)query);
            }

            using (DbCommand batchCommand = CombineCommands(context, commandList))
            {
                LogCommand(context, batchCommand);

                batchCommand.Connection = context.Connection;


                if (batchCommand.Connection == null)
                    throw new InvalidOperationException("The DataContext must contain a valid SqlConnection.");

                if (context.Transaction != null)
                    batchCommand.Transaction = context.Transaction;

                DbDataReader dr;

                if (batchCommand.Connection.State == ConnectionState.Closed)
                {
                    batchCommand.Connection.Open();
                    dr = batchCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }
                else
                    dr = batchCommand.ExecuteReader();

                if (dr == null)
                    return null;

                return context.Translate(dr);
            }
        }

        /// <summary>
        /// Provides information about SQL commands generated by LINQ to SQL.
        /// </summary>
        /// <param name="context">The DataContext to get the command from.</param>
        /// <param name="query">The query whose SQL command information is to be retrieved.</param>
        /// <param name="isForTranslate">if set to <c>true</c>, adjust the sql command to support calling DataContext.Translate().</param>
        /// <returns></returns>
        public static DbCommand GetCommand(this DataContext context, IQueryable query, bool isForTranslate)
        {
            // HACK: GetCommand will not work with transactions and the L2SProfiler.
            DbTransaction tran = context.Transaction;
            context.Transaction = null;
            var dbCommand = context.GetCommand(query);
            dbCommand.Transaction = tran;
            context.Transaction = tran;

            if (!isForTranslate)
                return dbCommand;

            MetaType metaType = context.Mapping.GetMetaType(query.ElementType);
            if (metaType != null && metaType.IsEntity)
                dbCommand.CommandText = RemoveColumnAlias(dbCommand.CommandText, metaType);

            return dbCommand;
        }

        public static DbCommand GetCommand(this DataContext dataContext, Expression expression)
        {
            // get provider from DataContext
            object provider = GetProvider(dataContext);
            if (provider == null)
                throw new InvalidOperationException("Failed to get the DataContext provider instance.");

            Type providerType = provider.GetType().GetInterface("IProvider");
            if (providerType == null)
                throw new InvalidOperationException("Failed to cast the DataContext provider to IProvider.");

            MethodInfo commandMethod = providerType.GetMethod("GetCommand", BindingFlags.Instance | BindingFlags.Public);
            if (commandMethod == null)
                throw new InvalidOperationException("Failed to get the GetCommand method from the DataContext provider.");

            // run the GetCommand method from the provider directly
            var commandObject = commandMethod.Invoke(provider, new object[] { expression });
            return commandObject as DbCommand;
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns>An object representing the new transaction.</returns>
        public static DbTransaction BeginTransaction(this DataContext dataContext)
        {
            return BeginTransaction(dataContext, IsolationLevel.Unspecified);
        }

        /// <summary>
        /// Starts a database transaction with the specified isolation level. 
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="isolationLevel">The isolation level for the transaction.</param>
        /// <returns>An object representing the new transaction.</returns>
        public static DbTransaction BeginTransaction(this DataContext dataContext, IsolationLevel isolationLevel)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (dataContext.Connection.State == ConnectionState.Closed)
                dataContext.Connection.Open();

            var transaction = dataContext.Connection.BeginTransaction(isolationLevel);
            dataContext.Transaction = transaction;

            return transaction;
        }

        /// <summary>
        /// Combines multiple SELECT commands into a single <see cref="SqlCommand"/> so that all statements can be executed in a
        /// single round trip to the database and return multiple result sets.
        /// </summary>
        /// <param name="context">The DataContext to get the command from.</param>
        /// <param name="selectCommands">Represents a collection of commands to be batched together.</param>
        /// <returns>
        /// Returns a single <see cref="SqlCommand"/> that executes all SELECT statements at once.
        /// </returns>
        private static DbCommand CombineCommands(DataContext context, IEnumerable<object> selectCommands)
        {
            var batchCommand = context.Connection.CreateCommand();
            DbParameterCollection newParamList = batchCommand.Parameters;
            var sql = new StringBuilder();

            int commandCount = 0;

            foreach (var cmdOrText in selectCommands)
            {
                if (commandCount > 0)
                    sql.AppendLine();

                sql.AppendFormat("-- Query #{0}", commandCount + 1);
                sql.AppendLine();
                sql.AppendLine();

                if (cmdOrText is DbCommand)
                {
                    var cmd = (DbCommand)cmdOrText;
                    DbParameterCollection paramList = cmd.Parameters;
                    int paramCount = paramList.Count;

                    for (int currentParam = paramCount - 1; currentParam >= 0; currentParam--)
                    {
                        DbParameter param = paramList[currentParam];
                        DbParameter newParam = CloneParameter(param);
                        string newParamName = param.ParameterName.Replace("@", string.Format("@q{0}", commandCount));
                        cmd.CommandText = cmd.CommandText.Replace(param.ParameterName, newParamName);
                        newParam.ParameterName = newParamName;
                        newParamList.Add(newParam);
                    }

                    sql.Append(cmd.CommandText.Trim());
                }
                else
                {
                    sql.Append(((string)cmdOrText).Trim());
                }
                sql.AppendLine(";");
                commandCount++;
            }

            batchCommand.CommandText = sql.ToString();

            return batchCommand;
        }

        /// <summary>
        /// Returns a clone (via copying all properties) of an existing <see cref="DbParameter"/>.
        /// </summary>
        /// <param name="src">The <see cref="DbParameter"/> to clone.</param>
        /// <returns>Returns a clone (via copying all properties) of an existing <see cref="DbParameter"/>.</returns>
        private static DbParameter CloneParameter(DbParameter src)
        {
            var source = src as SqlParameter;
            if (source == null)
                return src;

            var destination = new SqlParameter();

            destination.Value = source.Value;
            destination.Direction = source.Direction;
            destination.Size = source.Size;
            destination.Offset = source.Offset;
            destination.SourceColumn = source.SourceColumn;
            destination.SourceVersion = source.SourceVersion;
            destination.SourceColumnNullMapping = source.SourceColumnNullMapping;
            destination.IsNullable = source.IsNullable;

            destination.CompareInfo = source.CompareInfo;
            destination.XmlSchemaCollectionDatabase = source.XmlSchemaCollectionDatabase;
            destination.XmlSchemaCollectionOwningSchema = source.XmlSchemaCollectionOwningSchema;
            destination.XmlSchemaCollectionName = source.XmlSchemaCollectionName;
            destination.UdtTypeName = source.UdtTypeName;
            destination.TypeName = source.TypeName;
            destination.ParameterName = source.ParameterName;
            destination.Precision = source.Precision;
            destination.Scale = source.Scale;

            return destination;
        }

        private static string RemoveColumnAlias(string sql, MetaType metaType)
        {
            // Work around issue with DataContext.Translate not supporting column aliases.

            // find the first from
            int fromIndex = sql.IndexOf("\r\nFROM ");

            string selectText = sql.Substring(0, fromIndex);
            string fromText = sql.Substring(fromIndex);

            foreach (MetaDataMember member in metaType.PersistentDataMembers)
            {
                if (member.IsAssociation || member.Name == member.MappedName)
                    continue;

                // remove column alias because DataContext.Translate doesn't work with them
                string search = string.Format("[{0}] AS [{1}]", member.MappedName, member.Name);
                string replace = string.Format("[{0}]", member.MappedName);

                selectText = selectText.Replace(search, replace);
            }

            return selectText + fromText;
        }

        internal static void LogCommand(DataContext context, DbCommand cmd)
        {
            if (context == null || context.Log == null || cmd == null)
                return;

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            // get provider from DataContext
            object provider = GetProvider(context);
            if (provider == null)
                return;

            Type providerType = provider.GetType();
            MethodInfo logCommandMethod = providerType.GetMethod("LogCommand", flags);

            while (logCommandMethod == null && providerType.BaseType != null)
            {
                providerType = providerType.BaseType;
                logCommandMethod = providerType.GetMethod("LogCommand", flags);
            }

            if (logCommandMethod == null)
                return;

            logCommandMethod.Invoke(provider, new object[] { context.Log, cmd });
        }

        private static object GetProvider(DataContext dataContext)
        {
            Type contextType = dataContext.GetType();
            PropertyInfo providerProperty = contextType.GetProperty("Provider", BindingFlags.Instance | BindingFlags.NonPublic);
            if (providerProperty == null)
                return null;

            return providerProperty.GetValue(dataContext, null);
        }



        /// <summary>
        /// Generates a simple select command like "SELECT ThingId, Title, FilesLocation FROM [Project] WHERE ThingId = 45", which is useful for auxillary data commands.
        /// This helped us create sql commands that didn't include parameters (which LINQ does by default), which let us avoid the "too many parameters" exception created when lots of queries where batched together.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static string SimpleSelectQuery(this DataContext dataContext, Type modelType, int keyEquals, params string[] columnsToSelect)
        {
            var modelMap = DataContextExtensions.GetModelDbMap(modelType);
            return SimpleSelectQuery(dataContext, modelType, modelMap.PrimaryKey.ColumnName.StartWith("[").EndWith("]") + " = " + keyEquals.ToInvariant(), columnsToSelect);
        }

        public static string SimpleSelectQuery(this DataContext dataContext, Type modelType, string whereClause, params string[] columnsToSelect)
        {
            var modelMap = DataContextExtensions.GetModelDbMap(modelType);

            StringBuilder sb = new StringBuilder("SELECT ");

            if (columnsToSelect == null || columnsToSelect.Length == 0)
            {
                columnsToSelect = modelMap.Columns.Select(c => c.SafeColumnName + " as [" + c.PropertyName + "]").ToArray();
            }
            else
            {
                columnsToSelect = columnsToSelect.Select(c => c.StartWith("[").EndWith("]")).ToArray();
            }

            sb.Append(string.Join(",", columnsToSelect) + " FROM " + modelMap.TableName + " WHERE " + whereClause);
            return sb.ToString();
        }


        #region Linq2SQL Entity Mapping Cache

        private static Dictionary<Type, ModelDbMap> columnDatas = new Dictionary<Type, ModelDbMap>();
        private static object lockObj = new object();

        internal class ModelDbMap
        {
            public string TableName { get; set; }
            public SqlColumnData PrimaryKey { get; set; }
            public SqlColumnData[] Columns { get; set; }
            public HashSet<string> ColumnNames { get; set; }
            /// <summary>
            /// Gets column data per property name on the model.
            /// </summary>
            public Dictionary<string,SqlColumnData> ColumnsPerProperty { get; set; }
        }

        internal class SqlColumnData
        {
            /// <summary>
            /// Wonky parameter that should go away.  Sometimes stuff is input and sometimes output.  It depends.
            /// </summary>
            public ParameterDirection ParameterDirection { get; set; }

            public string ColumnName { get; set; }

            /// <summary>
            /// The column encased in brackets.  Use this when creating queries.
            /// </summary>
            public string SafeColumnName { get; set; }

            public string PropertyName { get; set; }

            public bool IsPrimaryKey { get; set; }

            public PropertyInfo PropertyInfo { get; set; }

            public SqlDbType DbType { get; set; }

            public int Size { get; set; }

            public bool IsNullable { get; set; }

            public SqlParameter CreateParameter(string paramName, ParameterDirection direction, object value = null)
            {
                var dbParam = new SqlParameter(paramName, DbType);
                dbParam.SourceColumn = ColumnName;
                if (Size > 0)
                    dbParam.Size = Size;
                dbParam.Direction = direction;
                if (direction == System.Data.ParameterDirection.Input || direction == System.Data.ParameterDirection.InputOutput)
                {
                    dbParam.Value = value;
                    if (dbParam.Value == null)
                        dbParam.Value = DBNull.Value;
                }
                dbParam.IsNullable = IsNullable;
                return dbParam;
            }
        }

        internal static ModelDbMap GetModelDbMap(Type modelType)
        {
            ModelDbMap map;
            if (!columnDatas.TryGetValue(modelType, out map))
            {
                lock (lockObj)
                {
                    if (!columnDatas.TryGetValue(modelType, out map))
                    {
                        map = new ModelDbMap();
                        map.TableName = modelType.GetCustomAttributes(typeof(System.Data.Linq.Mapping.TableAttribute), false).Cast<System.Data.Linq.Mapping.TableAttribute>().First().Name;
                        if (map.TableName.Contains("."))
                            map.TableName = map.TableName.After(".");
                        map.TableName = map.TableName.StartWith("[").EndWith("]");
                        List<SqlColumnData> columns = new List<SqlColumnData>();
                        foreach (var prop in modelType.GetProperties())
                        {
                            var colAtt = prop.GetCustomAttributes(typeof(ColumnAttribute), true).Cast<ColumnAttribute>().FirstOrDefault();
                            if (colAtt != null)
                            {
                                var colMap = new SqlColumnData();
                                colMap.IsPrimaryKey = colAtt.IsPrimaryKey;
                                colMap.ParameterDirection = colAtt.IsDbGenerated ? ParameterDirection.Output : ParameterDirection.Input;
                                columns.Add(colMap);
                                if (colMap.IsPrimaryKey)
                                    map.PrimaryKey = colMap;

                                colMap.ColumnName = string.IsNullOrEmpty(colAtt.Name) ? prop.Name : colAtt.Name;
                                colMap.SafeColumnName = colMap.ColumnName.StartWith("[").EndWith("]");
                                colMap.PropertyInfo = prop;
                                colMap.PropertyName = prop.Name;
                                var colType = colAtt.DbType;
                                if (colType.Contains("("))
                                    colType = colType.Before("(");
                                if (colType.Contains(" "))
                                    colType = colType.Before(" ");
                                colMap.DbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), colType);

                                var size = colAtt.DbType.FilterDigits();
                                if (size.HasChars())
                                {
                                    colMap.Size = size.ToInt();
                                }
                                colMap.IsNullable = !colAtt.DbType.Contains("NOT NULL");
                            }
                        }

                        map.Columns = columns.ToArray();
                        map.ColumnNames = new HashSet<string>(columns.Select(c => c.ColumnName));
                        map.ColumnsPerProperty = columns.ToDictionary(c => c.PropertyName);
                        columnDatas.Add(modelType, map);
                    }
                }
            }
            return map;
        }

        #endregion

    }



}
