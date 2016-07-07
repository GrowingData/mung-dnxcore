using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using GrowingData.Utilities.Csv;

namespace GrowingData.Utilities.Database {
	public abstract class DbBulkInserter {



		public abstract bool CreateTable(SqlTable tbl);
		public abstract bool CreateSchemaIfRequired(string targetSchema);

		public abstract bool BulkInsert(string schemaName, string tableName, CsvReader reader);
		public abstract bool BulkInsert(string schemaName, string tableName, DbDataReader reader, Action<DbDataReader> callback);
		public abstract bool ModifySchema(SqlTable oldSchema, SqlTable newSchema);


		public bool BulkInsert(string schemaName, string tableName, DbDataReader reader) {
			return BulkInsert(schemaName, tableName, reader, null);
		}

		public abstract SqlTable GetDbSchema(string schema, string table);


		protected DbBulkInserter() {

		}

		public bool Execute(string targetSchema, string targetTable, string filename) {
			try {
				using (var stream = File.OpenText(filename)) {
					using (var reader = new CsvReader(stream)) {

						BulkInsert(targetSchema, targetTable, reader);
					}

				}
				return true;
			} catch (Exception ex) {
				return false;
			}
		}


	}
}
