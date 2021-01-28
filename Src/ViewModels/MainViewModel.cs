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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace ShowPlay
{
    public class MainViewModel
    {
        #region Private Proporties

        private List<int> mClients      { get; set; } = new List<int>();
        private int?      mActiveClient { get; set; } = null;
        private Server    mServer       { get; set; } = null;

        private string mTextRow1 { get; set; } = "";
        private string mTextRow2 { get; set; } = "";

        #endregion

        #region Public Proporties

        public SongInfo     CurrentSong  { get; set; } = null;
        public PlayerInfo   ActivePlayer { get; set; } = null;
        public PlaybackInfo Playback     { get; set; } = new PlaybackInfo();
        public CoverInfo    AlbumCover   { get; set; } = null;

        public string FormatStringRow1 { get; set; } = "";
        public string FormatStringRow2 { get; set; } = "";

        public MarqueeTextViewModel Row1 { get; set; }
        public MarqueeTextViewModel Row2 { get; set; }

        #endregion

        #region Events

        public event EventHandler ResetAnimRow1 = null;
        public event EventHandler ResetAnimRow2 = null;

        #endregion

        #region Commands

        public DelegateCommand RestartServerCommand { get; set; } = null;

        #endregion

        #region Constructor

        public MainViewModel()
        {
            Row1 = new MarqueeTextViewModel();
            Row1.ResetAnim += OnResetAnimRow1;

            Row2 = new MarqueeTextViewModel();
            Row2.ResetAnim += OnResetAnimRow2;

            // Start server.
            var ip = new System.Net.IPAddress(new byte[4]{127, 0, 0, 1});
            mServer = new Server(ip, 8585, false);

            mServer.ConnectionAccepted += Server_ConnectionAccepted;
            mServer.ConnectionClosed += Server_ConnectionClosed;
            mServer.DataReceived += Server_DataReceived;

            mServer.Start();

            RestartServerCommand = new DelegateCommand((s) => mServer.Restart());
        }

        #endregion

        #region Reset Animations on UI

        void OnResetAnimRow1(object sender, EventArgs e)
        {
            ResetAnimRow1?.Invoke(this, e);
        }

        void OnResetAnimRow2(object sender, EventArgs e)
        {
            ResetAnimRow2?.Invoke(this, e);
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

            var jsonStr = Encoding.UTF8.GetString(args.Data).TrimEnd('\0');
            ParsePayload(jsonStr);
        }

        #endregion

        #region Parse Payload

        private void ParsePayload(string jsonStr)
        {
            Log.Info("Parsing payload");
            Log.Debug("{0}", jsonStr);

            // Deserialize json.
            var root = (Payload)null;
            try
            {
                root = (Payload)JsonSerializer.Deserialize(jsonStr, typeof(Payload));
            }
            catch (Exception e)
            {
                Log.Error("Failed to deserialize json: {0}", e);
                return;
            }
            finally
            {
                Log.Success("Succesfully deserialized json");
            }

            // Update UI.
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                if (root.Player is not null)
                    UpdatePlayer(root.Player);

                if (root.Song is not null)
                    UpdateSong(root.Song);

                if (root.Playback is not null)
                    UpdatePlayback(root.Playback);

                if (root.Cover is not null)
                    UpdateCover(root.Cover);
            }));
        }

        private void UpdatePlayer(PlayerInfo player)
        {

        }

        private void UpdateSong(SongInfo song)
        {
            Row1.Text = song.Title;
            Row2.Text = song.Album;
        }

        private void UpdatePlayback(PlaybackInfo palyback)
        {

        }

        private void UpdateCover(CoverInfo cover)
        {

        }

        #endregion

        #region Formater

        private string Format(SongInfo song, PlaybackInfo playback, PlayerInfo player)
        {
            return "";
        }

        #endregion
    }
}
