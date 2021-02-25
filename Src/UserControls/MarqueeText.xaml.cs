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
using System.Windows.Data;
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
        #region Dependency Proporties

        public static readonly DependencyProperty AlwaysAnimProperty = DependencyProperty.Register(
            nameof(AlwaysAnim),
            typeof(bool),
            typeof(MarqueeText),
            new PropertyMetadata(false)
        );

        public static readonly DependencyProperty AnimSpeedProperty = DependencyProperty.Register(
            nameof(AnimSpeed),
            typeof(double),
            typeof(MarqueeText),
            new PropertyMetadata(1.0)
        );

        public static readonly DependencyProperty AnimTypeProperty = DependencyProperty.Register(
            nameof(AnimType),
            typeof(MarqueeAnimationType),
            typeof(MarqueeText),
            new PropertyMetadata(MarqueeAnimationType.None)
        );

        public static readonly DependencyProperty LoopDelayOnceProperty = DependencyProperty.Register(
            nameof(LoopDelayOnce),
            typeof(bool),
            typeof(MarqueeText),
            new PropertyMetadata(false)
        );

        public static readonly DependencyProperty LoopDirectionProperty = DependencyProperty.Register(
            nameof(LoopDirection),
            typeof(MarqueeLoopDirection),
            typeof(MarqueeText),
            new PropertyMetadata(MarqueeLoopDirection.Left)
        );

        public static readonly DependencyProperty SpacesBetweenProperty = DependencyProperty.Register(
            nameof(SpacesBetween),
            typeof(uint),
            typeof(MarqueeText),
            new PropertyMetadata(default(uint))
        );

        public static readonly DependencyProperty StartDelayProperty = DependencyProperty.Register(
            nameof(StartDelay),
            typeof(uint),
            typeof(MarqueeText),
            new PropertyMetadata(default(uint))
        );

        public static readonly DependencyProperty TextAlignProperty = DependencyProperty.Register(
            nameof(TextAlign),
            typeof(MarqueeTextAlignment),
            typeof(MarqueeText),
            new PropertyMetadata(MarqueeTextAlignment.Left)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(MarqueeText),
            new PropertyMetadata(default(string))
        );

        private static readonly DependencyProperty TextInternalProperty = DependencyProperty.Register(
            nameof(TextInternal),
            typeof(string),
            typeof(MarqueeText),
            new PropertyMetadata(default(string))
        );

        #endregion

        #region Public Proporties

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool AlwaysAnim
        {
            get { return (bool)GetValue(AlwaysAnimProperty); }
            set { SetValue(AlwaysAnimProperty, value); }
        }

        public double AnimSpeed
        {
            get { return (double)GetValue(AnimSpeedProperty); }
            set { SetValue(AnimSpeedProperty, value); }
        }

        public MarqueeAnimationType AnimType
        {
            get { return (MarqueeAnimationType)GetValue(AnimTypeProperty); }
            set { SetValue(AnimTypeProperty, value); }
        }

        public bool LoopDelayOnce
        {
            get { return (bool)GetValue(LoopDelayOnceProperty); }
            set { SetValue(LoopDelayOnceProperty, value); }
        }

        public MarqueeLoopDirection LoopDirection
        {
            get { return (MarqueeLoopDirection)GetValue(LoopDirectionProperty); }
            set { SetValue(LoopDirectionProperty, value); }
        }

        public uint SpacesBetween
        {
            get { return (uint)GetValue(SpacesBetweenProperty); }
            set { SetValue(SpacesBetweenProperty, value); }
        }

        public uint StartDelay
        {
            get { return (uint)GetValue(StartDelayProperty); }
            set { SetValue(StartDelayProperty, value); }
        }

        public MarqueeTextAlignment TextAlign
        {
            get { return (MarqueeTextAlignment)GetValue(TextAlignProperty); }
            set { SetValue(TextAlignProperty, value); }
        }
        
        #endregion

        #region Private Proporties

        private string TextInternal
        {
            get { return (string)GetValue(TextInternalProperty); }
            set { SetValue(TextInternalProperty, value); }
        }
        private int    mAnimCount    { get; set; } = 0;
        private int    mAnimStep     { get; set; } = 0;
        
        #endregion

        #region Constructor

        public MarqueeText()
        {
            InitializeComponent();
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

        public void Reset()
        {            
            // Stop current animation.
            StopAnimation();
            
            ResetTextBlocks();
            ResetText();
            ResetTextTransform();

            RemoveHelperBlocks();

            if (AnimType == MarqueeAnimationType.Loop || AnimType == MarqueeAnimationType.Bounce)
            {
                CreateHelperBlocks();

                // Restart Animation only if AlwaysAnim or text doesn't fit the area.
                if (AlwaysAnim || !CheckIfTextFit())
                {
                    ResetAnimation();
                }
            }
        }

        public void Update()
        {
            ResetText();
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
                    TextInternal = Text + new string(' ', (int)SpacesBetween);
                }
                else
                {
                    TextInternal = new string(' ', (int)SpacesBetween) + Text;
                }

                uiTextBlock.UpdateLayout(); // Make sure ActualWidth gets updated.

                // Fix if text is on right and it's too big.
                // We check this after we set the text to new value.
                if (TextAlign == MarqueeTextAlignment.Right && !CheckIfTextFit())
                {
                    TextInternal = Text + new string(' ', (int)SpacesBetween);
                }
            }
            else
            {
                TextInternal = Text;
            }

            uiTextBlock.UpdateLayout(); // Make sure ActualWidth gets updated.
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

        private void ResetTextBlocks()
        {
            if (AnimType == MarqueeAnimationType.None)
            {
                uiCanvas.Visibility = Visibility.Collapsed;
                uiTextBlockFallback.Visibility = Visibility.Visible;
            }
            else
            {
                uiTextBlockFallback.Visibility = Visibility.Collapsed;
                uiCanvas.Visibility = Visibility.Visible;
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
            if (delay > 0)
            {
                storyboard.Seek(this, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                storyboard.Pause(this);
                await Task.Delay(delay);
                storyboard.Resume(this);
            }
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
                    var diff = uiGrid.ActualWidth - uiTextBlockFallback.ActualWidth;
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
                if (SpacesBetween == 0 && visibleWidth > 0.0)
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
            tb.SetBinding(TextBlock.BackgroundProperty , new Binding{ Path = new PropertyPath("Background"  ), ElementName = "self" });
            tb.SetBinding(TextBlock.FontFamilyProperty , new Binding{ Path = new PropertyPath("FontFamily"  ), ElementName = "self" });
            tb.SetBinding(TextBlock.FontSizeProperty   , new Binding{ Path = new PropertyPath("FontSize"    ), ElementName = "self" });
            tb.SetBinding(TextBlock.FontStretchProperty, new Binding{ Path = new PropertyPath("FontStretch" ), ElementName = "self" });
            tb.SetBinding(TextBlock.FontStyleProperty  , new Binding{ Path = new PropertyPath("FontStyle"   ), ElementName = "self" });
            tb.SetBinding(TextBlock.FontWeightProperty , new Binding{ Path = new PropertyPath("FontWeight"  ), ElementName = "self" });
            tb.SetBinding(TextBlock.ForegroundProperty , new Binding{ Path = new PropertyPath("Foreground"  ), ElementName = "self" });
            tb.SetBinding(TextBlock.TextProperty       , new Binding{ Path = new PropertyPath("TextInternal"), ElementName = "self" });
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
                if (visibleWidth <= 0)
                {
                    return;
                }

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
                if (textWidth <= 0)
                {
                    return;
                }

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
