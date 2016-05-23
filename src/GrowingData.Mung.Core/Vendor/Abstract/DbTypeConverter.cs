using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrowingData.Utilities.Database;

namespace GrowingData.Mung.Core {
	public abstract class DbTypeConverter {

		public abstract string GetCreateColumnDefinition(MungType type);
		public abstract MungType GetTypeFromInformationSchema(string infoSchemaType);


		public static SqlServerTypeConverter SqlServer = new SqlServerTypeConverter();
		public static PostgresqlDbTypeConverter Postgresql = new PostgresqlDbTypeConverter();

	}
}
