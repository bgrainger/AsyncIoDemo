
using System;
using System.Collections.Generic;
using System.Net;
using Logos.Utility.Threading;
#if FAKE
using WebRequest = Service.Fake.WebRequest;
using WebResponse = Service.Fake.WebResponse;
#endif

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
			ShippingRatesRequest request = new ShippingRatesRequest(asyncResult, 3);

			// launch requests in parallel
			WebRequest fedExRequest = CreateFedExRequest(weight, originZipCode, destinationZipCode);
			fedExRequest.BeginGetResponse(ar => FedExCallback(request, fedExRequest, ar), null);

			WebRequest upsRequest = CreateUpsRequest(weight, originZipCode, destinationZipCode);
			upsRequest.BeginGetResponse(ar => UpsCallback(request, upsRequest, ar), null);

			WebRequest uspsRequest = CreateUspsRequest(weight, originZipCode, destinationZipCode);
			uspsRequest.BeginGetResponse(ar => UspsCallback(request, uspsRequest, ar), null);

			// return IAsyncResult implementation to client
			return asyncResult;
		}

		public ShippingRate[] EndGetShippingRatesAsync(IAsyncResult asyncResult)
		{
			return ((AsyncResult<ShippingRate[]>) asyncResult).EndInvoke();
		}

		private static void FedExCallback(ShippingRatesRequest request, WebRequest webRequest, IAsyncResult asyncResult)
		{
			using (WebResponse response = webRequest.EndGetResponse(asyncResult))
				request.AddRates(GetFedExRates(response));
		}

		private static void UpsCallback(ShippingRatesRequest request, WebRequest webRequest, IAsyncResult asyncResult)
		{
			using (WebResponse response = webRequest.EndGetResponse(asyncResult))
				request.AddRates(GetUpsRates(response));
		}

		private static void UspsCallback(ShippingRatesRequest request, WebRequest webRequest, IAsyncResult asyncResult)
		{
			using (WebResponse response = webRequest.EndGetResponse(asyncResult))
				request.AddRates(GetUspsRates(response));
		}

		/// <summary>
		/// <see cref="ShippingRatesRequest"/> represents the state of one request to GetShippingRatesAsync.
		/// </summary>
		private class ShippingRatesRequest
		{
			public ShippingRatesRequest(AsyncResult<ShippingRate[]> asyncResult, int outstandingRequests)
			{
				m_lock = new object();
				m_rates = new List<ShippingRate>();
				m_asyncResult = asyncResult;
				m_outstandingRequests = outstandingRequests;
			}

			/// <summary>
			/// Adds shipping rates to this object's list of results.
			/// </summary>
			/// <param name="rates">The shipping rates to add.</param>
			/// <remarks>This method is thread-safe.</remarks>
			public void AddRates(IEnumerable<ShippingRate> rates)
			{
				lock (m_lock)
				{
					m_rates.AddRange(rates);
					if (--m_outstandingRequests == 0)
						m_asyncResult.Finish(m_rates.ToArray(), false);
				}
			}

			readonly object m_lock;
			readonly List<ShippingRate> m_rates;
			readonly AsyncResult<ShippingRate[]> m_asyncResult;
			int m_outstandingRequests;
		}

		#endregion
	}
}
