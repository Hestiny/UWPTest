using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml.Documents;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using System.ComponentModel;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App1.Control
{

    public sealed class Employee : INotifyPropertyChanged
    {
        private string color;
        private double opacity;

        public string Color
        {
            get { return color; }
            set
            {
                color = value;
                RaisePropertyChanged("Color");
            }
        }

        public double Opacity
        {
            get { return opacity; }
            set
            {
                opacity = value;
                RaisePropertyChanged("Opacity");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ControlPage : Page
    {
        public ControlPage()
        {
            employee.Color = "#FFFF1111";
            employee.Opacity = 1;
            this.InitializeComponent();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);
            // Hide default title bar.


            // Set XAML element as a draggable region.
            //Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
        }

        public UnderlineType Underline = UnderlineType.Wave;
        Employee employee = new Employee();
        string Color = "#FFFF1111";

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }


        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            TitleBarButton.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }


        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TextRange textRange1 = new TextRange()
            {
                StartIndex = 0,
                Length = rt.Text.Length
            };
            TextHighlighter highlighter = new TextHighlighter()
            {
                Background = richEditBox.Foreground,
                Ranges = { textRange1 }
            };

            rt.TextHighlighters.Clear();
            rt.TextHighlighters.Add(highlighter);

            richEditBox.Document.SetText(TextSetOptions.None, new string((char)160, 10));
            Windows.UI.Text.ITextSelection selectedText = richEditBox.Document.Selection;
            ITextRange textRange = richEditBox.Document.GetRange(0, 4);
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                if (charFormatting.Underline == Windows.UI.Text.UnderlineType.None)
                {
                    textRange.CharacterFormat.Underline = Windows.UI.Text.UnderlineType.Wave;
                    charFormatting.Underline = Windows.UI.Text.UnderlineType.Wave;
                }
                else
                {
                    textRange.CharacterFormat.Underline = Windows.UI.Text.UnderlineType.None;
                    charFormatting.Underline = Windows.UI.Text.UnderlineType.Wave;
                }
                //selectedText.CharacterFormat = charFormatting;
            }
        }

        private void richEditBox_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            employee.Color = "#" + ((int)((e.NewValue / 100) * 255)).ToString("X2") + employee.Color.Substring(3);
            var m = box.Foreground;
            box.Foreground.Opacity = e.NewValue / 100;
            box.IsHitTestVisible = true;
            VisualStateManager.GoToState(box, "PointerOver", true);

            wite();
        }
        private async void wite()
        {
            await Task.Delay(10);
            VisualStateManager.GoToState(box, "Normal", true);
            box.IsHitTestVisible = false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //必须刷新状态才能更新前景色
            box.IsHitTestVisible = true;
            VisualStateManager.GoToState(box, "Normal", true);
            box.IsHitTestVisible = false;
        }

    }
}
