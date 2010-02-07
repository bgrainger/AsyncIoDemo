
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Service;

namespace Driver
{
	/// <summary>
	/// <see cref="ShippingRatesClient"/> encapsulates a ChannelFactory and a Channel for the
	/// <see cref="IShippingRatesProvider"/> interface.
	/// </summary>
	class ShippingRatesClient : IShippingRatesProvider, IDisposable
	{
		public ShippingRatesClient(Binding binding, Uri uri)
		{
			m_factory = new ChannelFactory<IShippingRatesProvider>(binding, new EndpointAddress(uri));
			m_channel = m_factory.CreateChannel();
		}

		public ShippingRate[] GetShippingRates(decimal weight, string originZipCode, string destinationZipCode)
		{
			return m_channel.GetShippingRates(weight, originZipCode, destinationZipCode);
		}

		public void Dispose()
		{
			((ICommunicationObject) m_channel).Close();
			((ICommunicationObject) m_factory).Close();
		}

		ChannelFactory<IShippingRatesProvider> m_factory;
		IShippingRatesProvider m_channel;
	}
}
