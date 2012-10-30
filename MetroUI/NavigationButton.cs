using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    [TemplatePart(Name = NavigationButton.ElementButtonGrid, Type = typeof(Grid))]
    [TemplatePart(Name = NavigationButton.ElementButton, Type = typeof(Button))]
    public class NavigationButton : ContentControl
    {
        private const string ElementButtonGrid = "PART_ButtonGrid";
        private const string ElementButton = "PART_Button";

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

                UpdateState();
            }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(NavigationButton), new PropertyMetadata(default(ImageSource)));

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

        static NavigationButton()
        {
            // required to override the default styling so that our styles are picked up from generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationButton), new FrameworkPropertyMetadata(typeof(NavigationButton)));
        }

        public NavigationButton()
        {
        }

        ~NavigationButton()
        {

            if (_button != null)
            {
                try
                {
                    _button.Click -= Button_Click;
                    _gridButton.MouseEnter -= parens_MouseEvent;
                    _gridButton.MouseLeave -= parens_MouseEvent;
                }
                catch { }
            }

        }

        private Grid _gridButton;
        private Button _button;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            _gridButton = GetTemplateChild(ElementButtonGrid) as Grid;

            if (_gridButton == null)
                throw new MissingMemberException("Expected to find " + ElementButtonGrid);

            _gridButton.MouseEnter += parens_MouseEvent;
            _gridButton.MouseLeave += parens_MouseEvent;

            _button = GetTemplateChild(ElementButton) as Button;

            if (_button == null)
                throw new MissingMemberException("Expected to find " + ElementButton);

            _button.Click += Button_Click;

            UpdateState();
        }

        /// <summary>
        /// Updates the VSM state of the control
        /// </summary>
        private void UpdateState()
        {
            if (_gridButton != null)
                VisualStateManager.GoToElementState(_gridButton, (IsSelected ? "SelectedNormal" : "Normal"), true);
        }

        private void parens_MouseEvent(object sender, MouseEventArgs e)
        {

            string toState = (IsSelected ? "IsSelected" : "");

            if (_gridButton.IsMouseOver || e.LeftButton == MouseButtonState.Pressed)
            {
                toState += "MouseOver";
            }
            else
            {
                toState += "Normal";
            }

            VisualStateManager.GoToElementState(_gridButton, toState, true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSelected)
                IsSelected = true;
        }
    }
}
