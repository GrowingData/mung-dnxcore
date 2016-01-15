using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrowingData.Mung.Web {
	public class MungFileSystemEntry {
		public string Type;
		public string SubType;
		public string Name;
		public string Url;
		

		private MungFileSystemEntry Parent;
		public List<MungFileSystemEntry> Children = new List<MungFileSystemEntry>();


		public MungFileSystemEntry(string type, string subType, string name, string url, MungFileSystemEntry parent) {
			Type = type;
			SubType = subType;
			Name = name;
			Url = url;
			Parent = parent;
			if (parent != null) {
				parent.Children.Add(this);
			}
		}
	}

}
