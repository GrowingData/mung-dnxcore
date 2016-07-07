using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrowingData.Utilities.Database {
	public abstract class DbTypeConverter {

		public abstract string GetCreateColumnDefinition(MungType type);
		public abstract MungType GetTypeFromInformationSchema(string infoSchemaType);


		public static SqlServerTypeConverter SqlServer = new SqlServerTypeConverter();
		public static PostgresqlTypeConverter Postgresql = new PostgresqlTypeConverter();

	}
}
