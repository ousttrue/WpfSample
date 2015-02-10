using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reactive.Linq;
using System.Collections.Specialized;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.IO;


namespace WpfSample
{
    class MainWindowViewModel: Livet.ViewModel
    {
        public ObservableCollection<Uri> Items { get; private set; }

        Uri m_selectedItem;
        public Uri SelectedItem
        {
            get { return m_selectedItem; }
            set
            {
                if (m_selectedItem == value) return;
                m_selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        public ReactiveCommand AddItemsCommand { get; private set; }
        void AddItems(Object arg)
        {
            var urilist = (IEnumerable<Uri>)arg;

            foreach (var uri in urilist)
            {
                Items.Add(uri);
            }
        }

        public ListenerCommand<OpeningFileSelectionMessage> OpenCommand { get; private set; }
        void Open(OpeningFileSelectionMessage m)
        {
            if (m.Response == null)
            {
                Messenger.Raise(new InformationMessage("Cancel", "Error", MessageBoxImage.Error, "Info"));
                return;
            }

            AddItems(m.Response.Select(f => new Uri(f)));
        }

        public ReactiveCommand ClearItemsCommand { get; private set; }
        void ClearItems(Object _)
        {
            Items.Clear();
        }

        public ReactiveCommand RemoveSelectedItemCommand { get; private set; }
        void RemoveSelectedItem(Object _)
        {
            Items.Remove(SelectedItem);
        }

        String LayoutFile { 
            get { 
                return System.IO.Path.ChangeExtension(
                    Environment.GetCommandLineArgs()[0]
                    , ".AvalonDock.config"
                    );
            } 
        }

        ReactiveCommand m_loadLayoutCommand;
        public ReactiveCommand LoadLayoutCommand
        {
            get
            {
                if (m_loadLayoutCommand == null)
                {
                    m_loadLayoutCommand = new ReactiveCommand();
                    m_loadLayoutCommand.Subscribe(LoadLayout);
                }
                return m_loadLayoutCommand;
            }
        }
        void LoadLayout(Object arg)
        {
            var dockManager=(DockingManager)arg;
            var currentContentsList = dockManager.Layout.Descendents().OfType<LayoutContent>().Where(c => c.ContentId != null).ToArray();          
            var serializer = new XmlLayoutSerializer(dockManager);
            //serializer.LayoutSerializationCallback += (s, args) =>
            //    {
            //        var prevContent = currentContentsList.FirstOrDefault(c => c.ContentId == args.Model.ContentId);
            //        if (prevContent != null)
            //            args.Content = prevContent.Content;
            //    };
            try
            {
                using (var stream = new StreamReader(LayoutFile))
                    serializer.Deserialize(stream);
            }
            catch(FileNotFoundException ex)
            {

            }
        }

        ReactiveCommand m_saveLayoutCommand;
        public ReactiveCommand SaveLayoutCommand {
            get {
                if(m_saveLayoutCommand == null)
                {
                    m_saveLayoutCommand = new ReactiveCommand();
                    m_saveLayoutCommand.Subscribe(SaveLayout);
                }
                return m_saveLayoutCommand; 
            }
        }
        void SaveLayout(Object arg)
        {
            var dockManager = (DockingManager)arg;
            var serializer = new XmlLayoutSerializer(dockManager);
            using (var stream = new StreamWriter(LayoutFile))
                serializer.Serialize(stream);
        }

        public MainWindowViewModel()
        {
            Items = new ObservableCollection<Uri>();

            AddItemsCommand = new ReactiveCommand();
            AddItemsCommand.Subscribe(AddItems);

            var hasAnyItem = Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => (o, e) => h(e), h => Items.CollectionChanged += h, h => Items.CollectionChanged -= h)
                .Select(_ => Items.Any())
                ;
            ClearItemsCommand = hasAnyItem.ToReactiveCommand(false);
            ClearItemsCommand.Subscribe(ClearItems);

            var hasSelectedItem = this.ObserveProperty(o => o.SelectedItem).Select(item => item != null);
            RemoveSelectedItemCommand = hasSelectedItem.ToReactiveCommand(false);
            RemoveSelectedItemCommand.Subscribe(RemoveSelectedItem);

            OpenCommand = new ListenerCommand<OpeningFileSelectionMessage>(Open, () => true);
        }
    }
}
