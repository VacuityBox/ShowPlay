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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ShowPlay
{
    public class LogTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isMouseOver = (parameter as string) == "true";
            var color = LogColors.GetForegroundColor((LogType)value, isMouseOver);

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LogTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isMouseOver = (parameter as string) == "true";
            var color = LogColors.GetBackgroundColor((LogType)value, isMouseOver);

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class LogInfoCustomTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var time = (DateTime)value;
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for LoggerWindow.xaml
    /// </summary>
    public partial class LoggerWindow : Window, ILoggerBackend
    {
        #region Public Proporties

        public int LogLevel { get; set; } = (int)LogType.Info;
        public ObservableCollection<LogInfo> LogHistory { get; set; } = new ObservableCollection<LogInfo>();

        #endregion

        #region Constructor

        public LoggerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #endregion

        #region ILoggerBackend Implementation

        bool ILoggerBackend.IsInitialized()
        {
            return IsInitialized;
        }

        public void LogMessage(LogInfo info)
        {
            // Run on UI thread.
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                // Limit history to 4096 messages.
                if (LogHistory.Count > 4096)
                {
                    for (var i = 0; i < 1024; ++i)
                        LogHistory.RemoveAt(i);
                }
            
                // Add message to history.
                LogHistory.Add(info);

                // Scroll to bottom.
                if (VisualTreeHelper.GetChildrenCount(uiLogHistory) > 0)
                {
                    var border = (Border)VisualTreeHelper.GetChild(uiLogHistory, 0);
                    var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
                }
            }));
        }

        #endregion

        #region Events

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void LogHistory_Copy(object sender, RoutedEventArgs e)
        {
            var selected = uiLogHistory.SelectedItem;
            if (selected is not null)
            {
                Clipboard.SetText((selected as LogInfo).ToString());
            }
        }

        private void LogHistory_CopyAll(object sender, RoutedEventArgs e)
        {
            if (LogHistory.Count < 1)
                return;

            string output = "";
            foreach (var item in LogHistory)
            {
                output += item.ToString();
            }

            Clipboard.SetText(output);
        }

        private void LogHistory_Close(object sender, RoutedEventArgs e)
        {
            Window_Closing(sender, new CancelEventArgs());
        }

        #endregion Events
    }
}
