using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using emeging.Models;
using Newtonsoft.Json;

namespace emeging
{
	public class ChatServer : IDisposable
	{
		public delegate void InfoReceivedHandler(Inforesp info);
		public event InfoReceivedHandler InfoReceived;

		public delegate void ConnectedHandler();
		public event ConnectedHandler Connected;

		public delegate void InvalidUsernameHandler(string username);
		public event InvalidUsernameHandler InvalidUsername;

		public delegate void NewMessageHandler(string user, string message);
		public event NewMessageHandler NewMessage;

		public delegate void InvalidOperationHandler(string msg);
		public event InvalidOperationHandler InvalidOperation;

		public delegate void ErrorHandler(string msg);
		public event ErrorHandler Error;

		public string Ip { get { return _connection.Ip; } }

		private readonly TcpConnection _connection;

		public ChatServer()
		{
			_connection = new TcpConnection();
		}

		public async Task ConnectAsync(string ip, int port)
		{
			await _connection.ConnectAsync(ip, port);

			var thread = new Thread(ReadLoop);
			thread.Start();
		}

		private void ReadLoop()
		{
			while (true)
			{
				var data = BytesToString(_connection.GetData(80));
				var parts = ConvertMessageString(data);

				switch (parts.Command)
				{
					case "INFORESP":
						InfoReceived(JsonConvert.DeserializeObject<Inforesp>(parts.Arguments[0]));
						break;
					case "CONNECTED":
						Connected();
						break;
					case "INVALIDUN":
						InvalidUsername(parts.Arguments[0]);
						break;
					case "NEWMSG":
						NewMessage(parts.Arguments[0], parts.Arguments[1]);
						break;
					case "INVOP":
						InvalidOperation(parts.Arguments[0]);
						break;
					case "ERROR":
						Error(parts.Arguments[0]);
						break;
					default: 
						throw new NotImplementedException("The prefix for the string is not implemented.");
				}
			}
		}

		public async Task RequestServerInfoAsync()
		{
			await _connection.SendDataAsync(StringToBytes("INFOREQ"));
		}

		public async Task RequestConnect(string username)
		{
			await _connection.SendDataAsync(StringToBytes(GetMessageString("CONNECT", username, "emeging")));
		}

		public async Task SendMessage(string message)
		{
			await _connection.SendDataAsync(StringToBytes(GetMessageString("SEND", message)));
		}

		public async Task SetStatus(string message)
		{
			await _connection.SendDataAsync(StringToBytes(GetMessageString("STATUS", message)));
		}

		public async Task SetAfk(bool isAfk)
		{
			await _connection.SendDataAsync(StringToBytes(GetMessageString("AFK", isAfk ? "true" : "false")));
		}

		private static MessageData ConvertMessageString(string message)
		{
			var parts = message.Split(' ');
			if (parts.Length == 1)
				return new MessageData(parts[0]);
			if (parts.Length == 2)
			{
				return new MessageData(parts[0], parts[1].Split('&').Select(Uri.UnescapeDataString).Select(Uri.UnescapeDataString).ToArray());
			}

			throw new InvalidOperationException("The incoming message had an invalid number of arguments.");
		}

		private static string GetMessageString(string command, params string[] args)
		{
			if (args.Length == 0)
				return command;

			var sb = new StringBuilder();
			sb.Append(command + " ");

			foreach (var s in args)
				sb.Append(Uri.EscapeDataString(s) + "&");

			sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
		}

		private static byte[] StringToBytes(string text)
		{
			return Encoding.UTF8.GetBytes(text);
		}

		private static string BytesToString(byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}

		public class MessageData
		{
			public string Command { get; set; }

			public string[] Arguments { get; set; }

			public MessageData(string command, string[] arguments)
			{
				Command = command;
				Arguments = arguments;
			}

			public MessageData(string command)
			{
				Command = command;
			}
		}

		public void Dispose()
		{
			_connection.Dispose();
		}
	}
}
