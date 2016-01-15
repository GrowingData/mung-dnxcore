using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web {
	public static class SesNotificationSettings {
		public const string SesFromAddressSettingKey = "Notifications-SES-FromAddress";
		public const string RenderTemplateUrlSettingKey = "Notifications-RenderTemplateUrl";

		public const string SesAccessKeySettingKey = "Notifications-SES-AccessKey";
		public const string SesSecretKeySettingKey = "Notifications-SES-AccessSecret";

		public static string SesFromAddress {
			get {
				return Setting.Get(SesFromAddressSettingKey);
			}
		}
		public static string SesAccessKey {
			get {
				return Setting.Get(SesAccessKeySettingKey);
			}
		}
		public static string SesSecretKey {
			get {
				return Setting.Get(SesSecretKeySettingKey);
			}
		}
		public static string RenderTemplateUrl {
			get {
				return Setting.Get(RenderTemplateUrlSettingKey);
			}
		}

		public static BasicAWSCredentials SesCredentials {
			get {
				return new BasicAWSCredentials(
					SesNotificationSettings.SesAccessKey,
					SesNotificationSettings.SesSecretKey
				);
			}
		}

		public static bool HasConfiguration() {
			try {
				var emailAccount = SesFromAddress;
				var accessKey = SesAccessKey;
				var accessSecret = SesSecretKey;
				var renderTemplateUrl = RenderTemplateUrl;
				return true;
			} catch (Exception ex) {
				return false;
			}
		}
	}
}
