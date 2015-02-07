﻿using System;
using System.Windows.Input;
using emeging.Models;

namespace emeging
{
	public partial class Chat
	{
		private readonly ChatServer _server;
		private readonly Inforesp _info;

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
			Dispatcher.Invoke(() => Messages.Text += string.Format("{0}{1}: {2}", Environment.NewLine, user, message));
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private async void SendBox_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter) return;

			await _server.SendMessage(SendBox.Text);
			SendBox.Text = "";
		}
	}
}