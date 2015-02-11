using System;
using System.Collections.Generic;
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
			Messages.Text = string.Format("You have connected to {0} ({1})", info.SERVERNAME, _server.Ip);

			_server.NewMessage += ServerOnNewMessage;
			_server.AfkUser += ServerOnAfkUser;
			_server.UsersReceived += ServerOnUsersReceived;
			_server.Alert += AlertRecieved;
			_server.Shutdown += ShutdownRecieved;
		}

		private void ShutdownRecieved(string msg, string type)
		{
			Dispatcher.Invoke(() =>
			{
				AppendToChat(String.Format("Alert: {0} with code {1}", msg, type));
				var window = new MainWindow();
				window.Show();
				_server.Dispose();
				Close();
			});
		}

		private void AlertRecieved(string msg, string prio)
		{
			Dispatcher.Invoke(() =>
			{
				AppendToChat(String.Format("System: {0}", msg));
			});
		}

		private void ServerOnUsersReceived(Dictionary<string, User> info)
		{
			Dispatcher.Invoke(() =>
			{
				foreach (var user in info)
				{
					if (user.Value.AFK)
						AppendToChat(string.Format("// User: {0} (is AFK)", user.Key));
					else
						AppendToChat(string.Format("// User: {0}", user.Key));

					AppendToChat(string.Format("// Status message: {0}", user.Value.STATUS ?? "No message."));
				}
			});
		}

		private void ServerOnAfkUser(string user, bool userAfk)
		{
			if (userAfk)
				Dispatcher.Invoke(() => AppendToChat(string.Format("// {0} has gone AFK.", user)));
			else
				Dispatcher.Invoke(() => AppendToChat(string.Format("{0} has come back from AFK.", user)));
		}

		private void ServerOnNewMessage(string user, string message)
		{
			Dispatcher.Invoke(() => AppendToChat(string.Format("{0}: {1}", user, message)));
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void AppendToChat(string text)
		{
			Messages.AppendText(Environment.NewLine + text);

			var prevfocus = FocusManager.GetFocusedElement(this);
			Messages.Focus();
			Messages.CaretIndex = Messages.Text.Length;
			Messages.ScrollToEnd();

			if (prevfocus == null)
				SendBox.Focus();
			else
				prevfocus.Focus();
		}

		private async void SendBox_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter) return;

			if (SendBox.Text.StartsWith("/"))
				switch (SendBox.Text.Split(' ')[0])
				{
					case "/afk":
						await _server.SetAfk(!isAfk);
						isAfk = !isAfk;
						break;
					case "/status":
						int spaceIndex = SendBox.Text.IndexOf(' ');
						int index = SendBox.Text.Split(' ').Length;
						if (index != 2)
							break;
						await _server.SetStatus(SendBox.Text.Substring(spaceIndex, SendBox.Text.Length - spaceIndex));
						break;
					case "/users":
						await _server.RequestUsers();
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
