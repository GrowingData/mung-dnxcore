using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace GrowingData.Mung {
	public class DatabaseContext {
		private static DatabaseContext _Db = null;
		public static DatabaseContext Db {
			get {
				return _Db;
			}
		}


		public static void Initialize(string connectionString) {
			Console.WriteLine("DatabaseContext initialized!");

			_Db = new DatabaseContext(connectionString);
		}


		private string _sqlWarehouseConnectionString;
		public DatabaseContext(string connectionString) {
			_sqlWarehouseConnectionString = connectionString;
		}



		public Func<DbConnection> Warehouse {
			get {
				return () => {
					var cn = new SqlConnection(_sqlWarehouseConnectionString);
					cn.Open();
					return cn;
				};
			}
		}
		public Func<DbConnection> Metadata {
			get {
				return () => {
					var cn = new SqlConnection(_sqlWarehouseConnectionString);
					cn.Open();
					return cn;
				};
			}
		}
	}
}