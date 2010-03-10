
using System;
using System.Threading;
using Logos.Utility.Threading;
using System.IO;

namespace Service.Fake
{
	public sealed class WebRequest
	{
		public static WebRequest Create(Uri uri)
		{
			return new WebRequest();
		}

		public string Method { get; set; }

		public string ContentType { get; set; }

		public WebResponse GetResponse()
		{
			Random random = new Random();
			Thread.Sleep(100 + random.Next(200));
			return new WebResponse();
		}

		public IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			AsyncResult<WebResponse> asyncResult = new AsyncResult<WebResponse>(callback, state);
			ThreadPool.QueueUserWorkItem(_ => asyncResult.Finish(GetResponse(), false));
			return asyncResult;
		}

		public WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			return ((AsyncResult<WebResponse>) asyncResult).EndInvoke();
		}

		public IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			AsyncResult<Stream> asyncResult = new AsyncResult<Stream>(callback, state);
			ThreadPool.QueueUserWorkItem(_ => asyncResult.Finish(new MemoryStream(), false));
			return asyncResult;
		}

		public Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			return ((AsyncResult<Stream>) asyncResult).EndInvoke();
		}
	}
}
