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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ShowPlay.UserControls
{
    #region Public Enumerations

    public enum MarqueeAnimationType
    {
        None,
        Loop,
        Bounce
    }

    public enum MarqueeTextAlignment
    {
        Left,
        Right
    }

    #endregion

    /// <summary>
    /// Interaction logic for MarqueeText.xaml
    /// </summary>
    public partial class MarqueeText : UserControl
    {
        #region Public Proporties

        public string Text
        {
            get { return mText; }
            set
            {
                mText = value;
                mTextOriginal = mText;
            }
        }
        public bool                 AlwaysAnim    { get; set; } = false;
        public double               AnimSpeed     { get; set; } = 1.0;
        public MarqueeAnimationType AnimType      { get; set; } = MarqueeAnimationType.None;
        public uint                 SpacesBetween { get; set; } = 1;
        public uint                 StartDelay    { get; set; } = 0;
        public MarqueeTextAlignment TextAlign     { get; set; } = MarqueeTextAlignment.Left;
        
        #endregion

        #region Private Proporties

        private string mText         { get; set; } = "MarqueeText";
        private string mTextOriginal { get; set; } = "MarqueeText";
        private int    mAnimStep     { get; set; } = 0;

        #endregion

        #region Constructors

        public MarqueeText()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += MarqueeText_Loaded;
            this.SizeChanged += MarqueeText_SizeChanged;
        }

        #endregion

        #region Control Events

        private void MarqueeText_Loaded(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void MarqueeText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Reset();
        }

        #endregion

        #region Reset
        
        public void Reset()
        {
            // Add spaces for loop animation only.
            if (AnimType == MarqueeAnimationType.Loop)
            {
                if (TextAlign == MarqueeTextAlignment.Left)
                    mText = mTextOriginal + new string(' ', (int)SpacesBetween);
                else
                    mText = new string(' ', (int)SpacesBetween) + mTextOriginal;
            }

            // Stop current animation.
            StopAnimation();

            // Restart Animation only if AlwaysAnim or text doesn't fit the area.
            if (AlwaysAnim || !CheckIfTextFit())
            {
                ResetAnimation();
            }
            else
            {
                // We not going to animate, restart position and hide helper text blocks.
                ResetTextTransform();
                uiTextBlockLeft.Visibility = Visibility.Hidden;
                uiTextBlockRight.Visibility = Visibility.Hidden;
            }
        }

        private bool CheckIfTextFit()
        {
            // We need to measure mTextOriginal not mText.
            var ft = new FormattedText(
                mTextOriginal,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    uiTextBlock.FontFamily,
                    uiTextBlock.FontStyle,
                    uiTextBlock.FontWeight,
                    uiTextBlock.FontStretch
                    ),
                uiTextBlock.FontSize,
                uiTextBlock.Foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
            );

            return ft.Width <= uiGrid.ActualWidth;
        }

        private void ResetTextTransform()
        {
            // Reset transform, set text position basing on aligment.
            if (TextAlign == MarqueeTextAlignment.Left)
                transformText.X = 0;
            else
                transformText.X = uiGrid.ActualWidth - uiTextBlock.ActualWidth;
        }

        private void ResetAnimation()
        {
            // Setup specific animation and start.
            mAnimStep = 0;
            var storyboard = uiStack.Resources["storyboard"] as Storyboard;
            switch (AnimType)
            {
                case MarqueeAnimationType.None:
                    break;

                case MarqueeAnimationType.Loop:
                    uiTextBlockLeft.Visibility = Visibility.Visible;
                    uiTextBlockRight.Visibility = Visibility.Visible;

                    Canvas.SetLeft(uiTextBlockLeft, -uiTextBlock.ActualWidth);
                    Canvas.SetLeft(uiTextBlockRight, uiTextBlock.ActualWidth);

                    StartLoopAnimation();
                    break;

                case MarqueeAnimationType.Bounce:
                    uiTextBlockLeft.Visibility = Visibility.Hidden;
                    uiTextBlockRight.Visibility = Visibility.Hidden;
                    
                    StartBounceAnimation();
                    break;
            }
        }

        #endregion

        #region Text Animation

        private void StartLoopAnimation()
        {
            // Get storyboard.
            var storyboard = uiStack.Resources["storyboard"] as Storyboard;
            var moveTextAnim = storyboard.Children[0] as DoubleAnimation;

            // Calculate start/end.
            var start = 0.0;
            var end = uiTextBlock.ActualWidth;

            if (TextAlign == MarqueeTextAlignment.Left)
            {
                end = -end;
            }
            else if (TextAlign == MarqueeTextAlignment.Right)
            {
                start = uiGrid.ActualWidth - uiTextBlock.ActualWidth;
                end = end - Math.Abs(start);
            }

            // Set appropriate values for From/To.
            moveTextAnim.From = start;
            moveTextAnim.To = end;

            // Calculate Duration.
            moveTextAnim.Duration = CalculateDuration(start, end);

            // Setup storyboard.
            storyboard.AutoReverse = false;
            storyboard.BeginTime = new TimeSpan(0, 0, 0, 0, (int)this.StartDelay);
            storyboard.RepeatBehavior = new RepeatBehavior(1.0);
            storyboard.SpeedRatio = AnimSpeed;

            // Start the animation.
            storyboard.Begin(this, true);
        }

        private void StartBounceAnimation()
        {
            // Get storyboard.
            var storyboard = uiStack.Resources["storyboard"] as Storyboard;
            var moveTextAnim = storyboard.Children[0] as DoubleAnimation;

            // Calculate start/end.
            var start = 0;
            var end = uiGrid.ActualWidth - uiTextBlock.ActualWidth;

            // Set appropriate values for From/To.
            // step / align    From - To
            //    0 / left  ->    0 - end
            //    1 / left  ->  end - 0
            //    0 / right ->  end - 0
            //    1 / right ->    0 - end
            var b0 = mAnimStep == 0 && this.TextAlign == MarqueeTextAlignment.Left;
            var b1 = mAnimStep == 1 && this.TextAlign == MarqueeTextAlignment.Right;
            if (b0 || b1)
            {
                moveTextAnim.From = start;
                moveTextAnim.To = end;
            }
            else
            {
                moveTextAnim.From = end;
                moveTextAnim.To = start;
            }

            // Calculate Duration.
            moveTextAnim.Duration = CalculateDuration(start, end);

            // Setup storyboard.
            storyboard.AutoReverse = false;
            storyboard.BeginTime = new TimeSpan(0, 0, 0, 0, (int)this.StartDelay);
            storyboard.RepeatBehavior = new RepeatBehavior(1.0);
            storyboard.SpeedRatio = AnimSpeed;

            // Start the animation.
            storyboard.Begin(this, true);
        }

        private void StopAnimation()
        {
            var storyboard = uiStack.Resources["storyboard"] as Storyboard;
            storyboard.SeekAlignedToLastTick(this, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            storyboard.Stop(this);
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            mAnimStep += 1;
            if (mAnimStep > 1)
                mAnimStep = 0;

            switch (AnimType)
            {
                case MarqueeAnimationType.None:
                    break;
                case MarqueeAnimationType.Loop:
                    StartLoopAnimation();
                    break;
                case MarqueeAnimationType.Bounce:
                    StartBounceAnimation();
                    break;
            }
        }

        private Duration CalculateDuration(double start, double end)
        {
            const double PIXELS_PER_SECOND = 30.0;
            return TimeSpan.FromSeconds((Math.Abs(start) + Math.Abs(end)) / PIXELS_PER_SECOND);
        }

        #endregion
    }
}
