
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using Service;

namespace TestHarness
{
	class Program
	{
		static void Main(string[] args)
		{
			TestAsyncMethods();

			TestWcfService();
		}

		private static void TestAsyncMethods()
		{
			Uri uri = new Uri("http://code.logos.com/blog/");

			// synchronous
			WebRequest request = WebRequest.Create(uri);
			using (WebResponse response = request.GetResponse())
				Console.WriteLine("Downloaded {0} on thread {1}", uri, Thread.CurrentThread.ManagedThreadId);

			// asynchronous -- callback
			request = WebRequest.Create(uri);
			IAsyncResult asyncResult = request.BeginGetResponse(ar =>
			{
				using (WebResponse response = ((WebRequest) ar.AsyncState).EndGetResponse(ar))
					Console.WriteLine("Downloaded {0} on thread {1}", uri, Thread.CurrentThread.ManagedThreadId);
			}, request);

			// asynchronous -- polling
			request = WebRequest.Create(uri);
			asyncResult = request.BeginGetResponse(null, null);
			while (!asyncResult.IsCompleted)
				Thread.Sleep(50);
			using (WebResponse response = request.EndGetResponse(asyncResult))
				Console.WriteLine("Downloaded {0} on thread {1}", uri, Thread.CurrentThread.ManagedThreadId);

			// asynchronous -- waiting
			request = WebRequest.Create(uri);
			asyncResult = request.BeginGetResponse(null, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			using (WebResponse response = request.EndGetResponse(asyncResult))
				Console.WriteLine("Downloaded {0} on thread {1}", uri, Thread.CurrentThread.ManagedThreadId);

			// asynchronous -- call EndXxx
			request = WebRequest.Create(uri);
			asyncResult = request.BeginGetResponse(null, null);
			using (WebResponse response = request.EndGetResponse(asyncResult))
				Console.WriteLine("Downloaded {0} on thread {1}", uri, Thread.CurrentThread.ManagedThreadId);
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

					foreach (string destination in new[] { "90210", "12345", "10101" })
					{
						weight = random.Next(1, 160) / 16m;
						Stopwatch sw;
						int rateCount;

						Console.Write("Synchronous:  ");
						sw = Stopwatch.StartNew();
						rateCount = client.GetShippingRatesSync(weight, origin, destination).Length;
						Console.WriteLine("{0} rates in {1}", rateCount, sw.Elapsed);

						Console.Write("Asynchronous: ");
						sw = Stopwatch.StartNew();
						rateCount = client.GetShippingRatesAsync(weight, origin, destination).Length;
						Console.WriteLine("{0} rates in {1}", rateCount, sw.Elapsed);

						Console.WriteLine();
					}
				}

				Console.Write("Press Enter to exit.");
				Console.ReadLine();
			}
		}

		private static void SampleAsyncMethods()
		{
			IAsyncResult asyncResult;

			/***** SQL Connection *****/
			// NOTE: "Async=true" setting required for asynchronous operations.
			using (SqlConnection connection = new SqlConnection(@"Async=true;Server=SERVER;Database=DATABASE;Integrated Security=true"))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand("SELECT UserId, Name, LastLogIn FROM Users WHERE Email = 'test@example.com'", connection))
				{
					asyncResult = cmd.BeginExecuteReader();
					// ... query executes asynchronously in background ...
					using (IDataReader reader = cmd.EndExecuteReader(asyncResult))
					{
						// WARNING: The DbAsyncResult object returned by BeginExecuteReader always creates a ManualResetEvent, but
						// never closes it; after calling EndExecuteReader, the AsyncWaitHandle property is still valid, so we close it explicitly.
						asyncResult.AsyncWaitHandle.Close();

						while (reader.Read())
						{
							// do stuff
						}
					}
				}

				using (SqlCommand cmd = new SqlCommand("UPDATE Users SET LastLogIn = GETUTCDATE() WHERE UserId = 1", connection))
				{
					asyncResult = cmd.BeginExecuteNonQuery();
					// ... query executes asynchronously in background ...
					int rowsAffected = cmd.EndExecuteNonQuery(asyncResult);

					// WARNING: The DbAsyncResult object returned by BeginExecuteNonQuery always creates a ManualResetEvent, but
					// never closes it; after calling EndExecuteReader, the AsyncWaitHandle property is still valid, so we close it explicitly.
					asyncResult.AsyncWaitHandle.Close();
				}
			}

			/***** File Operations *****/
			// NOTE: FileOptions.Asynchronous flag required for asynchronous operations.
			using (Stream stream = new FileStream(@"C:\Temp\test.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096,
				FileOptions.Asynchronous))
			{
				byte[] buffer = new byte[65536];
				asyncResult = stream.BeginRead(buffer, 0, buffer.Length, null, null);
				// ... disk read executes asynchronously in background ...
				int bytesRead = stream.EndRead(asyncResult);
			}

			/***** HTTP Operation *****/
			// WARNING: DNS operations are synchronous, and will block!
			WebRequest request = WebRequest.Create(new Uri(@"http://www.example.com/sample/page"));
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";

			asyncResult = request.BeginGetRequestStream(null, null);
			// ... connection to server opened in background ...
			using (Stream stream = request.EndGetRequestStream(asyncResult))
			{
				byte[] bytes = Encoding.UTF8.GetBytes("Sample request");
				stream.Write(bytes, 0, bytes.Length);
			}

			// WARNING: WebRequest will swallow any exceptions thrown from the AsyncCallback passed to BeginGetResponse.
			asyncResult = request.BeginGetResponse(null, null);
			// ... web request executes in background ...
			using (WebResponse response = request.EndGetResponse(asyncResult))
			using (Stream stream = response.GetResponseStream())
			{
				// read response from server
			}

			/***** DNS hostname resolution *****/
			// WARNING: Doesn't truly use async I/O, but simply queues the request to a ThreadPool thread.
			asyncResult = Dns.BeginGetHostEntry("www.example.com", null, null);
			// ... DNS lookup executes in background
			IPHostEntry entry = Dns.EndGetHostEntry(asyncResult);

			/***** Other: Sockets, Serial Ports, SslStream *****/
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
