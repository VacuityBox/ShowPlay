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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

    public enum MarqueeLoopDirection
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

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(MarqueeText),
            new PropertyMetadata(default(string))
        );

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public bool                 AlwaysAnim    { get; set; } = false;
        public double               AnimSpeed     { get; set; } = 1.0;
        public MarqueeAnimationType AnimType      { get; set; } = MarqueeAnimationType.None;
        public bool                 LoopDelayOnce { get; set; } = false;
        public MarqueeLoopDirection LoopDirection { get; set; } = MarqueeLoopDirection.Left;
        public uint                 SpacesBetween { get; set; } = 0;
        public uint                 StartDelay    { get; set; } = 100;
        public MarqueeTextAlignment TextAlign     { get; set; } = MarqueeTextAlignment.Left;
        
        #endregion

        #region Private Proporties

        private int    mAnimCount    { get; set; } = 0;
        private int    mAnimStep     { get; set; } = 0;
        private string mText         { get; set; } = "MarqueeText";
        
        #endregion

        #region Constructor

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

        #region Public Methods

        /// <summary>
        /// Use this to update text.
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            mText = text;
        }

        public void Reset()
        {            
            // Stop current animation.
            StopAnimation();
            
            ResetText();
            //ResetTextTransform();

            // Update helper blocks.
            RemoveHelperBlocks();
            CreateHelperBlocks();

            // Restart Animation only if AlwaysAnim or text doesn't fit the area.
            if (AlwaysAnim || !CheckIfTextFit())
            {
                ResetAnimation();
            }
        }

        #endregion

        #region Reset

        private void ResetText()
        {
            // Add spaces between for loop animation only.
            if (AnimType == MarqueeAnimationType.Loop && SpacesBetween > 0)
            {
                if (TextAlign == MarqueeTextAlignment.Left)
                {
                    Text = mText + new string(' ', (int)SpacesBetween);
                }
                else
                {
                    Text = new string(' ', (int)SpacesBetween) + mText;
                }
                
                uiTextBlock.UpdateLayout(); // Make sure ActualWidth gets updated.

                // Fix if text is on right and it's too big.
                // We need to check after the update.
                if (TextAlign == MarqueeTextAlignment.Right && !CheckIfTextFit())
                {
                    Text = mText + new string(' ', (int)SpacesBetween);
                    uiTextBlock.UpdateLayout();
                }
            }
            else
            {
                Text = mText;
                uiTextBlock.UpdateLayout(); // Make sure ActualWidth gets updated.
            }
        }

        private void ResetTextTransform()
        {
            var (start, _) = CalculateStartEnd();
            transformText.X = start;
        }

        private void ResetAnimation()
        {
            // Setup specific animation and start.
            mAnimStep = 0;
            mAnimCount = 0;
            
            switch (AnimType)
            {
                case MarqueeAnimationType.None:
                    break;

                case MarqueeAnimationType.Loop:
                case MarqueeAnimationType.Bounce:
                    StartAnimation();
                    break;
            }
        }

        #endregion

        #region Text Animation

        private async void StartAnimation()
        {
            // Get storyboard.
            var storyboard = uiStack.Resources["storyboard"] as Storyboard;
            var moveTextAnim = storyboard.Children[0] as DoubleAnimation;

            // Calculate Start/End.
            var (start, end) = CalculateStartEnd(mAnimStep);
            moveTextAnim.From = start;
            moveTextAnim.To = end;

            // Calculate Duration.
            moveTextAnim.Duration = CalculateDuration(start, end);

            // Calculate Delay.
            var delay = CalculateDelay(mAnimCount);

            // Setup storyboard.
            storyboard.AutoReverse = false;
            storyboard.BeginTime = new TimeSpan(0, 0, 0, 0, 0);
            storyboard.RepeatBehavior = new RepeatBehavior(1.0);
            storyboard.SpeedRatio = AnimSpeed;

            // Start the animation.
            storyboard.Begin(this, true);

            // Seek anim to begining to make text appear at start position,
            // and then manually pause/delay/resume.
            storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            storyboard.Pause(this);
            await Task.Delay(delay);
            storyboard.Resume(this);
        }

        private void StopAnimation()
        {
            var storyboard = uiStack.Resources["storyboard"] as Storyboard;
            //storyboard.SeekAlignedToLastTick(this, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            storyboard.Stop(this);
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            mAnimStep += 1;
            if (mAnimStep > 1)
                mAnimStep = 0;

            mAnimCount += 1;

            switch (AnimType)
            {
                case MarqueeAnimationType.None:
                    break;
                case MarqueeAnimationType.Loop:
                case MarqueeAnimationType.Bounce:
                    StartAnimation();
                    break;
            }
        }

        #endregion

        #region Helper Calculation Methods

        private Tuple<double, double> CalculateStartEnd(int animStep = 0)
        {
            switch (AnimType)
            {
                case MarqueeAnimationType.None:
                    var diff = uiGrid.ActualWidth - uiTextBlock.ActualWidth;
                    if (TextAlign == MarqueeTextAlignment.Left || diff < 0.0)
                        return new Tuple<double, double>(0.0, 0.0);
                    else
                        return new Tuple<double, double>(diff, diff);

                case MarqueeAnimationType.Loop:
                    return CalculateStartEndForLoopAnim();

                case MarqueeAnimationType.Bounce:
                    return CalculateStartEndForBounceAnim(animStep);
            }

            return new Tuple<double, double>(0.0, 0.0);
        }

        private Tuple<double, double> CalculateStartEndForLoopAnim()
        {
            // V = visibleWidth
            // W = textWidth
            //
            // SpacesBetween == 0            | SpacesBetween > 0
            // ===================================================
            // |TEXT    | <-    [  0,    -V] | |TEXT    | <-    [  0,    -W]
            // |TEXT    | ->    [  0,     V] | |TEXT    | ->    [  0,     W]
            // |    TEXT| <-    [V-W, V-W-V] | |    TEXT| <-    [V-W, V-W-W]
            // |    TEXT| ->    [V-W, V-W+V] | |    TEXT| ->    [V-W, V-W+W]
            //
            // |TEXTTEXT|EXT <- [  0,   -cV] | |TEXTTEXT|EXT <- [  0,    -W]
            // |TEXTTEXT|EXT -> [  0,    cV] | |TEXTTEXT|EXT -> [  0,     W]
            // TEX|TEXTTEXT| <- [  0,   -cV] | TEX|TEXTTEXT| <- [  0,    -W] // fallback
            // TEX|TEXTTEXT| -> [  0,    cV] | TEX|TEXTTEXT| -> [  0,     W] // fallback

            var visibleWidth = uiGrid.ActualWidth;
            var textWidth    = uiTextBlock.ActualWidth;
            var textTooBig   = textWidth > visibleWidth;

            var D = visibleWidth - textWidth;
            var S = SpacesBetween == 0 ? visibleWidth : textWidth;
            
            var LL = (TextAlign == MarqueeTextAlignment.Left ) && (LoopDirection == MarqueeLoopDirection.Left );
            var LR = (TextAlign == MarqueeTextAlignment.Left ) && (LoopDirection == MarqueeLoopDirection.Right);
            var RL = (TextAlign == MarqueeTextAlignment.Right) && (LoopDirection == MarqueeLoopDirection.Left );
            var RR = (TextAlign == MarqueeTextAlignment.Right) && (LoopDirection == MarqueeLoopDirection.Right);

            if (!textTooBig)
            {
                if (LL) { return new Tuple<double, double>(0.0,  -S); }
                if (LR) { return new Tuple<double, double>(0.0,   S); }
                if (RL) { return new Tuple<double, double>(  D, D-S); }
                if (RR) { return new Tuple<double, double>(  D, D+S); }
            }
            else
            {
                // Calculate number of visible block that text cover.
                var C = 1;
                if (SpacesBetween == 0)
                {
                    var x = textWidth;
                    while (x > visibleWidth)
                    {
                        C += 1;
                        x -= visibleWidth;
                    }
                }

                if (LL || LR) { return new Tuple<double, double>(0.0, -C*S); }
                if (RL || RR) { return new Tuple<double, double>(0.0,  C*S); }
            }

            return new Tuple<double, double>(0.0, 0.0);
        }

        private Tuple<double, double> CalculateStartEndForBounceAnim(int animStep)
        {
            // V = visibleWidth
            // W = textWidth
            //
            // animStep == 0               | animStep == 1
            // ===================================================
            // |TEXT    |       [  0, V-W] | |    TEXT|       [V-W,   0]
            // |    TEXT|       [V-W,   0] | |TEXT    |       [  0, V-W]
            //
            // |TEXTTEXT|EXT    [  0, V-W] | TEX|TEXTTEXT|    [V-W,   0]
            // TEX|TEXTTEXT|    [  0, V-W] | |TEXTTEXT|EXT    [V-W,   0] // fallback

            var visibleWidth = uiGrid.ActualWidth;
            var textWidth    = uiTextBlock.ActualWidth;
            var textTooBig   = textWidth > visibleWidth;

            var start = 0.0;
            var end = 0.0;

            if (textTooBig || this.TextAlign == MarqueeTextAlignment.Left)
                end = visibleWidth - textWidth;
            else
                start = visibleWidth - textWidth;

            return animStep == 0
                ? new Tuple<double, double>(start, end)
                : new Tuple<double, double>(end ,start)
                ;
        }

        private Duration CalculateDuration(double start, double end)
        {
            const double PIXELS_PER_SECOND = 30.0;
            return TimeSpan.FromSeconds(Math.Abs(start - end) / PIXELS_PER_SECOND);
        }

        private int CalculateDelay(int animCount)
        {
            return animCount > 0 && LoopDelayOnce
                ? 0
                : (int)StartDelay
                ;
        }

        #endregion

        #region Helpers

        private bool CheckIfTextFit()
        {
            return uiTextBlock.ActualWidth <= uiGrid.ActualWidth;
        }

        private TextBlock CreateHelperTextBlock(string name)
        {
            // Bindings should match those for uiTextBlock in xaml.
            var tb = new TextBlock();
            tb.Name = name;
            tb.SetBinding(TextBlock.BackgroundProperty , "Background" );
            tb.SetBinding(TextBlock.FontFamilyProperty , "FontFamily" );
            tb.SetBinding(TextBlock.FontSizeProperty   , "FontSize"   );
            tb.SetBinding(TextBlock.FontStretchProperty, "FontStretch");
            tb.SetBinding(TextBlock.FontStyleProperty  , "FontStyle"  );
            tb.SetBinding(TextBlock.FontWeightProperty , "FontWeight" );
            tb.SetBinding(TextBlock.ForegroundProperty , "Foreground" );
            tb.SetBinding(TextBlock.TextProperty       , "Text"       );
            return tb;
        }

        private void CreateHelperBlocks()
        {
            // Remove previous helper blocks if any.
            RemoveHelperBlocks();

            // Calculate how many helper blocks to create.
            var visibleWidth = uiGrid.ActualWidth;
            var textWidth    = uiTextBlock.ActualWidth;
            var count = 1;
            var off = 0.0;
            if (SpacesBetween == 0)
            {
                var c = 1;
                var x = textWidth;
                while (x > visibleWidth)
                {
                    x -= visibleWidth;
                    c += 1;
                }

                off = visibleWidth * c;
            }
            else
            {
                var x = visibleWidth - textWidth;
                while (x > 0.0)
                {
                    x -= textWidth;
                    count += 1;
                }

                off = textWidth;
            }

            // Create helper blocks.
            var pos = off;
            for (var i = 0; i < count; i += 1)
            {
                var tbl = CreateHelperTextBlock("uiTextBlockL" + i.ToString());
                uiCanvas.Children.Add(tbl);
                Canvas.SetLeft(tbl, -pos);

                var tbr = CreateHelperTextBlock("uiTextBlockR" + i.ToString());
                uiCanvas.Children.Add(tbr);
                Canvas.SetLeft(tbr, pos);

                pos += off;
            }
        }

        private void RemoveHelperBlocks()
        {
            // Don't remove the first main block!
            if (uiCanvas.Children.Count > 0)
            {
                uiCanvas.Children.RemoveRange(1, uiCanvas.Children.Count - 1);
            }
        }

        #endregion
    }
}
