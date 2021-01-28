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
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace ShowPlay
{
    public class MarqueeTextViewModel : INotifyPropertyChanged
    {
        #region Private Proporties

        private SolidColorBrush mBackground  { get; set; } = Brushes.Transparent;
        private SolidColorBrush mForeground  { get; set; } = Brushes.Black;
        private FontFamily      mFontFamily  { get; set; } = new FontFamily();
        private double          mFontSize    { get; set; } = 14;
        private FontStretch     mFontStretch { get; set; } = new FontStretch();
        private FontStyle       mFontStyle   { get; set; } = new FontStyle();
        private FontWeight      mFontWeight  { get; set; } = new FontWeight();
        private string          mText        { get; set; } = "MarqueeText";

        #endregion

        #region Public Proporties

        public event PropertyChangedEventHandler PropertyChanged = null;
        public event EventHandler                ResetAnim       = null;

        public SolidColorBrush Background
        {
            get
            {
                return mBackground;
            }
            set
            {
                mBackground = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Foreground
        {
            get
            {
                return mForeground;
            }
            set
            {
                mForeground = value;
                NotifyPropertyChanged();
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                return mFontFamily;
            }
            set
            {
                mFontFamily = value;
                NotifyPropertyChanged();
            }
        }

        public double FontSize
        {
            get
            {
                return mFontSize;
            }
            set
            {
                mFontSize = value;
                NotifyPropertyChanged();
            }
        }

        public FontStretch FontStretch
        {
            get
            {
                return mFontStretch;
            }
            set
            {
                mFontStretch = value;
                NotifyPropertyChanged();
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return mFontStyle;
            }
            set
            {
                mFontStyle = value;
                NotifyPropertyChanged();
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return mFontWeight;
            }
            set
            {
                mFontWeight = value;
                NotifyPropertyChanged();
            }
        }

        public string Text
        {
            get
            {
                return mText;
            }
            set
            {
                mText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
        
        #region NotifyPropertyChanged

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ResetAnim?.Invoke(this, null);
        }  

        #endregion
    }
}
