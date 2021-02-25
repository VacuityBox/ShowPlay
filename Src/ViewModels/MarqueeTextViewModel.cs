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
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using ShowPlay.UserControls;

namespace ShowPlay
{
    public class MarqueeTextViewModel : INotifyPropertyChanged
    {
        #region Private Proporties

        private bool                 mAlwaysAnim          { get; set; } = false;
        private double               mAnimSpeed           { get; set; } = 1.0;
        private MarqueeAnimationType mAnimType            { get; set; } = MarqueeAnimationType.None;
        private SolidColorBrush      mBackground          { get; set; } = Brushes.Transparent;
        private bool                 mEnabled             { get; set; } = true;
        private FontFamily           mFontFamily          { get; set; } = new FontFamily("SegoeUI");
        private double               mFontSize            { get; set; } = 14;
        private FontStretch          mFontStretch         { get; set; } = new FontStretch();
        private FontStyle            mFontStyle           { get; set; } = new FontStyle();
        private FontWeight           mFontWeight          { get; set; } = new FontWeight();
        private SolidColorBrush      mForeground          { get; set; } = Brushes.Black;
        private string               mFormatNothing       { get; set; } = "";
        private string               mFormatPaused        { get; set; } = "";
        private string               mFormatPlaying       { get; set; } = "";
        private HorizontalAlignment  mHorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        private bool                 mLoopDelayOnce       { get; set; } = false;
        private MarqueeLoopDirection mLoopDirection       { get; set; } = MarqueeLoopDirection.Left;
        private Thickness            mMargin              { get; set; } = new Thickness(0.0);
        private Thickness            mPadding             { get; set; } = new Thickness(0.0);
        private uint                 mSpacesBetween       { get; set; } = 0;
        private uint                 mStartDelay          { get; set; } = 100;
        private string               mText                { get; set; } = "MarqueeText";
        private MarqueeTextAlignment mTextAlign           { get; set; } = MarqueeTextAlignment.Left;
        private VerticalAlignment    mVerticalAlignment   { get; set; } = VerticalAlignment.Center;

        #endregion

        #region Public Proporties

        public event PropertyChangedEventHandler PropertyChanged = null;
        public event EventHandler                ResetAnim       = null;
  
        public bool AlwaysAnim
        {
            get
            {
                return mAlwaysAnim;
            }
            set
            {
                mAlwaysAnim = value;
                NotifyPropertyChanged();
            }
        }

        public double AnimSpeed
        {
            get
            {
                return mAnimSpeed;
            }
            set
            {
                mAnimSpeed = value;
                NotifyPropertyChanged();
            }
        }

        public MarqueeAnimationType AnimType
        {
            get
            {
                return mAnimType;
            }
            set
            {
                mAnimType = value;
                NotifyPropertyChanged();
            }
        }

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
        
        public bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
                NotifyPropertyChanged(false);
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

        public string FormatNothing
        {
            get
            {
                return mFormatNothing;
            }
            set
            {
                mFormatNothing = value;
                NotifyPropertyChanged(false);
            }
        }

        public string FormatPaused
        {
            get
            {
                return mFormatPaused;
            }
            set
            {
                mFormatPaused = value;
                NotifyPropertyChanged(false);
            }
        }

        public string FormatPlaying
        {
            get
            {
                return mFormatPlaying;
            }
            set
            {
                mFormatPlaying = value;
                NotifyPropertyChanged(false);
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return mHorizontalAlignment;
            }
            set
            {
                mHorizontalAlignment = value;
                NotifyPropertyChanged();
            }
        }

        public bool LoopDelayOnce
        {
            get
            {
                return mLoopDelayOnce;
            }
            set
            {
                mLoopDelayOnce = value;
                NotifyPropertyChanged(false);
            }
        }

        public MarqueeLoopDirection LoopDirection
        {
            get
            {
                return mLoopDirection;
            }
            set
            {
                mLoopDirection = value;
                NotifyPropertyChanged();
            }
        }

        public Thickness Margin
        {
            get
            {
                return mMargin;
            }
            set
            {
                mMargin = value;
                NotifyPropertyChanged();
            }
        }

        public Thickness Padding
        {
            get
            {
                return mPadding;
            }
            set
            {
                mPadding = value;
                NotifyPropertyChanged();
            }
        }

        public uint SpacesBetween
        {
            get
            {
                return mSpacesBetween;
            }
            set
            {
                mSpacesBetween = value;
                NotifyPropertyChanged();
            }
        }
        
        public uint StartDelay
        {
            get
            {
                return mStartDelay;
            }
            set
            {
                mStartDelay = value;
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
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
        
        public MarqueeTextAlignment TextAlign
        {
            get
            {
                return mTextAlign;
            }
            set
            {
                mTextAlign = value;
                NotifyPropertyChanged();
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return mVerticalAlignment;
            }
            set
            {
                mVerticalAlignment = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
        
        #region NotifyPropertyChanged

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ResetAnim?.Invoke(this, new RefreshEventArgs(true));
        }

        private void NotifyPropertyChanged(bool reset, [CallerMemberName] string propertyName = "")  
        {  
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ResetAnim?.Invoke(this, new RefreshEventArgs(reset));
        } 

        #endregion
    }
}
