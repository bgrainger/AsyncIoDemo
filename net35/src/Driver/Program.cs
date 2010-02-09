
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Service;

namespace Driver
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Hosting service...");
			using (ServiceHost host = CreateHost())
			{
				Console.WriteLine("done.");

				Random random = new Random();
				const string origin = "98225";

				using (ShippingRatesClient client = new ShippingRatesClient(CreateBinding(), s_baseUri))
				{
					decimal weight = 1.5m;
					Console.WriteLine("Testing service with {0}lb package:", weight);
					foreach (ShippingRate rate in client.GetShippingRatesAsync(weight, "12345", "23456"))
						Console.WriteLine("{0} -- {1:c}", rate.Name, rate.Cost);
					Console.WriteLine();

					foreach (string destination in new[] { "90210", "12345", "10101" })
					{
						weight = random.Next(1, 160) / 16m;
						Stopwatch sw;
						int rateCount;

						Console.Write("Synchronous:  ");
						sw = Stopwatch.StartNew();
						rateCount = client.GetShippingRatesSync(weight, origin, destination).Length;
						Console.WriteLine("{0} rates in {1}", rateCount, sw.Elapsed);

						Console.Write("Asynchronous: ");
						sw = Stopwatch.StartNew();
						rateCount = client.GetShippingRatesAsync(weight, origin, destination).Length;
						Console.WriteLine("{0} rates in {1}", rateCount, sw.Elapsed);

						Console.WriteLine();
					}
				}

				Console.Write("Press Enter to exit.");
				Console.ReadLine();
			}
		}

		private static ServiceHost CreateHost()
		{
			ServiceHost host = new ServiceHost(typeof(ShippingRatesProvider), s_baseUri);
			host.AddServiceEndpoint(typeof(IShippingRatesProvider), CreateBinding(), "");
			host.Open();
			return host;
		}

		private static Binding CreateBinding()
		{
			return new BasicHttpBinding();
		}

		static readonly Uri s_baseUri = new Uri("http://localhost:8731/Design_Time_Addresses/ShippingRatesProvider/");
	}
}
