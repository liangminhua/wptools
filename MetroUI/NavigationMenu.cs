using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MetroUI
{
    [TemplatePart(Name = NavigationMenu.ElementPageContent, Type = typeof(ContentControl))]
    [TemplatePart(Name = NavigationMenu.MenuContent, Type = typeof(Panel))]
    [ContentProperty("Children")]
    public class NavigationMenu : ContentControl
    {
        private const string ElementPageContent = "PART_PageContent";
        private const string MenuContent = "PART_MenuContent";

        static NavigationMenu()
        {
            // required to override the default styling so that our styles are picked up from generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationMenu), new FrameworkPropertyMetadata(typeof(NavigationMenu)));
        }

        #region Dependency Properties

        public static readonly DependencyProperty SelectedContentProperty = DependencyProperty.Register("SelectedContent", typeof(List<Object>), typeof(NavigationMenu), new PropertyMetadata(default(object)));

        public object SelectedContent
        {
            get { return (object)GetValue(SelectedContentProperty); }
            set { SetValue(SelectedContentProperty, value); }
        }

        // null default value to force us to go through the property getter / setters for valid setup
        public static readonly DependencyProperty ChildrenContentProperty = DependencyProperty.Register("Children", typeof(ObservableCollection<Object>), typeof(NavigationMenu), new PropertyMetadata(null));

        public ObservableCollection<object> Children
        {
            get
            {
                var rv = GetValue(ChildrenContentProperty) as ObservableCollection<object>;

                if (rv == null)
                {
                    // create a new one
                    rv = new ObservableCollection<object>();

                    Children = rv;
                }

                return rv;
            }

            set
            {
                SetValue(ChildrenContentProperty, value);
                value.CollectionChanged += children_CollectionChanged;
            }
        }

        #endregion

        private ContentControl _pageContent = null;
        private Panel _menuContent = null;

        private NavigationButton _selectedButton = null;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _pageContent = GetTemplateChild(ElementPageContent) as ContentControl;

            if (_pageContent == null)
                throw new MissingMemberException("Expected to find template member: " + ElementPageContent);

            _menuContent = GetTemplateChild(MenuContent) as Panel;

            if (_menuContent == null)
                throw new MissingMemberException("Expected to find template member: " + MenuContent);

            AddChildren();

            // select the first child
            if (Children.Count > 0)
            {
                NavigationButton button = Children[0] as NavigationButton;

                if (button != null)
                {
                    button.IsSelected = true;
                    SelectionChanged(Children[0] as NavigationButton);
                }
            }
        }

        /// <summary>
        /// Add any children to the menu panel. This is useful since OnApplyTemplate will be called after 
        /// the initial children collection is populated
        /// </summary>
        private void AddChildren()
        {
            foreach (var i in Children)
            {
                _menuContent.Children.Add(i as UIElement);
            }
        }

        void children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_menuContent == null)
                return;

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var i in e.NewItems)
                {
                    _menuContent.Children.Add(i as UIElement);
                }
            }
        }

        public void SelectionChanged(NavigationButton button)
        {
            // ignore the case where the selection changed to be "not selected" or is already selected
            if (button.IsSelected == false || button == _selectedButton)
                return;

            if (_selectedButton != null)
            {
                _selectedButton.IsSelected = false;
            }

            _selectedButton = button;

            _pageContent.Content = button.Content;
        }
    }
}
