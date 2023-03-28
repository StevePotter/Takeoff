using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security;


using Takeoff.Models;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections;
using System.Data.Linq;
using System.Dynamic;
using Data = Takeoff.Models.Data;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using System.Data.Linq.Mapping;
using System.Reflection;
using System.Data;

namespace Takeoff.Models
{

    
        /// <summary>
        /// Makes it possible to batch insert statements, which LINQtoSQL doesn't natively do.
        /// </summary>
    ///             //https://levenblog.svn.codeplex.com/svn/LevenBlog/LevenBlog.Data.SqlServer/SqlHelper.cs

        public class CommandBatcher: IDisposable
        {
            public CommandBatcher()
            {
                connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeoffConnectionString"].ConnectionString);
                command = connection.CreateCommand();
            }

            /// <summary>
            /// Occurs right before the command is executed.  This is a good chance to add more text or something.
            /// </summary>
            public event EventHandler Executing;
            public event EventHandler Executed;

            List<Action> callbacks = new List<Action>();
            StringBuilder text = new StringBuilder();
            SqlConnection connection;
            SqlCommand command;
            int commandIndex = 0;

            

            private void AddCommandText(string commandText)
            {
                text.AppendLine(commandText);
                commandIndex++;
            }

            public void Execute()
            {
                if (Executing != null)
                    Executing(this, EventArgs.Empty);

                command.CommandText = text.ToString();
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                foreach (var callback in callbacks)
                    callback();

                if (Executed != null)
                    Executed(this, EventArgs.Empty);

            }

            #region Insert

            /// <summary>
            /// Queues an insert command for the LINQ2SQL entity object passed in.  Returns the parameter that will contain the identity (primary key) value once the command is executed.
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public SqlParameter QueueInsertAutoId(object model, SubstituteParameter[] substituteParams, Action inserted = null)
            {
                return QueueInsert(model, substituteParams, true, inserted);
            }

            /// <summary>
            /// Queues an insert command for the LINQ2SQL entity object passed in.  The primary key for the model will be set to a parameter that was placed earlier in the command and whose value is not yet available.
            /// </summary>
            /// <param name="model"></param>
            public void QueueChainedInsert(object model, SqlParameter primaryKeyParam)
            {
                QueueInsert(model, new SubstituteParameter[] { new SubstituteParameter { ColumnName = DataContextExtensions.GetModelDbMap(model.GetType()).PrimaryKey.ColumnName, ValueParameter = primaryKeyParam } }, false);
            }

            public SqlParameter QueueInsert(object model, SubstituteParameter[] substituteParams, bool setIdentityParam, Action inserted = null)
            {
                if (inserted != null)
                    callbacks.Add(inserted);
                var modelType = model.GetType();
                var modelMap = DataContextExtensions.GetModelDbMap(modelType);

                var primaryKey = modelMap.PrimaryKey == null ? null : modelMap.PrimaryKey.ColumnName;

                int paramNum = 0;

                Dictionary<string, string> colVals = new Dictionary<string, string>();
                if (substituteParams != null)
                {
                    foreach (var substituteParam in substituteParams)
                    {
                        colVals[substituteParam.ColumnName] = substituteParam.ValueParameter.ParameterName;
                    }
                }

                SqlParameter identityParam = null;
                //only add primary key and input parameters that haven't already had a substitute value specified.
                var parameters = modelMap.Columns.Where(c=> !colVals.ContainsKey(c.ColumnName) && (c.IsPrimaryKey || c.ParameterDirection == ParameterDirection.Input)).Select(col =>
                {
                    var paramName = "@p" + commandIndex.ToInvariant() + "_" + paramNum.ToInvariant();
                    paramNum++;
                    var dbParam = new SqlParameter(paramName, col.DbType);
                    dbParam.SourceColumn = col.ColumnName;
                    if (col.Size > 0)
                        dbParam.Size = col.Size;
                    dbParam.Direction = col.ParameterDirection;
                    if (dbParam.Direction == ParameterDirection.Input)
                    {
                        dbParam.Value = col.PropertyInfo.GetValue(model, null);
                        if (dbParam.Value == null)
                            dbParam.Value = DBNull.Value;
                        colVals[col.ColumnName] = paramName;
                    }
                    if (col.IsPrimaryKey)
                        identityParam = dbParam;
                    dbParam.IsNullable = col.IsNullable;
                    command.Parameters.Add(dbParam);
                    return dbParam;
                }).ToArray();

                string commandText = "INSERT INTO " + modelMap.TableName + " (" + string.Join(",", colVals.Keys.Select(p => p.Surround("[","]")).ToArray()) + ") VALUES (" + string.Join(",", colVals.Values.ToArray()) + ")";
                if (setIdentityParam)
                {
                    if (identityParam == null)
                        throw new InvalidOperationException("No primary key column found.");
                    commandText += " SET " + identityParam.ParameterName + " = SCOPE_IDENTITY()";
                }
                AddCommandText(commandText);
                return identityParam;
            }

            
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="TModel"></typeparam>
            /// <param name="primaryKey"></param>
            /// <param name="propertiesToUpdate">Shoudl be an anonymous type.  This will get its properties converted into a dictionary.</param>
            public void QueueUpdate(int primaryKeyValue, Type modelType, object propertiesToUpdate)
            {
                Dictionary<string,object> props = propertiesToUpdate.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToDictionary(p => p.Name, p => p.GetValue(propertiesToUpdate, null));
                QueueUpdate(primaryKeyValue, modelType, props);
            }

            /// <summary>
            /// Updates certain properties in a given model.
            /// </summary>
            /// <typeparam name="TModel"></typeparam>
            /// <param name="primaryKey"></param>
            /// <param name="propertiesToUpdate"></param>
            public void QueueUpdate(int primaryKeyValue, Type modelType, Dictionary<string,object> propertiesToUpdate)
            {
                var modelMap = DataContextExtensions.GetModelDbMap(modelType);

                int paramNum = 0;
                var valueParams = propertiesToUpdate.Select(prop =>
                {
                    var paramName = "@p" + commandIndex.ToInvariant() + "_" + paramNum.ToInvariant();
                    paramNum++;

                    var col = modelMap.ColumnsPerProperty[prop.Key];
                    var dbParam = col.CreateParameter(paramName, ParameterDirection.Input, prop.Value);
                    if (col.IsPrimaryKey)
                        throw new InvalidOperationException("Parameter for " + col.ColumnName + " is a primary key.  That's not allowed.");
                    command.Parameters.Add(dbParam);
                    return dbParam;
                }).ToArray();
                if (valueParams.Length == 0)
                    throw new InvalidOperationException("No columns to update.");

                var primaryKey = modelMap.PrimaryKey;
                var primaryKeyParam = primaryKey.CreateParameter("@p" + commandIndex.ToInvariant() + "_" + paramNum.ToInvariant(), ParameterDirection.Input, primaryKeyValue);
                command.Parameters.Add(primaryKeyParam);
                paramNum++;

                string commandText = "UPDATE " + modelMap.TableName + " SET " + string.Join(",", valueParams.Select(p => p.SourceColumn.Surround("[", "]") + " = " + p.ParameterName).ToArray()) + " WHERE " + primaryKeyParam.SourceColumn.Surround("[", "]") + " = " + primaryKeyParam.ParameterName;
                AddCommandText(commandText);

            }

            #endregion

            #region Delete

            /// <summary>
            /// Queues a DELETE command for the Linq2Sql object passed in, using its primary key value to specify the record. 
            /// </summary>
            /// <param name="model"></param>
            public void QueueDeleteViaPrimaryKey(object model)
            {
                var modelType = model.GetType();
                var modelMap = DataContextExtensions.GetModelDbMap(modelType);

                var primaryKey = modelMap.PrimaryKey;
                var paramName = "@p" + commandIndex.ToInvariant();
                var dbParam = new SqlParameter(paramName, primaryKey.DbType);
                dbParam.SourceColumn = primaryKey.ColumnName;
                if (primaryKey.Size > 0)
                    dbParam.Size = primaryKey.Size;
                dbParam.Direction = ParameterDirection.Input;
                dbParam.IsNullable = primaryKey.IsNullable;
                dbParam.Value = primaryKey.PropertyInfo.GetValue(model, null);
                if (dbParam.Value == null)
                    dbParam.Value = DBNull.Value;
                command.Parameters.Add(dbParam);

                string commandText = "DELETE " + modelMap.TableName + " WHERE " + primaryKey.ColumnName.StartWith("[").EndWith("]") + " = " + dbParam.ParameterName;
                AddCommandText(commandText);
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                command.Dispose();
                command = null;
                connection.Dispose();
                connection = null;
                callbacks.Clear();
                Executing = null;
            }

            #endregion
        }

    /// <summary>
        /// Used to set a value for a column in an insert statement to a previous parameter (like ParentId).  Example could be   [ParentId] ... values .... @p901Id, where @p901Id is from a parmeter set earlier in the command
    /// </summary>
    public class SubstituteParameter
    {
        public string ColumnName { get; set; }
        public SqlParameter ValueParameter { get; set; }
        /// <summary>
        /// Invoked after the command has been executed.  
        /// </summary>
        public Action<object> ApplyValue { get; set; }
    }

}
