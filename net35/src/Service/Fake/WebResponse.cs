
using System;
using System.IO;

namespace Service.Fake
{
	public sealed class WebResponse : IDisposable
	{
		public Stream GetResponseStream()
		{
			return new MemoryStream();
		}

		public void Dispose()
		{
		}
	}
}
