
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Logos.Utility.Threading;
using System.Threading.Tasks;

namespace Service
{
	public partial class ShippingRatesProvider : IShippingRatesProvider
	{
		#region Synchronous Implementation

		public ShippingRate[] GetShippingRatesSync(decimal weight, string originZipCode, string destinationZipCode)
		{
			// create object that will store results
			List<ShippingRate> rates = new List<ShippingRate>();

			// launch requests serially
			using (WebResponse response = CreateFedExRequest(weight, originZipCode, destinationZipCode).GetResponse())
				rates.AddRange(GetFedExRates(response));

			using (WebResponse response = CreateUpsRequest(weight, originZipCode, destinationZipCode).GetResponse())
				rates.AddRange(GetUpsRates(response));

			using (WebResponse response = CreateUspsRequest(weight, originZipCode, destinationZipCode).GetResponse())
				rates.AddRange(GetUspsRates(response));

			return rates.ToArray();
		}

		#endregion

		#region Asynchronous Implementation

		public IAsyncResult BeginGetShippingRatesAsync(decimal weight, string originZipCode, string destinationZipCode, AsyncCallback callback, object state)
		{
			// create object that will store results
			AsyncResult<ShippingRate[]> asyncResult = new AsyncResult<ShippingRate[]>(callback, state);

			// launch requests in parallel
			WebRequest fedExRequest = CreateFedExRequest(weight, originZipCode, destinationZipCode);
			Task<ShippingRate[]> fedExTask = Task.Factory.FromAsync<WebResponse>(fedExRequest.BeginGetResponse, fedExRequest.EndGetResponse, null).ContinueWith(t => GetFedExRates(t.Result));

			WebRequest upsRequest = CreateUpsRequest(weight, originZipCode, destinationZipCode);
			Task<ShippingRate[]> upsTask = Task.Factory.FromAsync<WebResponse>(upsRequest.BeginGetResponse, upsRequest.EndGetResponse, null).ContinueWith(t => GetUpsRates(t.Result));

			WebRequest uspsRequest = CreateUspsRequest(weight, originZipCode, destinationZipCode);
			Task<ShippingRate[]> uspsTask = Task.Factory.FromAsync<WebResponse>(uspsRequest.BeginGetResponse, uspsRequest.EndGetResponse, null).ContinueWith(t => GetUspsRates(t.Result));

			// combine results when all are done
			Task.Factory.ContinueWhenAll(new[] { fedExTask, upsTask, uspsTask }, t => TaskCallback(asyncResult, t));

			// return IAsyncResult implementation to client
			return asyncResult;
		}

		public ShippingRate[] EndGetShippingRatesAsync(IAsyncResult asyncResult)
		{
			return ((AsyncResult<ShippingRate[]>) asyncResult).EndInvoke();
		}

		private static void TaskCallback(AsyncResult<ShippingRate[]> asyncResult, Task<ShippingRate[]>[] tasks)
		{
			asyncResult.Finish(tasks.SelectMany(t => t.Result).ToArray(), false);
		}

		#endregion
	}
}
