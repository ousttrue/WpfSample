using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace UriListViewModel
{
    public class FileDropBehavior : Behavior<FrameworkElement>
    {
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(FileDropBehavior)
            , new PropertyMetadata(null));


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewDragOver += OnPreviewDragOver;
            this.AssociatedObject.Drop += OnDrop;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.PreviewDragOver -= OnPreviewDragOver;
            this.AssociatedObject.Drop -= OnDrop;
        }

        private void OnPreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent("UniformResourceLocator"))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }

        IEnumerable<Uri> ToUrlList(IDataObject data)
        {
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                return (data.GetData(DataFormats.FileDrop) as string[])
                    .Select(s => new Uri(s))
                    ;
            }
            else
            {
                var url = new Uri(data.GetData(DataFormats.Text).ToString());
                return new Uri[] { url };
            }
        }

        private void OnDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (Command == null) return;
            if (!Command.CanExecute(e)) return;
            var urllist = ToUrlList(e.Data);
            if (urllist == null) return;
            Command.Execute(urllist);
        }
    }

}
