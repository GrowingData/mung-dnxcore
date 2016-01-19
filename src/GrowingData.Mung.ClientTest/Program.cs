using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrowingData.Mung.Client;


namespace GrowingData.Mung.ClientTest {
	class Program {


		static void Main(string[] args) {
			var client = new MungClient();

			var perf = new SystemPerformance(client);
			perf.BeginCollection(new TimeSpan(0, 10, 0));




			for (var i = 0; i < 10000; i++) {
				Console.WriteLine("Sending...");
				client.Send("password_reset", new {
					User = new {
						Username = "john",
						FirstName = "John",
						Message = "This is '\"A mult\r\nline'\"\r\nMessage",
						Email = "terence@growingdata.com.au"
					},
					PasswordResetToken = "shJwFOfIQjy6c3dkjf8id3"
				});
				Console.ReadKey();
			}

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();

			Console.WriteLine("Done!");

		}
	}
}
