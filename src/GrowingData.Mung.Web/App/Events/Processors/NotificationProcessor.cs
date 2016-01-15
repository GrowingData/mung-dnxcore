using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
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

					Console.WriteLine($"SesNotification.SendNotification({mungEvent.Type}, {fire.NotificationId})");
					// Do this in a queue later...
					SesNotification.SendNotification(fire, mungEvent);
				}
			}


		}



	}
}
