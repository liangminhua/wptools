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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetroUI
{
    //[TemplatePart(Name = TimePicker.ElementHourTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name=MetroDialog.ElementCloseButton, Type=typeof(ImageButton))]
    public class MetroDialog : ContentControl
    {

        private const string ElementCloseButton = "PART_CloseButton";

        static MetroDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroDialog), new FrameworkPropertyMetadata(typeof(MetroDialog)));
        }

        private ImageButton _exitButton;
        
        public MetroDialog()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _exitButton = GetTemplateChild(ElementCloseButton) as ImageButton;

            if (_exitButton != null)
            {
                _exitButton.Click += ExitButton_Click;
            }
        }

        ~MetroDialog()
        {
            if (_exitButton != null)
                _exitButton.Click -= ExitButton_Click;
        }

        void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Show()
        {
            Visibility = System.Windows.Visibility.Visible;
        }

        public void Close()
        {
            Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
