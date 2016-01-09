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

			var perf = new SystemPerformance();
			perf.BeginCollection(new TimeSpan(0, 0, 1));

			for (var i = 0; i < 10000; i++) {

				////Console.WriteLine("Sending event: " + i.ToString());
				//client.Send("tester", "my_first_event", new {
				//	EventCounter = 100,
				//	SomethingElse = "yes",
				//	typeedAs = "int"
				//});
				//client.Send("tester", "my_first_event", new {
				//	EventCounter = 100.09m,
				//	SomethingElse = "yes",
				//	typeedAs = "decimal"
				//});
				//client.Send("tester", "my_first_event", new {
				//	EventCounter = 100.09f,
				//	SomethingElse = "yes",
				//	typeedAs = "float"
				//});
				//client.Send("tester", "my_first_event", new {
				//	EventCounter = 100.09f,
				//	SomethingElse = "yes",
				//	typeedAs = "double"
				//});

				//Console.WriteLine("Sent!");
				//Console.ReadKey();
			}
			

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();

			Console.WriteLine("Done!");

		}
	}
}
