
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Logos.Utility.Threading;

namespace Service
{
	public class ShippingRatesProvider : IShippingRatesProvider
	{
		public ShippingRate[] GetShippingRatesSync(decimal weight, string originZipCode, string destinationZipCode)
		{
			List<ShippingRate> rates = new List<ShippingRate>();
			rates.AddRange(GetUspsShippingRates(weight, originZipCode, destinationZipCode));
			return rates.ToArray();
		}

		public IAsyncResult BeginGetShippingRatesAsync(decimal weight, string originZipCode, string destinationZipCode, AsyncCallback callback, object state)
		{
			AsyncResult<ShippingRate[]> asyncResult = new AsyncResult<ShippingRate[]>(callback, state);
			
			WebRequest request = CreateUspsRequest();
			request.BeginGetResponse(ar => UspsCallback(ar, request, asyncResult), null);

			return asyncResult;
		}

		public ShippingRate[] EndGetShippingRatesAsync(IAsyncResult asyncResult)
		{
			return ((AsyncResult<ShippingRate[]>) asyncResult).EndInvoke();
		}

		private static void UspsCallback(IAsyncResult asyncResult, WebRequest request, AsyncResult<ShippingRate[]> ratesAsyncResult)
		{
			using (WebResponse response = request.EndGetResponse(asyncResult))
				ratesAsyncResult.Finish(CreateUspsRates(), false);
		}

		private static ShippingRate[] GetUspsShippingRates(decimal weight, string originZipCode, string destinationZipCode)
		{
			using (WebResponse response = CreateUspsRequest().GetResponse())
				return CreateUspsRates();
		}

		private static WebRequest CreateUspsRequest()
		{
			// In a real application, this would create the REST request that would retrieve rates from USPS.
			Uri uri = new Uri("http://www.usps.com/");
			return WebRequest.Create(uri);
		}

		private static ShippingRate[] CreateUspsRates()
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
