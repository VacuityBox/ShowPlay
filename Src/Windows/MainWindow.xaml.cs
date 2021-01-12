﻿// ShowPlay - Show what song is playing
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

using System.ComponentModel;
using System.Diagnostics;
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

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            
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
        }

        #endregion

        #region Events

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

        #endregion
    }
}