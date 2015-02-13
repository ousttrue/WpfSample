using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using Xceed.Wpf.AvalonDock.Layout;

namespace AvalonDockSample
{
    public class SetVisibleBehavior : Behavior<LayoutAnchorable>
    {
        public MainWindowViewModel Target
        {
            get { return (MainWindowViewModel)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(MainWindowViewModel), typeof(SetVisibleBehavior), new PropertyMetadata(null));


        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.IsVisibleChanged += (o, e) =>
                {
                    Target.ToolIsVisible = this.AssociatedObject.IsVisible;
                };
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
