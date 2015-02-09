using System;
using System.Windows;
using emeging.Models;

namespace emeging
{
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(Ip.Text) ||
			    string.IsNullOrWhiteSpace(Username.Text))
			{
				MessageBox.Show("All fields must be entered.", "Error");
				return;
			}

			var server = new ChatServer();

			try
			{
				await server.ConnectAsync(Ip.Text, 2015);
			}
			catch (System.Net.Sockets.SocketException)
			{
				MessageBox.Show(string.Format("A server could not be reached at {0}.", Ip.Text), "Error");
				server.Dispose();
				return;
			}

			Inforesp inforesp = null;

			server.Connected += () => Dispatcher.Invoke(() =>
			{
				var chat = new Chat(server, inforesp);
				chat.Show();
				Hide();
			});

			server.InvalidUsername += username => Dispatcher.Invoke(() =>
			{
				Username.Focus();

				MessageBox.Show(
					string.Format("'{0}' cannot be used as a username. Either somebody else is logged on with the same username, the username has invalid characters, or the server has decided that this username cannot be used.", Username.Text),
					"Error");

				server.Dispose();
			});

			server.InfoReceived += async info =>
			{
				inforesp = info;
				if (!info.SSLENABLED)
				{
					await Dispatcher.InvokeAsync(async () => await server.RequestConnect(Username.Text));
				}
				else
				{
					Ip.Focus();
					MessageBox.Show("This server requires an SSL connection, but emeging currently does not support SSL.", "Error");
					Dispatcher.Invoke(server.Dispose);
				}
			};

			await server.RequestServerInfoAsync();
		}
	}
}
