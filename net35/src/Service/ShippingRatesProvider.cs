
using System;
using System.Collections.Generic;
using System.Net;
using Logos.Utility.Threading;

namespace Service
{
	public partial class ShippingRatesProvider : IShippingRatesProvider
	{
		public ShippingRate[] GetShippingRatesSync(decimal weight, string originZipCode, string destinationZipCode)
		{
			List<ShippingRate> rates = new List<ShippingRate>();
			rates.AddRange(GetFedExRates(weight, originZipCode, destinationZipCode));
			rates.AddRange(GetUpsRates(weight, originZipCode, destinationZipCode));
			rates.AddRange(GetUspsRates(weight, originZipCode, destinationZipCode));
			return rates.ToArray();
		}

		private static ShippingRate[] GetFedExRates(decimal weight, string originZipCode, string destinationZipCode)
		{
			using (WebResponse response = CreateFedExRequest(weight, originZipCode, destinationZipCode).GetResponse())
				return GetFedExRates(response);
		}

		private static ShippingRate[] GetUpsRates(decimal weight, string originZipCode, string destinationZipCode)
		{
			using (WebResponse response = CreateUpsRequest(weight, originZipCode, destinationZipCode).GetResponse())
				return GetUpsRates(response);
		}

		private static ShippingRate[] GetUspsRates(decimal weight, string originZipCode, string destinationZipCode)
		{
			using (WebResponse response = CreateUspsRequest(weight, originZipCode, destinationZipCode).GetResponse())
				return GetUspsRates(response);
		}

		public IAsyncResult BeginGetShippingRatesAsync(decimal weight, string originZipCode, string destinationZipCode, AsyncCallback callback, object state)
		{
			AsyncResult<ShippingRate[]> asyncResult = new AsyncResult<ShippingRate[]>(callback, state);
			
			WebRequest request = CreateUspsRequest(weight, originZipCode, destinationZipCode);
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
				ratesAsyncResult.Finish(GetUspsRates(response), false);
		}
	}
}
