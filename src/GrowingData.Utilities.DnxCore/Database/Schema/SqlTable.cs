using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;

namespace GrowingData.Utilities.Database {
	public class SqlTable {
		public string TableName;
		public string SchemaName;
		public List<SqlColumn> Columns;

		public SqlTable(string tableName, string schemaName) {
			TableName = tableName;
			SchemaName = schemaName;
			Columns = new List<SqlColumn>();
		}
		public SqlTable(string tableName, string schemaName, List<SqlColumn> columns) {
			TableName = tableName;
			SchemaName = schemaName;
			Columns = columns;
		}

	}
}