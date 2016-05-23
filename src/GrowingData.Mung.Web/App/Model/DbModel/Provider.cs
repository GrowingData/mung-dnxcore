using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;
using GrowingData.Mung.Core;
using GrowingData.Utilities.Database;

namespace GrowingData.Mung.Web.Models {
	public class Provider {
		public int ProviderId;
		public string Name;

		private static List<Provider> _providers;

		public static List<Provider> Providers {
			get {
				if (_providers == null) {
					using (var cn = DatabaseContext.Db.Mung()) {
						var sql = @"SELECT * FROM provider";
						_providers = cn.SelectAnonymous<Provider>(sql, null);
					}
				}
				return _providers;
			}
		}

		public DbBulkInserter GetBulkInserter(Func<DbConnection> targetConnection, string targetSchema, string targetTable) {

			if (Name == "SQL Server") {
				throw new NotImplementedException("SQL Server doesn't have a bulk inserter working in DNX Core");
			}
			if (Name == "PostgreSQL") {
				return new PostgresqlBulkInserter(targetConnection, targetSchema, targetTable);
			}
			throw new Exception($"Unable to load a DbBulkInserter for Provider: {Name}");
		}

		public DbSchemaReader GetSchemaReader(Func<DbConnection> cn) {

			if (Name == "SQL Server") {
				return new SqlServerSchemaReader(cn);
			}
			if (Name == "PostgreSQL") {
				return new PostgresqlDbSchemaReader(cn);
			}

			throw new Exception($"Unable to load a DbSchemaReader for Provider: {Name}");

		}
	}
}