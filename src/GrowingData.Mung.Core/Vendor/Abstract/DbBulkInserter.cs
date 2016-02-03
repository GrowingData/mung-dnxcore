using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using GrowingData.Mung.Core;

namespace GrowingData.Mung.Core {
	public abstract class DbBulkInserter {

		public abstract bool CreateTable(DbTable tbl);
		public abstract bool BulkInsert(DbTable schema, MsvReader reader);
		public abstract bool BulkInsert(DbDataReader reader, Action<DbDataReader> eachRow);
		public abstract bool ModifySchema(DbTable oldSchema, DbTable newSchema);


		public abstract DbTable GetDbSchema();

		protected string _targetSchema;
		protected string _targetTable;

		public string Schema { get { return _targetSchema; } }
		public string TableName { get { return _targetTable; } }

		protected DbBulkInserter(Func<DbConnection> targetConnection, string targetSchema, string targetTable) {
			_targetSchema = targetSchema;
			_targetTable = targetTable;

		}

		public bool Execute(string filename) {
			using (var stream = File.OpenText(filename)) {
				using (var reader = new MsvReader(stream)) {
					var currentSchema = new DbTable(_targetTable, _targetSchema, reader.Columns);
					var oldSchema = GetDbSchema();

					// Check / Update the schema in the DB
					if (oldSchema == null) {
						CreateTable(currentSchema);
					} else {
						ModifySchema(oldSchema, currentSchema);
					}

					var newSchema = GetDbSchema();

					BulkInsert(newSchema, reader);
				}

			}
			return true;
		}




	}
}
