
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
			ApiKeys apiKeys = ApiKeys.Load();
			if (apiKeys == null)
			{
				Console.Error.WriteLine("The API keys must be stored in the registry to run this program. Register at\n" +
					"the shipping provider websites, then run the following commands:\n" +
					"\n" +
					@"reg add ""HKCU\Software\Bradley Grainger\AsyncIoDemo"" /v UspsUsername /d USER" + "\n" +
					@"reg add ""HKCU\Software\Bradley Grainger\AsyncIoDemo"" /v UspsPassword /d PWD");
				return;
			}

			Console.Write("Hosting service...");
			using (ServiceHost host = CreateHost())
			{
				Console.WriteLine("done.");

				using (ShippingRatesClient client = new ShippingRatesClient(CreateBinding(), s_baseUri))
				{
					foreach (ShippingRate rate in client.GetShippingRatesSync(1.1m, "98226", "90210"))
					{
						Console.WriteLine("Name: {0} -- Cost: {1:C}", rate.Name, rate.Cost);
					}

					foreach (ShippingRate rate in client.GetShippingRatesAsync(1.1m, "98226", "90210"))
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
