﻿using System;
using System.Collections.Generic;

namespace Lfx.Data
{
        public interface IConnection : Lazaro.Orm.Data.IConnection
        {
                bool ConstraintsEnabled { get; }
                string DataBaseName { get; }
                DateTime ServerDateTime { get; }
                qGen.SqlModes SqlMode { get; }
                string ServerName { get; }
                string ServerVersion { get; }

                bool HasLock(string lockName);
                void SetLock(bool enable, string lockName);

                void EnableConstraints(bool enable);
                void SetConstraints(Dictionary<string, ConstraintDefinition> newConstraints, bool deleteOnly);
                IDictionary<string, ConstraintDefinition> GetConstraints();
                TableCollection GetTables();
                Data.TableStructure GetTableStructure(string tableName, bool withConstraints);
                void SetTableStructure(Data.TableStructure newTableDef);

                int FieldInt(string selectCommand);
                int FieldInt(qGen.Select selectCommand);
                string FieldString(qGen.Select selectCommand);
                string FieldString(string selectCommand);
                decimal FieldDecimal(qGen.Select selectCommand);
                decimal FieldDecimal(string selectCommand);
                IList<int> FieldIntCollection(string selectCommand);
                DateTime FieldDateTime(string selectCommand, DateTime defaultValue);
                string FieldIntCSV(string selectCommand);

                System.Data.DataTable Select(qGen.Select selectCommand);
                System.Data.DataTable Select(string selectCommand);
                int Update(qGen.Update updateCommand);
                int Delete(qGen.Delete deleteCommand);
                int Insert(qGen.Insert insertCommand);

                Row FirstRowFromSelect(qGen.Select selectCommand);
                Row FirstRowFromSelect(string selectCommand);
                Row Row(string tableName, string fieldList, string idField, int id);
                Row Row(string tableName, string idField, int id);

                int ExecuteSql(string sqlCommand);
                int Execute(qGen.ICommand sqlCommand);

                string EscapeString(string stringValue);
        }
}
