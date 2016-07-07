using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrowingData.Utilities.Database {


	public class SqlServerType {
		public string CreateColumnDefinition;
		public string InfoSchemaName;
		public MungType MungType;

		public SqlServerType(MungType mt, string infoSchemaName, string columnDefinition) {
			MungType = mt;
			InfoSchemaName = infoSchemaName;
			CreateColumnDefinition = columnDefinition;
		}
	}

	public class SqlServerTypeConverter : DbTypeConverter {

		public static HashSet<string> IgnoreTypes = new HashSet<string>() {
			"geography"
		};

		public static List<SqlServerType> Types = new List<SqlServerType>() {
			new SqlServerType(MungType.Get(typeof(string)), "nvarchar", "nvarchar(max)"),
			new SqlServerType(MungType.Get(typeof(string)), "varchar", "varchar(max)"),


			new SqlServerType(MungType.Get(typeof(bool)), "bit", "bit"),

			new SqlServerType(MungType.Get(typeof(DateTime)), "datetime", "datetime"),
			new SqlServerType(MungType.Get(typeof(DateTime)), "datetime2", "datetime2"),
			new SqlServerType(MungType.Get(typeof(DateTime)), "smalldatetime", "smalldatetime"),
			new SqlServerType(MungType.Get(typeof(DateTime)), "date", "date"),
			new SqlServerType(MungType.Get(typeof(DateTime)), "datetimeoffset", "datetimeoffset"),

			new SqlServerType(MungType.Get(typeof(string)), "char", "char(100)"),
			new SqlServerType(MungType.Get(typeof(float)), "float", "float"),
			new SqlServerType(MungType.Get(typeof(double)), "float", "float"),
			new SqlServerType(MungType.Get(typeof(double)), "real", "real"),

			new SqlServerType(MungType.Get(typeof(decimal)), "money", "money"),
			new SqlServerType(MungType.Get(typeof(decimal)), "decimal", "decimal"),

			new SqlServerType(MungType.Get(typeof(byte)), "tinyint", "tinyint"),
			new SqlServerType(MungType.Get(typeof(short)), "smallint", "smallint"),
			new SqlServerType(MungType.Get(typeof(int)), "int", "int"),
			new SqlServerType(MungType.Get(typeof(long)), "bigint", "bigint"),
			new SqlServerType(MungType.Get(typeof(Guid)), "uniqueidentifier", "uniqueidentifier")
		};


		public override string GetCreateColumnDefinition(MungType type) {
			var t = Types.FirstOrDefault(x => x.MungType == type);
			if (t == null) {
				throw new InvalidOperationException($"MungType for {type.DotNetType} is unknown to SqlServerTypeConverter");
			}
			return t.CreateColumnDefinition;
		}


		public override MungType GetTypeFromInformationSchema(string infoSchemaName) {
			if (IgnoreTypes.Contains(infoSchemaName)) {
				return null;
			}

			var t = Types.FirstOrDefault(x => x.InfoSchemaName == infoSchemaName);
			if (t == null) {
				throw new InvalidOperationException($"MungType for {infoSchemaName} is unknown to SqlServerTypeConverter");
			}
			return t.MungType;

		}
	}
}
