
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Logos.Utility.Threading;

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
			// launch requests in parallel
			WebRequest fedExRequest = CreateFedExRequest(weight, originZipCode, destinationZipCode);
			Task<ShippingRate[]> fedExTask = Task.Factory.FromAsync<WebResponse>(fedExRequest.BeginGetResponse, fedExRequest.EndGetResponse, null).
				ContinueWith(t => GetRates(t, GetFedExRates));

			WebRequest upsRequest = CreateUpsRequest(weight, originZipCode, destinationZipCode);
			Task<ShippingRate[]> upsTask = Task.Factory.FromAsync<WebResponse>(upsRequest.BeginGetResponse, upsRequest.EndGetResponse, null).
				ContinueWith(t => GetRates(t, GetUpsRates));

			WebRequest uspsRequest = CreateUspsRequest(weight, originZipCode, destinationZipCode);
			Task<ShippingRate[]> uspsTask = Task.Factory.FromAsync<WebResponse>(uspsRequest.BeginGetResponse, uspsRequest.EndGetResponse, null).
				ContinueWith(t => GetRates(t, GetUspsRates));

			// combine results when all are done
			Task<ShippingRate[]> resultTask = Task.Factory.ContinueWhenAll(new[] { fedExTask, upsTask, uspsTask }, tasks => tasks.SelectMany(t => t.Result).ToArray());

			return resultTask.CreateAsyncResult(callback, state);
		}

		public ShippingRate[] EndGetShippingRatesAsync(IAsyncResult asyncResult)
		{
			return ((Task<ShippingRate[]>) asyncResult).Result;
		}

		private ShippingRate[] GetRates(Task<WebResponse> task, Func<WebResponse, ShippingRate[]> getRates)
		{
			using (task.Result)
				return getRates(task.Result);
		}

		#endregion
	}
}
