
namespace Service
{
	public class ShippingRatesProvider : IShippingRatesProvider
	{
		public ShippingRate[] GetShippingRates(decimal weight, string originZipCode, string destinationZipCode)
		{
			return new[]
			{
				new ShippingRate { Name = "Carrier Pigeon", Cost = 10 },
			};
		}
	}
}
