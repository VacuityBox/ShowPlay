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

        private LoggerWindow mLoggerWindow     { get; set; } = new LoggerWindow();
        private string       mSettingsFileName { get; set; } = "ShowPlay.json";

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

            // Load Settings.
            var settings = LoadSettings();

            // Bind ViewModel.
            var viewModel = new MainViewModel(settings);
            viewModel.ResetAnimRow1 += ResetAnimRow1;
            viewModel.ResetAnimRow2 += ResetAnimRow2;
            DataContext = viewModel;
            uiTextRow1.DataContext = viewModel.Row1;
            uiTextRow2.DataContext = viewModel.Row2;
        }

        #endregion

        #region UI Events

        private void uiOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.DataContext = this.DataContext;
            if (settingsWindow.ShowDialog() ?? false)
            {
                SaveSettings();
            }
        }

        private void uiShowLog_Click(object sender, RoutedEventArgs e)
        {
            if (mLoggerWindow.Visibility == Visibility.Hidden)
                mLoggerWindow.Show();
        }

        private void uiMenuExit_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
            Application.Current.Shutdown();
        }

        #endregion

        #region Events

        private void ResetAnimRow1(object sender, EventArgs e)
        {
            if ((e as RefreshEventArgs).ResetAnim)
            {
                uiTextRow1.Reset();
            }
            else
            {
                uiTextRow1.Update();
            }
        }

        private void ResetAnimRow2(object sender, EventArgs e)
        {
            if ((e as RefreshEventArgs).ResetAnim)
            {
                uiTextRow2.Reset();
            }
            else
            {
                uiTextRow2.Update();
            }
        }

        #endregion

        #region Settings Loading/Saving

        private Settings LoadSettings()
        {
            var settings = new Settings();
            try
            {
                settings = SettingsReader.Load(mSettingsFileName);
            }
            catch
            {
                MessageBox.Show(
                    string.Format("Failed to load settings file '{0}'.\nUsing default values.", mSettingsFileName),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            return settings;
        }

        private void SaveSettings()
        {
            var settings = new Settings();
            try
            {
                settings.Row1 = (this.DataContext as MainViewModel).Row1;
                settings.Row2 = (this.DataContext as MainViewModel).Row2;
                
                settings.WindowPosX   = this.Left;
                settings.WindowPosY   = this.Top;
                settings.WindowWidth  = this.Width;
                settings.WindowHeight = this.Height;
                settings.WindowTitle  = this.Title;

                SettingsWriter.Save(mSettingsFileName, settings);
            }
            catch
            {
                MessageBox.Show(
                    string.Format("Failed to save settings file '{0}'.", mSettingsFileName),
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        #endregion
    }
}
