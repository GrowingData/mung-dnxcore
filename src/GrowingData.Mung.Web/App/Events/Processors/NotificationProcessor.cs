using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GrowingData.Mung.Core;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web {


	public class NotificationProcessor : EventProcessor {

		private Dictionary<string, List<Notification>> _notifications;

		public NotificationProcessor() : base("NotificationProcessor") {
			ReloadNotifications();
		}
		public void ReloadNotifications() {
			_notifications = new Dictionary<string, List<Notification>>();
			foreach (var group in Notification.List(-1).GroupBy(x => x.EventType)) {
				_notifications[group.Key] = group.ToList();
			}

		}

		protected override void ProcessEvent(MungServerEvent mungEvent) {

			Console.WriteLine($"NotificationProcessor.ProcessEvent({mungEvent.Type})");
			if (_notifications.ContainsKey(mungEvent.Type)) {
				var toFire = _notifications[mungEvent.Type];
				foreach (var fire in toFire) {
					var filter = fire.EventFilter;
					var passedFilter = true;
					if (filter != null) {
						var filterToken = JToken.Parse(fire.EventFilter);
						foreach (var k in filterToken) {
							var prop = (k as JProperty);
							if (prop != null) {

								var propKey = prop.Name;
								var filterValue = filterToken[propKey] as JValue;
								var eventValue = mungEvent.Data[propKey] as JValue;

								if (filterValue == null) {
									continue;
								}
								if (eventValue == null) {
									passedFilter = false;
									break;
								}

								if (filterValue.Value?.ToString() != eventValue.Value?.ToString()) {
									// Nope, no match, so no notification
									passedFilter = false;
									break;

								}
							}
						}
					}

					if (passedFilter) {
						Console.WriteLine($"SesNotification.SendNotification({mungEvent.Type}, {fire.NotificationId})");
						// Do this in a queue later...
						SesNotification.SendNotification(fire, mungEvent);

					}
				}
			}


		}



	}
}
