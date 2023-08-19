using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace myVid
{
    public class WatermarkBehavior : Behavior<TextBox>
    {
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(WatermarkBehavior), new UIPropertyMetadata(string.Empty));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            UpdateWatermark();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            UpdateWatermark();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            UpdateWatermark();
        }

        private void UpdateWatermark()
        {
            if (AssociatedObject == null)
                return;

            if (string.IsNullOrEmpty(AssociatedObject.Text))
            {
                AssociatedObject.Text = Watermark;
                AssociatedObject.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
            else if (AssociatedObject.Text == Watermark)
            {
                AssociatedObject.Text = string.Empty;
                AssociatedObject.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }
    }
}