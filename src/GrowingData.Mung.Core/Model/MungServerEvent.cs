using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace GrowingData.Mung.Core {
	public class MungServerEvent {
		public DateTime LogTime;
		public JToken Token;
		public JToken Data;
		public string Type;
		public string Source;
		public int AppId;

		// Loaded directly from Json
		public MungServerEvent() { }
	}
}