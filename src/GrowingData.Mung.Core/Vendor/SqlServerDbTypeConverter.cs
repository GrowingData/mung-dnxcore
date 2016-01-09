using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrowingData.Mung.Core {


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



		public static List<SqlServerType> Types = new List<SqlServerType>() {
			new SqlServerType(MungType.Get(typeof(string)), "nvarchar", "nvarchar(max)"),
			new SqlServerType(MungType.Get(typeof(string)), "varchar", "varchar(max)"),


			new SqlServerType(MungType.Get(typeof(bool)), "bit", "bit"),
			new SqlServerType(MungType.Get(typeof(DateTime)), "datetime", "datetime"),
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
			var t = Types.FirstOrDefault(x => x.InfoSchemaName == infoSchemaName);
			if (t == null) {
				throw new InvalidOperationException($"MungType for {infoSchemaName} is unknown to SqlServerTypeConverter");
			}
			return t.MungType;

		}
	}
}
