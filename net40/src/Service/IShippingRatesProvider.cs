
using System;
using System.ServiceModel;

namespace Service
{
	[ServiceContract]
	public interface IShippingRatesProvider
	{
		[OperationContract]
		ShippingRate[] GetShippingRatesSync(decimal weight, string originZipCode, string destinationZipCode);

		[OperationContract]
		ShippingRate[] GetShippingRatesSyncParallel(decimal weight, string originZipCode, string destinationZipCode);

		[OperationContract(AsyncPattern=true)]
		IAsyncResult BeginGetShippingRatesAsync(decimal weight, string originZipCode, string destinationZipCode, AsyncCallback callback, object state);
		ShippingRate[] EndGetShippingRatesAsync(IAsyncResult asyncResult);
	}
}
