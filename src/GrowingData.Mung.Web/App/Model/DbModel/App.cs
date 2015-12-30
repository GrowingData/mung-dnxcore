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
				defaultApp.Save();
				Console.WriteLine("Created a default app");

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

		public int CreatedByUserId;
		public int ModifiedByUserId;

		public App() { }

		public App(string name, int createdByUserId) {
			AppId = -1;
			Name = name;
			AppSecret = JwtHelper.GenerateSecret();
			AppKey = RandomString.Get(16);
			CreatedAt = DateTime.UtcNow;
			UpdatedAt = DateTime.UtcNow;
			CreatedByUserId = createdByUserId;
			ModifiedByUserId = createdByUserId;
		}


		/// <summary>
		/// Get a connection
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		//public static App Get(string appKey) {
		//	using (var cn = DatabaseContext.Db.Metadata()) {
		//		var app = cn.ExecuteAnonymousSql<App>(
		//				@"SELECT * FROM mung.App WHERE AppId = @AppId",
		//				 new { appId = appId }
		//			)
		//			.FirstOrDefault();
		//		return app;
		//	}
		//}

		/// <summary>
		/// List all connections
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public static List<App> List() {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var apps = cn.ExecuteAnonymousSql<App>(@"SELECT * FROM mung.App", null);
				return apps;
			}
		}

		/// <summary>
		/// Save or Create a Connection reference
		/// </summary>
		/// <returns></returns>
		public bool Save() {
			using (var cn = DatabaseContext.Db.Metadata()) {

				if (AppId == -1) {
					var sql = @"
	INSERT INTO mung.App(Name, AppSecret, AppKey, CreatedAt, UpdatedAt, CreatedByUserId, ModifiedByUserId)
		SELECT @Name, @AppSecret, @AppKey, @CreatedAt, @UpdatedAt, @CreatedByUserId, @CreatedByUserId";
					cn.ExecuteSql(sql, new {
						Name = Name,
						AppKey = AppKey,
						AppSecret = AppSecret,
						UpdatedAt = DateTime.UtcNow,
						CreatedAt = DateTime.UtcNow,
						CreatedByUserId = CreatedByUserId
					});


				} else {

					var sql = @"
	UPDATE mung.App
		SET Name=@Name,
			UpdatedAt = @UpdatedAt,
			ModifiedByUserId = @ModifiedByUserId
		WHERE AppId = @AppId";
					cn.ExecuteSql(sql, new {
						Name = Name,
						UpdatedAt = DateTime.UtcNow,
						ModifiedByUserId = ModifiedByUserId,
						AppId = AppId
					});
				}
			}
			// Update the appCache
			_AppCache = List().ToDictionary(x => x.AppKey);

			return true;
		}


		/// <summary>
		/// Delete a connection from the list
		/// </summary>
		/// <returns></returns>
		public bool Delete() {
			using (var cn = DatabaseContext.Db.Metadata()) {
				var sql = @"
	DELETE FROM mung.App
		WHERE AppId = @AppId";
				cn.ExecuteSql(sql, new {
					AppId = AppId
				});

				return true;
			}
		}
	}

}
