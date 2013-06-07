using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Globalization;
using System.ComponentModel;
using System.Threading;

using Ragnarok;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Control;

namespace VoteSystem.Protocol.View
{
    using Model;

    /// <summary>
    /// 残りあと○○分○○秒ですと表示するコントロールです。
    /// </summary>
    [TemplatePart(Type = typeof(DecoratedText), Name = "PART_Label")]
    [TemplatePart(Type = typeof(DecoratedText), Name = "PART_Minutes")]
    [TemplatePart(Type = typeof(DecoratedText), Name = "PART_Seconds")]
    public class AnalogmaControl : UserControl
    {
        private const string ElementLabelName = "PART_Label";
        private const string ElementMinutesName = "PART_Minutes";
        private const string ElementSecondsName = "PART_Seconds";

        private DecoratedText labelText;
        private DecoratedText minutesText;
        private DecoratedText secondsText;

        /// <summary>
        /// 表示するラベルを取得または設定します。
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text", typeof(string), typeof(AnalogmaControl),
                new FrameworkPropertyMetadata(string.Empty, OnTextChanged));

        /// <summary>
        /// 表示する残り時間を取得または設定します。
        /// </summary>
        [Bindable(true)]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AnalogmaControl)d;

            self.UpdateText((string)e.NewValue);
        }

        private void UpdateText(string text)
        {
            if (this.labelText != null)
            {
                this.labelText.Text = text;
            }
        }

        /// <summary>
        /// 表示する残り時間を取得または設定します。
        /// </summary>
        public static readonly DependencyProperty LeaveTimeProperty =
            DependencyProperty.Register(
                "LeaveTime", typeof(TimeSpan), typeof(AnalogmaControl),
                new FrameworkPropertyMetadata(TimeSpan.Zero, OnLeaveTimeChanged));

        /// <summary>
        /// 表示する残り時間を取得または設定します。
        /// </summary>
        [Bindable(true)]
        public TimeSpan LeaveTime
        {
            get { return (TimeSpan)GetValue(LeaveTimeProperty); }
            set { SetValue(LeaveTimeProperty, value); }
        }

        private static void OnLeaveTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AnalogmaControl)d;

            self.UpdateLeaveTime((TimeSpan)e.NewValue);
        }

        private void UpdateLeaveTime(TimeSpan value)
        {
            if (this.minutesText != null)
            {
                this.minutesText.Text = ((int)value.TotalMinutes).ToString();
            }

            if (this.secondsText != null)
            {
                this.secondsText.Text = string.Format(
                    "{0:00}",
                    value.Seconds);
            }
        }

        /// <summary>
        /// テンプレートが適用されたときに呼ばれます。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.labelText = GetTemplateChild(ElementLabelName) as DecoratedText;
            this.minutesText = GetTemplateChild(ElementMinutesName) as DecoratedText;
            this.secondsText = GetTemplateChild(ElementSecondsName) as DecoratedText;

            UpdateText(Text);
            UpdateLeaveTime(LeaveTime);
        }

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static AnalogmaControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AnalogmaControl),
                new FrameworkPropertyMetadata(typeof(AnalogmaControl)));
        }
    }
}
