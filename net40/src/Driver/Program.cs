
using System;
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

				using (ShippingRatesClient client = new ShippingRatesClient(CreateBinding(), s_baseUri))
				{
					foreach (ShippingRate rate in client.GetShippingRates(1.1m, "98226", "90210"))
					{
						Console.WriteLine("Name: {0} -- Cost: {1:C}", rate.Name, rate.Cost);
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
