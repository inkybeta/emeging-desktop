using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace emeging
{
	public class TcpConnection : IDisposable
	{
		private readonly TcpClient _client;
		private NetworkStream _stream;
		public string Ip { get; private set; }

		public TcpConnection()
		{
			_client = new TcpClient();
		}

		public async Task ConnectAsync(string ip, int port)
		{
			await _client.ConnectAsync(ip, port);
			_stream = _client.GetStream();
			Ip = ip;
		}

		public async Task<byte[]> GetDataAsync(int bufferSize)
		{
			int byteLength;

			using (var ms = new MemoryStream())
			{
				while (ms.Length != 4)
				{
					var buffer = new byte[4-ms.Length];
					var read = await _stream.ReadAsync(buffer, 0, buffer.Length);

					await ms.WriteAsync(buffer, 0, read);
				}
				byteLength = BitConverter.ToInt32(ms.ToArray(), 0);
			}

			using (var ms = new MemoryStream())
			{
				while (ms.Length != byteLength)
				{
					var buffer = new byte[bufferSize];
					var read = await _stream.ReadAsync(buffer, 0, bufferSize);

					await ms.WriteAsync(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		public async Task SendDataAsync(byte[] data)
		{
			var header = BitConverter.GetBytes(data.Length);
			await _stream.WriteAsync(header, 0, header.Length);
			await _stream.WriteAsync(data, 0, data.Length);
			await _stream.FlushAsync();
		}

		public byte[] GetData(int bufferSize)
		{
			int byteLength;

			using (var ms = new MemoryStream())
			{
				while (ms.Length != 4)
				{
					var buffer = new byte[4];
					var read = _stream.Read(buffer, 0, buffer.Length);

					ms.Write(buffer, 0, read);
				}
				byteLength = BitConverter.ToInt32(ms.ToArray(), 0);
			}

			using (var ms = new MemoryStream())
			{
				while (ms.Length != byteLength)
				{
					var buffer = new byte[bufferSize];
					var read = _stream.Read(buffer, 0, bufferSize);

					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		public void SendData(byte[] data)
		{
			var header = BitConverter.GetBytes(data.Length);
			_stream.Write(header, 0, header.Length);
			_stream.Write(data, 0, data.Length);
			_stream.Flush();
		}

		public void Dispose()
		{
			_client.Close();
		}
	}
}
