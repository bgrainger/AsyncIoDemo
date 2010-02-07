
using System;
using System.Net;
using System.Threading;

namespace Service
{
	partial class ShippingRatesProvider
	{
		private static WebRequest CreateFedExRequest(decimal weight, string originZipCode, string destinationZipCode)
		{
			// In a real application, this would create the request that would retrieve rates from FedEx.
			Uri uri = new Uri("http://www.fedex.com/");
			return WebRequest.Create(uri);
		}

		private static ShippingRate[] GetFedExRates(WebResponse response)
		{
			// In a real application, this method would parse the response from FedEx and return actual shipping rates.
			// Because this is a demo, we simply make up some rates (and sleep to simulate network delay).

			Thread.Sleep(c_demoDelay);

			Random random = new Random();
			return new[]
			{
				new ShippingRate { Name = "FedEx Standard Overnight", Cost = random.Next(1500, 5000) / 100m },
				new ShippingRate { Name = "FedEx 2Day", Cost = random.Next(1000, 2000) / 100m },
			};
		}

		private static WebRequest CreateUpsRequest(decimal weight, string originZipCode, string destinationZipCode)
		{
			// In a real application, this would create the request that would retrieve rates from UPS.
			Uri uri = new Uri("http://www.ups.com/");
			return WebRequest.Create(uri);
		}

		private static ShippingRate[] GetUpsRates(WebResponse response)
		{
			// In a real application, this method would parse the response from UPS and return actual shipping rates.
			// Because this is a demo, we simply make up some rates (and sleep to simulate network delay).

			Thread.Sleep(c_demoDelay);

			Random random = new Random();
			return new[]
			{
				new ShippingRate { Name = "UPS Next Day Air", Cost = random.Next(2500, 5000) / 100m },
				new ShippingRate { Name = "UPS 2nd Day Air", Cost = random.Next(1500, 4000) / 100m },
				new ShippingRate { Name = "UPS 3 Day Select", Cost = random.Next(500, 2000) / 100m },
				new ShippingRate { Name = "UPS Ground", Cost = random.Next(300, 1000) / 100m },
			};
		}

		private static WebRequest CreateUspsRequest(decimal weight, string originZipCode, string destinationZipCode)
		{
			// In a real application, this would create the request that would retrieve rates from USPS.
			Uri uri = new Uri("http://www.usps.com/");
			return WebRequest.Create(uri);
		}

		private static ShippingRate[] GetUspsRates(WebResponse response)
		{
			// In a real application, this method would parse the response from USPS and return actual shipping rates.
			// Because this is a demo, we simply make up some rates (and sleep to simulate network delay).

			Thread.Sleep(c_demoDelay);

			Random random = new Random();
			return new[]
			{
				new ShippingRate { Name = "Express Mail", Cost = random.Next(1000, 2000) / 100m },
				new ShippingRate { Name = "Priority Mail", Cost = random.Next(500, 1500) / 100m },
				new ShippingRate { Name = "First-Class Mail", Cost = random.Next(200, 800) / 100m },
			};
		}

		const int c_demoDelay = 300;
	}
}
