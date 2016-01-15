using System.Text;

namespace GrowingData.Utilities.DnxCore {
	public static class UrlString {

		public static string UrlSafeString(this string title) {
			StringBuilder sb = new StringBuilder();

			foreach (char c in title) {
				if (c == ' ') {
					sb.Append('-');
				} else {
					if (char.IsLetterOrDigit(c)) {
						sb.Append(c);
					}
				}
			}

			return sb.ToString();
		}
	}
}
