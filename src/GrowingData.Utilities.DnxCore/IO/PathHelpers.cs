using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GrowingData.Utilities.DnxCore {
	public class PathHelpers {
		public static void CreateDirectoryRecursively(string path) {
			string[] pathParts = path.Split('\\');

			for (int i = 0; i < pathParts.Length; i++) {
				if (i > 0) {
					pathParts[i] = Path.Combine(pathParts[i - 1], pathParts[i]);
				}
				if (!Directory.Exists(pathParts[i])) {
					Directory.CreateDirectory(pathParts[i]);
				}
			}
		}
	}
}

