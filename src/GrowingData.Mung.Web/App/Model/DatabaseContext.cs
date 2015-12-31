using System;
using System.Data.Common;
using Npgsql;

namespace GrowingData.Mung {
	public class DatabaseContext {
		private static DatabaseContext _Db = null;
		public static DatabaseContext Db {
			get {
				return _Db;
			}
		}


		public static void Initialize(string mungConnectionString, string eventsConnectionString) {

			_Db = new DatabaseContext(mungConnectionString, eventsConnectionString);
			Console.WriteLine("DatabaseContext initialized!");
		}



		private string _mungConnectionString;
		private string _eventsConnectionString;
		public DatabaseContext(string mungConnectionString, string eventsConnectionString) {
			_mungConnectionString = mungConnectionString;
			_eventsConnectionString = eventsConnectionString;
		}

		public Func<DbConnection> Events {
			get {
				return () => {
					var cn = new NpgsqlConnection(_eventsConnectionString);
					cn.Open();
					return cn;
				};
			}
		}


		public Func<DbConnection> Mung {
			get {
				return () => {
					var cn = new NpgsqlConnection(_mungConnectionString);
					cn.Open();
					return cn;
				};
			}
		}


	}
}