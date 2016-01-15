using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Data.SqlClient;

using GrowingData.Utilities.DnxCore;

namespace GrowingData.Mung.Web.Models {
	public class Setting {
		public class MissingSettingException : Exception {
			public string MissingSettingKey = null;
			public MissingSettingException(string key) : base($"Unable to load the setting: '{key}'.") {
				MissingSettingKey = key;

			}

		}
		private static Dictionary<string, Setting> _AppSettings = null;


		public static void InitializeSettings() {
			_AppSettings = List().ToDictionary(x => x.Key);

		}

		public static string Get(string key) {
			if (_AppSettings.ContainsKey(key)) {
				return _AppSettings[key].Value;
			}
			throw new MissingSettingException(key);
		}

		public string Value;
		public string Key;

		public Setting() { }

		public Setting(string key, string value) {
			Key = key;
			Value = value;
		}

		/// <summary>
		/// List all connections
		/// </summary>
		/// <param name="connectionId"></param>
		/// <returns></returns>
		public static List<Setting> List() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var apps = cn.ExecuteAnonymousSql<Setting>(@"SELECT * FROM setting", null);
				return apps;
			}
		}
		
		/// <summary>
		/// Insert the setting into the 
		/// </summary>
		/// <returns></returns>
		public bool Insert() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"INSERT INTO setting(key, value) VALUES (@Key, @Value)";
				cn.ExecuteSql(sql, this);
				_AppSettings = List().ToDictionary(x => x.Key);
				return true;
			}
		}

		public bool Save() {
			using (var cn = DatabaseContext.Db.Mung()) {
				Delete();
				Insert();
				_AppSettings = List().ToDictionary(x => x.Key);
				return true;
			}
		}

		/// <summary>
		/// Delete a connection from the list
		/// </summary>
		/// <returns></returns>
		public bool Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM setting WHERE key = @Key";
				cn.ExecuteSql(sql, this);
				return true;
			}
		}
	}

}
