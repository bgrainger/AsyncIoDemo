
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Logos.Utility.ServiceModel;
using Service;

namespace Driver
{
	/// <summary>
	/// <see cref="ShippingRatesClient"/> encapsulates a ChannelFactory and a Channel for the
	/// <see cref="IShippingRatesProvider"/> interface.
	/// </summary>
	class ShippingRatesClient : IDisposable
	{
		public ShippingRatesClient(Binding binding, Uri uri)
		{
			m_factory = new ChannelFactory<IShippingRatesProvider>(binding, new EndpointAddress(uri));
			m_channel = m_factory.CreateChannel();
		}

		public ShippingRate[] GetShippingRatesSync(decimal weight, string originZipCode, string destinationZipCode)
		{
			VerifyNotDisposed();
			return m_channel.GetShippingRatesSync(weight, originZipCode, destinationZipCode);
		}

		public ShippingRate[] GetShippingRatesAsync(decimal weight, string originZipCode, string destinationZipCode)
		{
			VerifyNotDisposed();
			IAsyncResult ar = m_channel.BeginGetShippingRatesAsync(weight, originZipCode, destinationZipCode, null, null);
			return m_channel.EndGetShippingRatesAsync(ar);
		}

		public void Dispose()
		{
			if (m_channel != null)
			{
				((ICommunicationObject) m_channel).CloseOrAbort();
				m_channel = null;

				m_factory.CloseOrAbort();
				m_factory = null;
			}
		}

		private void VerifyNotDisposed()
		{
			if (m_channel == null)
				throw new ObjectDisposedException(GetType().Name);
		}

		ChannelFactory<IShippingRatesProvider> m_factory;
		IShippingRatesProvider m_channel;
	}
}
