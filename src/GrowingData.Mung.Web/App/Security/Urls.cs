using System;

namespace GrowingData.Mung.Web {
	public class Urls {

		public const string INITIAL_ACCOUNT_CREATE = "setup";
		public const string LOGIN = "signin";
		public const string LOGOUT = "signout";

		public const string DEFAULT = "";

		/// <summary>
		/// Remove the leading slash in a URL
		/// </summary>
		/// <param name="slashBasedUrl"></param>
		/// <returns></returns>
		public static string Routify(string slashBasedUrl) {
			if (string.IsNullOrEmpty(slashBasedUrl)) {
				return slashBasedUrl;
			}

			if (slashBasedUrl.Length >= 1) {
				if (slashBasedUrl[0] != '/') {
					return slashBasedUrl;
				}
			}

			return slashBasedUrl.Substring(1);
		}
	}
}