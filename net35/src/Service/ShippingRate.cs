
using System.Runtime.Serialization;

namespace Service
{
	[DataContract]
	public class ShippingRate
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public decimal Cost { get; set; }
	}
}
