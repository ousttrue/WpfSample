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
