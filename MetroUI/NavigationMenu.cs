using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MetroUI
{
    [TemplatePart(Name = NavigationMenu.ElementPageContent, Type = typeof(ContentControl))]
    public class NavigationMenu : ContentControl
    {
        private const string ElementPageContent = "PART_PageContent";

        static NavigationMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationMenu), new FrameworkPropertyMetadata(typeof(NavigationMenu)));
        }

        public static readonly DependencyProperty SelectedContentProperty = DependencyProperty.Register("SelectedContent", typeof(List<Object>), typeof(NavigationMenu), new PropertyMetadata(default(object)));

        public object SelectedContent
        {
            get { return (object)GetValue(SelectedContentProperty); }
            set { SetValue(SelectedContentProperty, value); }
        }

        private ContentControl _pageContent = null;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _pageContent = GetTemplateChild(ElementPageContent) as ContentControl;

            if (_pageContent == null)
                throw new MissingMemberException("Expected to find template member: " + ElementPageContent);
        }

        public void SelectionChanged(NavigationButton button)
        {
            _pageContent.Content = button.Content;
        }
    }
}
