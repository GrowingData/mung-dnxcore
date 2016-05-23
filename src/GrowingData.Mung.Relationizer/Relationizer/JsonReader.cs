using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrowingData.Mung.Core;
using System.Data;
using GrowingData.Utilities.Database;

namespace GrowingData.Mung.Relationizer {
	public class JsonReader {


		private static JsonConverter[] Converters = new JsonConverter[] {
				new IsoDateTimeConverter()
		};


		public static object GetValue(JProperty token, MungType mungType) {
			return token.Value.ToObject<object>();
		}

		public static IEnumerable<RelationalEvent> Process(MungServerEvent evt, string json, string eventName) {

			var firstToken = JToken.Parse(json);
			return Process(evt, firstToken, eventName, null);


		}
		public static IEnumerable<RelationalEvent> Process(MungServerEvent evt, JToken t, string eventName, RelationalEvent parent) {
			if (t.Type == JTokenType.Object) {
				foreach (var r in ProcessJsonObject(evt, t, eventName, null)) {
					yield return r;

				}
			}
			if (t.Type == JTokenType.Array) {
				foreach (var r in ProcessJsonArray(evt, t, eventName, null)) {
					yield return r;

				}
			}
		}

		public static IEnumerable<RelationalEvent> ProcessJsonArray(MungServerEvent evt, JToken t, string eventName, RelationalEvent parent) {

			if (t.Type == JTokenType.Array) {
				foreach (var child in t.Children()) {
					if (child.Type == JTokenType.Object) {
						foreach (var r in ProcessJsonObject(evt, child, eventName, parent)) {
							yield return r;
						}

					}

				}
			}
		}

		public static IEnumerable<RelationalEvent> ProcessJsonObject(MungServerEvent evt, JToken t, string eventName, RelationalEvent parent) {
			var relation = new RelationalEvent(evt);

			relation.Id = RelationalEvent.GetKey();
			relation.LogTime = evt.LogTime;
			relation.Name = eventName.ToLowerInvariant();


			if (parent != null) {
				relation.ParentId = parent.Id;
				relation.ParentType = parent.Name;

				relation.Name = relation.ParentType + "_" + relation.Name;
			} else {
				relation.ParentId = null;
				relation.ParentType = null;
			}


			if (t.Type == JTokenType.Object) {
				// If we have an object, we want to create fields for all the 
				// "simple" types, and child items for all the complex ones

				foreach (var child in t.Children()) {
					if (child.Type == JTokenType.Property) {
						var property = child as JProperty;

						if (property.Value.Type == JTokenType.Array) {
							foreach (var r in ProcessJsonArray(evt, property.Value, property.Name, relation)) {
								yield return r;
							}
							continue;
						}
						if (property.Value.Type == JTokenType.Object) {
							foreach (var r in ProcessJsonObject(evt, property.Value, property.Name, relation)) {
								yield return r;
							}
							continue;

						}

						// Base type, add it as a field
						if (property.Value.Type != JTokenType.Null) {
							var mungType = MungType.Get(property.Value.Type);
							relation.AddField(property.Name, mungType, GetValue(property, mungType));
						}
					}
				}
			}
			yield return relation;
		}




	}

}
