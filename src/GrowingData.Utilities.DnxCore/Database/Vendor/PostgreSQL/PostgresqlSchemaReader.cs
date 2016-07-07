using System;
using System.Data.Common;
using Npgsql;

namespace GrowingData.Utilities.Database {

	public class PostgresqlDbSchemaReader : DbSchemaReader {
		//private NpgsqlConnection _cn;

		public PostgresqlDbSchemaReader(Func<DbConnection> cn) : base(cn) {
			//_cn = cn as NpgsqlConnection;
			//if (_cn == null) {
			//	throw new Exception("Unable to convert the supplied connection to a NpgsqlConnection");
			//}
		}
		public override string SchemaSql {
			get {
				return @"
					SELECT table_schema, table_name, column_name, data_type
					FROM information_schema.columns C
					WHERE table_schema NOT IN ('information_schema', 'pg_catalog')
					AND		(table_schema = @Schema OR @Schema IS NULL)
					AND		(table_name = @Table OR @Table IS NULL)
					ORDER BY table_schema, ordinal_position
				";
			}
		}
		public override DbTypeConverter TypeConverter { get { return DbTypeConverter.Postgresql; } }
	}


}