using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;

namespace GrowingData.Utilities.Database {
	public class DbTable {
		public string TableName;
		public string SchemaName;
		public List<DbColumn> Columns;

		public DbTable(string tableName, string schemaName) {
			TableName = tableName;
			SchemaName = schemaName;
			Columns = new List<DbColumn>();
		}
		public DbTable(string tableName, string schemaName, List<DbColumn> columns) {
			TableName = tableName;
			SchemaName = schemaName;
			Columns = columns;
		}
		
	}
}