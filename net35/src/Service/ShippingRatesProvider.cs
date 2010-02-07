
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Logos.Utility.Threading;

namespace Service
{
	public class ShippingRatesProvider : IShippingRatesProvider
	{
		public ShippingRate[] GetShippingRatesSync(decimal weight, string originZipCode, string destinationZipCode)
		{
			List<ShippingRate> rates = new List<ShippingRate>();
			rates.AddRange(GetUspsShippingRates(weight, originZipCode, destinationZipCode));
			return rates.ToArray();
		}

		private static ShippingRate[] GetUspsShippingRates(decimal weight, string originZipCode, string destinationZipCode)
		{
			// convert to pounds and ounces
			decimal pounds = Math.Floor(weight);
			decimal ounces = Math.Round((weight - pounds) * 16m);

			// build XML document for USPS API
			ApiKeys apiKeys = ApiKeys.Load();
			XDocument requestDocument = new XDocument(
				new XElement("RateV3Request", new XAttribute("USERID", apiKeys.UspsUsername),
					new XElement("Package", new XAttribute("ID", "1"),
						new XElement("Service", "FIRST CLASS"),
						new XElement("FirstClassMailType", "PARCEL"),
						new XElement("ZipOrigination", originZipCode),
						new XElement("ZipDestination", destinationZipCode),
						new XElement("Pounds", pounds),
						new XElement("Ounces", ounces),
						new XElement("Size", "REGULAR"))));
			
			// save XML to string (on one line)
			StringBuilder sb = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(sb))
				requestDocument.WriteTo(writer);
			string requestXml = sb.ToString();

			// make web request; get result
			Uri uri = new Uri("http://testing.shippingapis.com/ShippingAPITest.dll?API=RateV3&XML=" + requestXml);
			WebRequest request = WebRequest.Create(uri);
			using (WebResponse response = request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (XmlReader reader = XmlReader.Create(stream))
			{
				XDocument responseDocument = XDocument.Load(reader);
				string xml = responseDocument.ToString();
				int n = xml.Length;
			}

			// results not available yet, due to testing server limitations
			return new ShippingRate[0];
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
