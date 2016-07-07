using System;
using System.Data;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Linq;

namespace GrowingData.Utilities.Database {
	public class PostgresqlType {

		public string InfoSchemaName;
		public string CreateColumnDefinition;
		public NpgsqlDbType PostgresqlDbType;
		public DbType DatabaseType;
		public MungType MungType;

		public PostgresqlType(string postgresqlColumnType, string infoSchemaName, NpgsqlDbType postgresqlDbType, DbType databaseType, MungTypeCode code) {
			InfoSchemaName = infoSchemaName;
			CreateColumnDefinition = postgresqlColumnType;
			PostgresqlDbType = postgresqlDbType;
			DatabaseType = databaseType;
			MungType = MungType.Get(code);
		}

	}
	public class PostgresqlTypeConverter : DbTypeConverter {

		public static HashSet<string> IgnoreTypes = new HashSet<string>() {
			"json"
		};


		public static PostgresqlType Get(MungType type) {
			var t = Types.FirstOrDefault(p => p.MungType.Code == type.Code);
			if (t == null) {
				throw new Exception($"Unable to find PostgresqlType for: '{type.Code.ToString()}' ({type.DotNetType})");
			}

			return t;
		}
		/// <summary>
		///  The ordering of these type sis important, as most lookups of this table
		///  will use .FirstOrDefault
		/// </summary>
		public static List<PostgresqlType> Types = new List<PostgresqlType>() {

			new PostgresqlType("text", "text", NpgsqlDbType.Text, DbType.String, MungTypeCode.String),
			new PostgresqlType("bool", "bool", NpgsqlDbType.Boolean, DbType.Boolean, MungTypeCode.Bool),

			new PostgresqlType("timestamptz", "timestamp with time zone", NpgsqlDbType.TimestampTZ, DbType.DateTime, MungTypeCode.DateTime),

			new PostgresqlType("double precision", "double precision", NpgsqlDbType.Double, DbType.Double, MungTypeCode.Double),
			new PostgresqlType("real", "real", NpgsqlDbType.Numeric, DbType.Decimal, MungTypeCode.Decimal),
			new PostgresqlType("real", "real", NpgsqlDbType.Real, DbType.Decimal, MungTypeCode.Decimal),

			new PostgresqlType("bigint","bigint", NpgsqlDbType.Bigint, DbType.Int64, MungTypeCode.Integer),

			new PostgresqlType("uuid", "uuid",NpgsqlDbType.Uuid, DbType.Guid, MungTypeCode.Guid),


			// Lets add in some type alias'
			new PostgresqlType("bool", "boolean", NpgsqlDbType.Boolean, DbType.Boolean, MungTypeCode.Bool),
			new PostgresqlType("timestamptz", "timestamp", NpgsqlDbType.Timestamp, DbType.DateTime, MungTypeCode.DateTime),
			new PostgresqlType("timestamptz", "date", NpgsqlDbType.Date, DbType.DateTime, MungTypeCode.DateTime),
			new PostgresqlType("timestamptz", "time", NpgsqlDbType.Time, DbType.DateTime, MungTypeCode.DateTime),
			new PostgresqlType("timestamptz", "time with time zone", NpgsqlDbType.TimeTZ, DbType.DateTime, MungTypeCode.DateTime),

			new PostgresqlType("double precision", "real", NpgsqlDbType.TimestampTZ, DbType.DateTime, MungTypeCode.Double),
			new PostgresqlType("real", "numeric", NpgsqlDbType.TimestampTZ, DbType.DateTime, MungTypeCode.Decimal),
			new PostgresqlType("bigint","smallint", NpgsqlDbType.Bigint, DbType.Int64, MungTypeCode.Integer),
			new PostgresqlType("bigint","integer", NpgsqlDbType.Bigint, DbType.Int64, MungTypeCode.Integer)


		};

		public override string GetCreateColumnDefinition(MungType type) {
			var t = Get(type);
			if (t == null) {
				throw new InvalidOperationException($"MungType for {type.DotNetType} is unknown to PostgresqlDbTypeConverter");
			}
			return t.CreateColumnDefinition;
		}


		public override MungType GetTypeFromInformationSchema(string infoSchemaName) {
			if (IgnoreTypes.Contains(infoSchemaName)) {
				return null;
			}

			var t = Types.FirstOrDefault(x => x.InfoSchemaName == infoSchemaName);
			if (t == null) {
				throw new InvalidOperationException($"MungType for {infoSchemaName} is unknown to PostgresqlDbTypeConverter");
			}
			return t.MungType;

		}

	}


}