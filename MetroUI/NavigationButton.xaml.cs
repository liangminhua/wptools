using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// <summary>
    /// Interaction logic for NavigationButton.xaml
    /// </summary>
    public partial class NavigationButton : UserControl
    {
        public event EventHandler OnSelectionChanged;

        #region Properties

        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(Object), typeof(NavigationButton), new PropertyMetadata(default(Object)));

        public new Object Content
        {
            get { return (Object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(NavigationButton), new PropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set 
            { 
                SetValue(IsSelectedProperty, value);

                if (OnSelectionChanged != null)
                {
                    OnSelectionChanged(this, null);
                }
            }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(NavigationButton), new PropertyMetadata(default(ImageSource)));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NavigationButton), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        public NavigationButton()
        {
            InitializeComponent();
        }

        private void parens_MouseEvent(object sender, MouseEventArgs e)
        {
            string toState = (IsSelected ? "IsSelected" : "");

            if (button.IsMouseOver)
            {
                toState += "MouseOver";
            }
            else
            {
                toState += "Normal";
            }

            VisualStateManager.GoToElementState(button, toState, true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsSelected = !IsSelected;
        }
    }
}
