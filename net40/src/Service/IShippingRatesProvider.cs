
using System.ServiceModel;

namespace Service
{
	[ServiceContract]
	public interface IShippingRatesProvider
	{
		[OperationContract]
		ShippingRate[] GetShippingRates(decimal weight, string originZipCode, string destinationZipCode);
	}
}
