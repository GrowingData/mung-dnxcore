using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace GrowingData.Utilities.Database {

	public class SqlServerSchemaReader : DbSchemaReader {
		//private SqlConnection _cn;

		public SqlServerSchemaReader(Func<DbConnection> cn) : base(cn) {
			//_cn = cn as SqlConnection;
			//if (_cn == null) {
			//	throw new Exception("Unable to convert the supplied connection to a SqlConnection");
			//}
		}

		public override string SchemaSql {
			get {
				return @"
					SELECT table_schema, table_name, column_name, data_type
					FROM information_schema.columns C
					WHERE	(table_schema = @Schema OR @Schema IS NULL)
					AND		(table_name = @Table OR @Table IS NULL)
					ORDER BY table_schema, ordinal_position
				";
			}
		}

		public override DbTypeConverter TypeConverter { get { return DbTypeConverter.SqlServer; } }
	}


}