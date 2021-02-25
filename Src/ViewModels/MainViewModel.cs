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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Timers;
using System.Windows;

namespace ShowPlay
{
    public class RefreshEventArgs : EventArgs
    {
        public bool ResetAnim { get; }

        public RefreshEventArgs(bool resetAnim)
        {
            ResetAnim = resetAnim;
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private Proporties

        private Server    mServer       { get; set; } = null;
        private Timer     mTimer        { get; set; } = null;

        private double mWindowPosX   { get; set; } = 200;
        private double mWindowPosY   { get; set; } = 100;
        private double mWindowWidth  { get; set; } = 400;
        private double mWindowHeight { get; set; } = 100;
        private string mWindowTitle  { get; set; } = "ShowPlay";

        #endregion

        #region Public Proporties

        public SongInfo     CurrentSong     { get; set; } = new SongInfo();
        public PlayerInfo   CurrentPlayer   { get; set; } = new PlayerInfo();
        public PlaybackInfo CurrentPlayback { get; set; } = new PlaybackInfo();
        public CoverInfo    CurrentCover    { get; set; } = new CoverInfo();

        public MarqueeTextViewModel Row1 { get; set; }
        public MarqueeTextViewModel Row2 { get; set; }

        public double WindowPosX
        {
            get
            {
                return mWindowPosX;
            }
            set
            {
                mWindowPosX = value;
                NotifyPropertyChanged();
            }
        }
                
        public double WindowPosY
        {
            get
            {
                return mWindowPosY;
            }
            set
            {
                mWindowPosY = value;
                NotifyPropertyChanged();
            }
        }
                
        public double WindowWidth
        {
            get
            {
                return mWindowWidth;
            }
            set
            {
                mWindowWidth = value;
                NotifyPropertyChanged();
            }
        }
        
        public double WindowHeight
        {
            get
            {
                return mWindowHeight;
            }
            set
            {
                mWindowHeight = value;
                NotifyPropertyChanged();
            }
        }

        public string WindowTitle
        {
            get
            {
                return mWindowTitle;
            }
            set
            {
                mWindowTitle = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Events

        public event EventHandler ResetAnimRow1 = null;
        public event EventHandler ResetAnimRow2 = null;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Commands

        public DelegateCommand RestartServerCommand { get; set; } = null;

        #endregion

        #region Constructor

        public MainViewModel(Settings settings)
        {
            WindowPosX = settings.WindowPosX;
            WindowPosY = settings.WindowPosY;
            WindowWidth  = settings.WindowWidth;
            WindowHeight = settings.WindowHeight;
            WindowTitle = settings.WindowTitle;

            Row1 = new MarqueeTextViewModel();
            Row1 = settings.Row1 ?? new MarqueeTextViewModel();
            //Row1.ResetAnim += OnResetAnimRow1;
            
            Row2 = new MarqueeTextViewModel();
            Row2 = settings.Row2 ?? new MarqueeTextViewModel();
            //Row2.ResetAnim += OnResetAnimRow2;

            mTimer = new Timer(1000);
            mTimer.Elapsed += RefreshInfo;
            //mTimer.Start();

            // Reset Player Playback info.
            CurrentPlayback.State = PlaybackState.Nothing;
            CurrentPlayback.Elapsed = 0.0;
            RefreshInfo(this, new RefreshEventArgs(true));

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

        #region NotifyPropertyChanged

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Reset Animations on UI

        void OnResetAnimRow1(object sender, RefreshEventArgs e)
        {
            ResetAnimRow1?.Invoke(this, e);
        }

        void OnResetAnimRow2(object sender, RefreshEventArgs e)
        {
            ResetAnimRow2?.Invoke(this, e);
        }

        #endregion

        #region Server Events

        private void Server_ConnectionAccepted(object sender, ClientEventArgs args)
        {
            // If there is no active client, activete first client that connects.
            if (mServer.GetActiveClientId() == null)
            {
                mServer.SetActiveClientId(args.ClientId);
                Log.Info("Setting active client to #{0}", args.ClientId);
            }
        }

        private void Server_ConnectionClosed(object sender, ClientEventArgs args)
        {
            //// Check if client is in client list.
            //if (!mClients.Contains(args.ClientId))
            //{
            //    Log.Warning("Client #{0} is not in client list", args.ClientId);
            //    return;
            //}

            //// If client was active then set active client to null.
            //if (mActiveClient == args.ClientId)
            //{
            //    mActiveClient = null;
            //    Log.Info("Setting active client to null");
            //}

            //// Remove client.
            //mClients.Remove(args.ClientId);
            //Log.Success("Removed client, id #{0}", args.ClientId);

        }

        private void Server_DataReceived(object sender, ClientEventArgs args)
        {
            UpdateInfo(args.Payload);
        }

        #endregion

        #region Parse Payload

        private void UpdateInfo(Payload payload)
        {
            Log.Info("Updating information...");

            // Update UI.
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
            var resetAnim = false;

            if (payload.Player is not null)
            {
                UpdatePlayer(payload.Player);                
            }

            if (payload.Song is not null)
            {
                UpdateSong(payload.Song);
                resetAnim = true;
            }

            if (payload.Playback is not null)
            {
                UpdatePlayback(payload.Playback);
            }

            if (payload.Cover is not null)
            {
                UpdateCover(payload.Cover);
            }

            RefreshInfo(this, new RefreshEventArgs(resetAnim));
            }));
        }

        private bool ValidatePayload(Payload payload)
        {
            return true;
        }

        private void UpdatePlayer(PlayerInfo player)
        {
            CurrentPlayer = player;
        }

        private void UpdateSong(SongInfo song)
        {
            if (CurrentSong is null)
            {
                CurrentSong = new SongInfo();
            }

            CurrentSong.Title       = song.Title       ?? "";
            CurrentSong.Album       = song.Album       ?? "";
            CurrentSong.Artist      = song.Artist      ?? "";
            CurrentSong.Date        = song.Date        ?? "";
            CurrentSong.Year        = song.Year        ?? "";
            CurrentSong.TrackNumber = song.TrackNumber ?? 0;
            CurrentSong.Length      = song.Length      ?? 0.0;
            CurrentSong.Path        = song.Path        ?? "";
        }

        private void UpdatePlayback(PlaybackInfo playback)
        {
            if (playback.State is not null)
            {
                if (CurrentPlayback.State != playback.State)
                {
                    CurrentPlayback.State = playback.State;

                    if (CurrentPlayback.State == PlaybackState.Playing)
                    {
                        //mTimer.Start();
                    }
                    else
                    {
                        //mTimer.Stop();
                    }
                }
            }

            if (playback.Elapsed is not null)
            {
                CurrentPlayback.Elapsed = playback.Elapsed;
            }
        }

        private void UpdateCover(CoverInfo cover)
        {
            CurrentCover = cover;
        }

        #endregion

        #region Refresh Info

        private void RefreshInfo(object sender, EventArgs e)
        {
            if (CurrentSong is not null)
            {
                var format1 = "";
                var format2 = "";

                switch (CurrentPlayback.State)
                {
                    case PlaybackState.Nothing:
                        format1 = Row1.FormatNothing;
                        format2 = Row2.FormatNothing;
                        break;
                    case PlaybackState.Paused:
                        format1 = Row1.FormatPaused;
                        format2 = Row2.FormatPaused;
                        break;
                    case PlaybackState.Playing:
                        format1 = Row1.FormatPlaying;
                        format2 = Row2.FormatPlaying;
                        break;
                }

                Row1.Text = Format(format1, CurrentSong, CurrentPlayback, CurrentPlayer);
                Row2.Text = Format(format2, CurrentSong, CurrentPlayback, CurrentPlayer);

                ResetAnimRow1?.Invoke(this, e);
                ResetAnimRow2?.Invoke(this, e);
            }
        

            //if (PlayerPlayback.State == PlaybackState.Playing)
            //{
            //    if (PlayerPlayback.Elapsed < CurrentSong.Length)
            //    {
            //        PlayerPlayback.Elapsed += 1;
            //    }
            //    else
            //    {
            //        Log.Warning("Elapsed time exceeded song length");
            //    }
            //}
        }

        #endregion

        #region Formater

        enum FormatterState
        {
            Read,
            Format,
            Done,
        }

        private string ElapsedToTime(int elapsed)
        {
            var seconds = elapsed % 60;
            elapsed = (elapsed - seconds) / 60;

            var minutes = elapsed % 60;
            elapsed = (elapsed - minutes) / 60;
            
            var hours = elapsed;

            return (hours > 0)
                ? string.Format("{0}:{1}:{2}", hours, minutes.ToString("D2"), seconds.ToString("D2"))
                : string.Format("{0}:{1}", minutes, seconds.ToString("D2"))
                ;
        }

        private string Format(string format, SongInfo song, PlaybackInfo playback, PlayerInfo player)
        {
            var state = FormatterState.Read;
            var formatChar = '%';
            var output = "";
            
            for (var i = 0; i < format.Length; )            
            {
                var c = format[i];
                var consume = true;

                if (state == FormatterState.Read)
                {
                    if (c == formatChar)
                    {
                        state = FormatterState.Format;
                    }
                    else
                    {
                        output += c;
                    }
                }
                else if (state == FormatterState.Format)
                {
                    switch (c)
                    {
                        case 's':
                            output += song.Title ?? "";
                            break;
                        case 'A':
                            output += song.Artist ?? "";
                            break;
                        case 'a':
                            output += song.Album ?? "";
                            break;
                        case 't':
                            output += (song.Length ?? 0).ToString();
                            break;
                        case 'T':
                            output += ElapsedToTime((int)(song.Length ?? 0));
                            break;
                        case 'e':
                            output += (playback.Elapsed ?? 0).ToString();
                            break;
                        case 'E':
                            output += ElapsedToTime((int)(playback.Elapsed ?? 0));
                            break;
                        case '%':
                            output += '%';
                            break;
                        default:
                            Log.Warning("Invalid format character '{0}'", c);
                            consume = false;
                            break;
                    }       

                    state = FormatterState.Read;
                }
                else
                {
                    break;
                }

                if (consume)
                {
                    i += 1;
                }
            }

            state = FormatterState.Done;

            return output;
        }

        #endregion
    }
}
