// ShowPlay - Show what song is playing
//
// Copyright (C) 2020-2021 VacuityBox
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
// SPDX-License-Identifier: GPL-3.0-only 

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace ShowPlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Proporties

        private LoggerWindow mLoggerWindow { get; set; } = new LoggerWindow();
        private List<int> mClients { get; set; } = new List<int>();
        private int? mActiveClient { get; set; } = null;
        private Server mServer { get; set; } = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            
            // Add logger backends.
            var fileBackend = new FileBackend("ShowPlay.log");
            if (Debugger.IsAttached)
            {
                mLoggerWindow.LogLevel = (int)LogType.Debug;
                fileBackend.LogLevel = (int)LogType.Debug;
                mLoggerWindow.Show();
            }
            else
            {
                mLoggerWindow.LogLevel = (int)LogType.Info;
                fileBackend.LogLevel = (int)LogType.Info;
                mLoggerWindow.Hide();
            }

            Log.LoggerInstance.AddBackend("FileLog", fileBackend);
            Log.LoggerInstance.AddBackend("WindowLog", mLoggerWindow);

            // Start server.
            var ip = new System.Net.IPAddress(new byte[4]{ 127, 0, 0, 1});
            mServer = new Server(ip, 8585, false);

            mServer.ConnectionAccepted += Server_ConnectionAccepted;
            mServer.ConnectionClosed += Server_ConnectionClosed;
            mServer.DataReceived += Server_DataReceived;

            mServer.Start();
        }

        #endregion

        #region UI Events

        private void uiShowLog_Click(object sender, RoutedEventArgs e)
        {
            if (mLoggerWindow.Visibility == Visibility.Hidden)
                mLoggerWindow.Show();
        }

        private void uiMenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void uiRestartServer_Click(object sender, RoutedEventArgs e)
        {
            mServer.Restart();
        }

        #endregion

        #region Server Events

        private void Server_ConnectionAccepted(object sender, ClientEventArgs args)
        {
            // Check if client is not already in client list.
            if (mClients.Contains(args.ClientId))
            {
                Log.Warning("Client #{0} already added to client list", args.ClientId);
                return;
            }

            // Add client.
            mClients.Add(args.ClientId);
            Log.Success("Added new client, id #{0}", args.ClientId);

            // If this is the first client, set as active.
            if (mActiveClient is null && mClients.Count == 1)
            {
                mActiveClient = args.ClientId;
                Log.Info("Setting active client to #{0}", mActiveClient);
            }
        }

        private void Server_ConnectionClosed(object sender, ClientEventArgs args)
        {
            // Check if client is in client list.
            if (!mClients.Contains(args.ClientId))
            {
                Log.Warning("Client #{0} is not in client list", args.ClientId);
                return;
            }

            // If client was active then set active client to null.
            if (mActiveClient == args.ClientId)
            {
                mActiveClient = null;
                Log.Info("Setting active client to null");
            }

            // Remove client.
            mClients.Remove(args.ClientId);
            Log.Success("Removed client, id #{0}", args.ClientId);

        }

        private void Server_DataReceived(object sender, ClientEventArgs args)
        {
            // If received data is not from current active client, ignore.
            if (mActiveClient != args.ClientId)
            {
                return;
            }

            // Deserialize.
            var jsonStr = Encoding.UTF8.GetString(args.Data).TrimEnd('\0');
            var root = JsonSerializer.Deserialize(jsonStr, typeof(Paylaod));
            Log.Debug("{0}", root.ToString());
        }

        #endregion
    }
}
