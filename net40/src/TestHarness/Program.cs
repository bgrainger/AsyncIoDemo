
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Service;
#if FAKE
using WebRequest = Service.Fake.WebRequest;
using WebResponse = Service.Fake.WebResponse;
#endif

namespace TestHarness
{
	class Program
	{
		static void Main(string[] args)
		{
			TestAsyncMethods();

			TestWcfService();

            Console.Write("Press Enter to exit.");
            Console.ReadLine();
        }

		private static void TestAsyncMethods()
		{
			Uri uri = new Uri("http://code.logos.com/blog/");

			// synchronous
			WebRequest request = WebRequest.Create(uri);
			using (WebResponse response = request.GetResponse())
				Console.WriteLine("Downloaded {0} on thread {1}", uri, Thread.CurrentThread.ManagedThreadId);

			// asynchronous -- ContinueWith
			request = WebRequest.Create(uri);
			Task<WebResponse> task = Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
			task.ContinueWith(t =>
			{
				using (WebResponse response = t.Result)
					Console.WriteLine("Downloaded {0} on thread {1}", uri, Thread.CurrentThread.ManagedThreadId);
			});

			// asynchronous -- access .Result
			request = WebRequest.Create(uri);
			task = Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
			using (WebResponse response = task.Result)
				Console.WriteLine("Downloaded {0} on thread {1}", uri, Thread.CurrentThread.ManagedThreadId);
		}

		private static void TaskDemo()
		{
			Uri uri = new Uri("http://code.logos.com/blog/");
			Task<byte[]> download = DownloadWebPage(uri);

			using (FileStream stream = new FileStream(@"C:\temp\test.dat", FileMode.Create))
			using (StreamWriter writer = new StreamWriter(stream))
			{
				writer.WriteLine("The contents of {0} are:", uri.AbsoluteUri);
				writer.WriteLine();
				writer.Flush();

				byte[] data = download.Result;
				stream.Write(data, 0, data.Length);
			}
		}

		private static Task<byte[]> DownloadWebPage(Uri uri)
		{
			WebRequest request = WebRequest.Create(uri);
			Task<WebResponse> task = Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
			return task.ContinueWith<byte[]>(t =>
				{
					using (WebResponse response = t.Result)
					using (Stream stream = response.GetResponseStream())
					using (MemoryStream memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						return memoryStream.ToArray();
					}
				});
		}

		private static void TestWcfService()
		{
			Console.Write("Hosting service...");
			using (ServiceHost host = CreateHost())
			{
				Console.WriteLine("done.");

				Random random = new Random();
				const string origin = "98225";

				using (ShippingRatesClient client = new ShippingRatesClient(CreateBinding(), s_baseUri))
				{
					decimal weight = 1.5m;
					Console.WriteLine("Testing service with {0}lb package:", weight);
					foreach (ShippingRate rate in client.GetShippingRatesAsync(weight, "12345", "23456"))
						Console.WriteLine("{0} -- {1:c}", rate.Name, rate.Cost);
					Console.WriteLine();

					foreach (string destination in Enumerable.Repeat("12345", 3))
					{
						weight = random.Next(1, 160) / 16m;
						Stopwatch sw;
						int rateCount;

						Console.Write("Synchronous:   ");
						sw = Stopwatch.StartNew();
						rateCount = client.GetShippingRatesSync(weight, origin, destination).Length;
						Console.WriteLine("{0} rates in {1}", rateCount, sw.Elapsed);

						Console.Write("Sync Parallel: ");
						sw = Stopwatch.StartNew();
						rateCount = client.GetShippingRatesSyncParallel(weight, origin, destination).Length;
						Console.WriteLine("{0} rates in {1}", rateCount, sw.Elapsed);

						Console.Write("Asynchronous:  ");
						sw = Stopwatch.StartNew();
						rateCount = client.GetShippingRatesAsync(weight, origin, destination).Length;
						Console.WriteLine("{0} rates in {1}", rateCount, sw.Elapsed);

						Console.WriteLine();
					}
				}
			}
		}

		private static ServiceHost CreateHost()
		{
			ServiceHost host = new ServiceHost(typeof(ShippingRatesProvider), s_baseUri);
			host.AddServiceEndpoint(typeof(IShippingRatesProvider), CreateBinding(), "");
			host.Open();
			return host;
		}

		private static Binding CreateBinding()
		{
			return new BasicHttpBinding();
		}

		static readonly Uri s_baseUri = new Uri("http://localhost:8731/Design_Time_Addresses/ShippingRatesProvider/");
	}
}
