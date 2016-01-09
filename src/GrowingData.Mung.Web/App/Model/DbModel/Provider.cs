using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GrowingData.Utilities.DnxCore;

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
						_providers = cn.ExecuteAnonymousSql<Provider>(sql, null);
					}
				}
				return _providers;
			}

		}
	}
}