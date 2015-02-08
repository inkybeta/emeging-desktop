using System;
using System.Windows.Input;
using emeging.Models;

namespace emeging
{
	public partial class Chat
	{
		private readonly ChatServer _server;
		private readonly Inforesp _info;
		private bool isAfk;

		public Chat(ChatServer server, Inforesp info)
		{
			InitializeComponent();
			_server = server;
			_info = info;
			Title = string.Format("{0} ({1} - {2}) - emeging", _info.SERVERNAME, _server.Ip, _info.SERVERVENDOR);
			
			_server.NewMessage += ServerOnNewMessage;
		}

		private void ServerOnNewMessage(string user, string message)
		{
			Dispatcher.Invoke(() => AppendToChat(string.Format("{0}{1}: {2}", Environment.NewLine, user, message)));
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void AppendToChat(string text)
		{
			Messages.AppendText(text);

			var prevfocus = FocusManager.GetFocusedElement(this);
			Messages.Focus();
			Messages.CaretIndex = Messages.Text.Length;
			Messages.ScrollToEnd();
			prevfocus.Focus();
		}

		private async void SendBox_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter) return;

			if (SendBox.Text.StartsWith("/"))
				switch (SendBox.Text.Split(' ')[0])
				{
					case "/afk":
						await _server.SetAfk(true);
						isAfk = true;
						break;
					case "/status":
						int spaceIndex = SendBox.Text.IndexOf(' ');
						await _server.SetStatus(SendBox.Text.Substring(spaceIndex, SendBox.Text.Length - spaceIndex));
						break;
				}

			else
			{
				await _server.SendMessage(SendBox.Text);
				if (isAfk)
				{
					await _server.SetAfk(false);
					isAfk = false;
				}
			}

			SendBox.Text = "";
		}
	}
}
