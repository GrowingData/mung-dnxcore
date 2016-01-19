using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GrowingData.Utilities.DnxCore;
using HtmlAgilityPack;

namespace GrowingData.Mung.Web.Models {

	public class Notification : IBracketsEditable {


		public int NotificationId;
		public string Name;

		private string _template = null;
		public string Template {
			get {
				return _template;
			}
			set {
				_eventType = null;
				_template = value;
			}
		}

		public int CreatedByMunger;
		public int UpdatedByMunger;

		public bool IsPaused;

		public DateTime CreatedAt;
		public DateTime UpdatedAt;

		private string _eventType;

		public string EventType {
			get {
				if (_eventType != null) {
					return _eventType;
				}

				var doc = new HtmlDocument();
				doc.LoadHtml(Template);
				_eventType = doc.DocumentNode.SelectNodes("//mung-event-type").FirstOrDefault()?.InnerText;

				return _eventType;
			}
			set {
				// Don't do anything here, but we need the setter because the
				// DbBinder will try to get this from the cached version.
			}
		}

		public string ResourceUrl { get { return $"/{MungFileSystem.NotificationRootUrlPart}/{Name}"; } }


		public static List<Notification> List(int mungerId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM notification ORDER BY name";
				return cn.ExecuteAnonymousSql<Notification>(sql, null);
			}
		}

		public static Notification Get(int notificationId) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM notification WHERE notification_id = @NotificationId";
				return cn.ExecuteAnonymousSql<Notification>(sql, new { NotificationId = notificationId }).FirstOrDefault();
			}
		}
		public static Notification Get(string name) {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"SELECT * FROM notification WHERE name = @Name";
				return cn.ExecuteAnonymousSql<Notification>(sql, new { Name = name }).FirstOrDefault();
			}
		}

		public void Delete() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"DELETE FROM notification WHERE name = @Name";
				cn.ExecuteSql<Notification>(sql, this);
			}
		}

		public Notification Save() {

			if (NotificationId <= 0) {
				return Insert();
			} else {
				return Update();
			}

		}

		public Notification Insert() {

			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					INSERT INTO notification(name, event_type, template, created_by_munger, updated_by_munger, is_paused)
						VALUES (@Name, @EventType, @Template, @CreatedByMungerId, @CreatedByMungerId, @IsPaused)
						RETURNING notification_id";

				NotificationId = cn.DumpList<int>(sql, this).FirstOrDefault();

				MungApp.Current.NotificationProcessor.ReloadNotifications();
				return this;
			}
		}

		public Notification Update() {
			using (var cn = DatabaseContext.Db.Mung()) {
				var sql = @"
					UPDATE notification
						SET name = @Name, 
							event_type = @EventType, 
							template = @Template,
							is_paused = @IsPaused,
							updated_by_munger = @UpdatedByMunger,
							updated_at = now() at time zone 'utc'
						WHERE notification_id = @NotificationId";
				cn.ExecuteSql(sql, this);

				MungApp.Current.NotificationProcessor.ReloadNotifications();
				return this;
			}
		}


	}
}
