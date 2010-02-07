
using System;
using Logos.Utility.Threading;

namespace Service
{
	public class ShippingRatesProvider : IShippingRatesProvider
	{
		public ShippingRate[] GetShippingRatesSync(decimal weight, string originZipCode, string destinationZipCode)
		{
			return new[]
			{
				new ShippingRate { Name = "Carrier Pigeon", Cost = 10 },
			};
		}

		public IAsyncResult BeginGetShippingRatesAsync(decimal weight, string originZipCode, string destinationZipCode, AsyncCallback callback, object state)
		{
			AsyncResult<ShippingRate[]> asyncResult = new AsyncResult<ShippingRate[]>(callback, state);
			asyncResult.Finish(new[]
				{
					new ShippingRate { Name = "Carrier Pigeon", Cost = 10 },
				}, true);
			return asyncResult;
		}

		public ShippingRate[] EndGetShippingRatesAsync(IAsyncResult asyncResult)
		{
			return ((AsyncResult<ShippingRate[]>) asyncResult).EndInvoke();
		}
	}
}
