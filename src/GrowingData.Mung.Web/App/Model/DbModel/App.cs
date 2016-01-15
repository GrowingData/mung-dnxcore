using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Data.SqlClient;

using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class App {
		private static Dictionary<string, App> _AppCache = null;


		public static void InitializeApps() {
			_AppCache = List().ToDictionary(x => x.AppKey);

			if (_AppCache.Count == 0) {
				var defaultApp = new App("default", -1);
				defaultApp.Insert();
				Console.WriteLine("Created a default app");

				var mungInternalApp = new App("mung-internal", -1);
				mungInternalApp.Insert();
				Console.WriteLine("Created the mung-internal app");
			}
		}
		
		public static App MungInternal {
			get {
				return _AppCache.Values.FirstOrDefault(a => a.Name == "mung-internal");
			}
		}

		public static App Get(string appKey) {
			return _AppCache[appKey];
		}

		public int AppId;
		public string Name;
		public string AppSecret;
		public string AppKey;
		public DateTime CreatedAt;
		public DateTime UpdatedAt;

		public int CreatedByMunger;
		public int UpdatedByMunger;

		public App() { }

		public App(string name, int createdByUserId) {
			AppId = -1;
			Name = name;
			AppSecret = JwtHelper.GenerateSecret();
			AppKey = RandomString.Get(16);
			CreatedAt = DateTime.UtcNow;
			UpdatedAt = DateTime.UtcNow;
			CreatedByMunger = createdByUserId;
			UpdatedByMunger = createdByUserId;
		}

		/// <summary>
		/// List all connections
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public static List<App> List() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var apps = cn.ExecuteAnonymousSql<App>(@"SELECT * FROM app", null);
				return apps;
			}
		}

		/// <summary>
		/// Save or Create a Connection reference
		/// </summary>
		/// <returns></returns>
		public bool Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {

				var sql = @"
					INSERT INTO app(name, app_secret, app_key, created_by_munger, updated_by_munger)
						VALUES (@Name, @AppSecret, @AppKey, @CreatedByMunger, @CreatedByMunger)
						RETURNING app_id ";

				AppId = cn.ExecuteSql(sql, this);
				_AppCache = List().ToDictionary(x => x.AppKey);

				return true;

			}

		}


		/// <summary>
		/// Delete a connection from the list
		/// </summary>
		/// <returns></returns>
		public bool Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM app WHERE app_id = @AppId";
				cn.ExecuteSql(sql, this);

				return true;
			}
		}
	}

}
