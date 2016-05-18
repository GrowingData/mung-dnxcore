using System;
using System.Collections.Generic;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System.Linq;
using System.Threading.Tasks;
using GrowingData.Mung.Web.Models;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GrowingData.Mung.Core;
using HtmlAgilityPack;

namespace GrowingData.Mung.Web {
	public class SesNotification {

		public static bool TestConfiguration(string to, string from, BasicAWSCredentials credentials) {

			var exampleEmail = @"
			<mung-notification>
				<mung-to>_TO_</mung-to>
				<mung-cc></mung-cc>
				<mung-bcc></mung-bcc>
				<mung-to></mung-to>
				<mung-subject>Testing MUNG email configuration</mung-subject>
				<mung-body-html><h1>It works!</h1></mung-body-html>
				<mung-body-text>It works!</mung-body-text>
			</mung-notification>
			".Replace("_TO_", to);



			var emailRequest = CreateEmailRequest(exampleEmail, from);


			var region = Amazon.RegionEndpoint.USEast1;

			var client = new AmazonSimpleEmailServiceClient(credentials, region);
			var sendResult = client.SendEmailAsync(emailRequest).Result;

			return sendResult.HttpStatusCode == System.Net.HttpStatusCode.OK;

		}

		/// <summary>
		/// Checks to see if the SES Configuration exists, and is valid
		/// </summary>
		/// <returns></returns>
		public static bool TestConfiguration(string to, string from) {
			return TestConfiguration(to, from, SesNotificationSettings.SesCredentials);

		}

		public static bool TestConfiguration(string to, string from, string accessKey, string accessSecret) {
			return TestConfiguration(to, from, new BasicAWSCredentials(accessKey, accessSecret));
		}



		public static string RenderTemplate(Notification notification, MungServerEvent evt) {

			var json = JsonConvert.SerializeObject(evt);

			try {
				var renderTemplateUrl = SesNotificationSettings.RenderTemplateUrl;
				using (var client = new HttpClient()) {
					var postContent = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
					new KeyValuePair<string, string>("notificationId", $"{notification.NotificationId}"),
					new KeyValuePair<string, string>("event", json)

				});
					var result = client.PostAsync(renderTemplateUrl, postContent).Result;
					return result.Content.ReadAsStringAsync().Result;
				}
			} catch (Exception ex) {
				MungApp.Current.ProcessInternalEvent("notification_template_render_exception", new {
					Notification = notification,
					NotificationId = notification.NotificationId,
					Event = evt,
					Message = "Unable to render template",
					ExceptionMessage = ex.Message,
					ExceptionStack = ex.StackTrace
				});
				return null;
			}
		}

		public static SendEmailRequest CreateEmailRequest(string renderedTemplate, string from) {

			// Parse the document to get important details, like the subject, etc
			var doc = new HtmlDocument();
			doc.LoadHtml(renderedTemplate);

			var to = doc.DocumentNode.SelectNodes("//mung-to").FirstOrDefault()?.InnerText;
			var cc = doc.DocumentNode.SelectNodes("//mung-cc").FirstOrDefault()?.InnerText;
			var bcc = doc.DocumentNode.SelectNodes("//mung-bcc").FirstOrDefault()?.InnerText;
			var subjectText = doc.DocumentNode.SelectNodes("//mung-subject").FirstOrDefault()?.InnerText;
			var bodyHtml = doc.DocumentNode.SelectNodes("//mung-body-html").FirstOrDefault()?.InnerHtml;
			var bodyText = doc.DocumentNode.SelectNodes("//mung-body-text").FirstOrDefault()?.InnerText;

			var toList = to.Split(';')?.Where(x => !string.IsNullOrEmpty(x)).ToList();
			var ccList = cc.Split(';')?.Where(x => !string.IsNullOrEmpty(x)).ToList();
			var bccList = bcc.Split(';')?.Where(x => !string.IsNullOrEmpty(x)).ToList();

			if (toList.Count + ccList.Count + bccList.Count == 0) {
				throw new Exception("No recipients defined, please ensure your template contains <mung-to />, <mung-cc /> or <mung-bcc /> tags.");
			}

			var destination = new Destination() {
				ToAddresses = toList,
				BccAddresses = bccList,
				CcAddresses = ccList
			};


			var subject = new Content(subjectText);
			var body = new Body() {
				Html = new Content(bodyHtml),
				Text = new Content(bodyText)
			};

			var message = new Message(subject, body);


			var request = new SendEmailRequest(from, destination, message);

			return request;
		}

		/// <summary>
		/// Sends the appropriate notification using the data from the event
		/// </summary>
		/// <param name="notification"></param>
		/// <param name="evt"></param>
		/// <returns></returns>
		public static bool SendNotification(Notification notification, MungServerEvent evt) {
			var content = RenderTemplate(notification, evt);

			if (string.IsNullOrEmpty(content)) {
				MungApp.Current.ProcessInternalEvent("notification_ignored", new {
					Notification = notification,
					NotificationId = notification.NotificationId,
					Event = evt
				});
				return false;
			}
			


			var emailRequest = CreateEmailRequest(content, SesNotificationSettings.SesFromAddress);

			var region = Amazon.RegionEndpoint.USEast1;
			var credentials = SesNotificationSettings.SesCredentials;

			var client = new AmazonSimpleEmailServiceClient(credentials, region);
			try {
				Console.WriteLine("Attempting to send an email through Amazon SES by using the AWS SDK for .NET...");
				var sendResult = client.SendEmailAsync(emailRequest).Result;

				MungApp.Current.ProcessInternalEvent("notification_sent", new {
					Notification = notification,
					NotificationId = notification.NotificationId,
					Event = evt,
					Email = content
				});
				return true;
			} catch (Exception ex) {
				var agg = ex as AggregateException;
				if (agg != null && agg.InnerExceptions.Count > 0) {
					ex = agg.InnerExceptions.FirstOrDefault();
				}

				MungApp.Current.ProcessInternalEvent("notification_send_exception", new {
					Notification = notification,
					NotificationId = notification.NotificationId,
					Event = evt,
					Email = content,
					ExceptionMessage = ex.Message,
					ExceptionStack = ex.StackTrace
				});
				return false;
			}

		}

	}
}

