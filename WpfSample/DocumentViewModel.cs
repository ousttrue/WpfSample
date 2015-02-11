using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using Livet.Messaging;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfSample
{
    public class DocumentViewModel: AvalonDockUtil.DocumentViewModelBase
    {
        #region Items
        ObservableCollection<Uri> m_items;
        public ObservableCollection<Uri> Items
        {
            get
            {
                if (m_items == null)
                {
                    m_items = new ObservableCollection<Uri>();
                }
                return m_items;
            }
        }

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

        ReactiveCommand m_addItemsCommand;
        public ReactiveCommand AddItemsCommand
        {
            get
            {
                if (m_addItemsCommand == null)
                {
                    m_addItemsCommand = new ReactiveCommand();
                    m_addItemsCommand.Subscribe(AddItems);
                }
                return m_addItemsCommand;
            }
        }
        void AddItems(Object arg)
        {
            var urilist = (IEnumerable<Uri>)arg;
            foreach (var uri in urilist)
            {
                Items.Add(uri);
            }
            IsDirty = true;
        }

        ReactiveCommand m_clearItemsCommand;
        public ReactiveCommand ClearItemsCommand
        {
            get
            {
                if (m_clearItemsCommand == null)
                {
                    var hasAnyItem = Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => (o, e) => h(e), h => Items.CollectionChanged += h, h => Items.CollectionChanged -= h)
                        .Select(_ => Items.Any())
                        ;
                    m_clearItemsCommand = hasAnyItem.ToReactiveCommand(false);
                    m_clearItemsCommand.Subscribe(ClearItems);
                }
                return m_clearItemsCommand;
            }
        }
        void ClearItems(Object _)
        {
            Items.Clear();
            IsDirty = true;
        }

        ReactiveCommand m_removeSelectedItemCommand;
        public ReactiveCommand RemoveSelectedItemCommand
        {
            get
            {
                if (m_removeSelectedItemCommand == null)
                {
                    var hasSelectedItem = this.ObserveProperty(o => o.SelectedItem).Select(item => item != null);
                    m_removeSelectedItemCommand = hasSelectedItem.ToReactiveCommand(false);
                    m_removeSelectedItemCommand.Subscribe(RemoveSelectedItem);
                }
                return m_removeSelectedItemCommand;
            }
        }
        void RemoveSelectedItem(Object _)
        {
            Items.Remove(SelectedItem);
            IsDirty = true;
        }
        #endregion

        #region Bytes
        Byte[] ToBytes()
        {
            return Items
                .Select(item => item.ToString() + "\n")
                .SelectMany(s => Encoding.UTF8.GetBytes(s))
                .ToArray()
                ;
            ;
        }

        void FromBytes(Byte[] bytes)
        {
            Items.Clear();
            var str = Encoding.UTF8.GetString(bytes);
            AddItems(str.Split('\n')
                .Where(s => !String.IsNullOrEmpty(s))
                .Select(s => new Uri(s))
                );
        }
        #endregion

        #region Save & Load
        override public void Save()
        {
            if (String.IsNullOrEmpty(FilePath))
            {
                Messenger.Raise(SaveDialog);
            }

            try
            {
                System.IO.File.WriteAllBytes(FilePath, ToBytes());
                IsDirty = false;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        override public void Load(String path)
        {
            try
            {
                FilePath = path;
                FromBytes(System.IO.File.ReadAllBytes(FilePath));
                IsDirty = false;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }
        #endregion

        #region AddDialog
        OpeningFileSelectionMessage m_addDialog;
        public OpeningFileSelectionMessage AddDialog
        {
            get
            {
                if (m_addDialog == null)
                {
                    m_addDialog = new OpeningFileSelectionMessage("AddDialog")
                    {
                        Filter = "すべてのファイル(*.*)|*.*",
                        Title = "AddDialog",
                        MultiSelect = true
                    };
                }
                return m_addDialog;
            }
        }

        ReactiveCommand m_addDialogCommand;
        public ReactiveCommand AddDialogCommand
        {
            get
            {
                if (m_addDialogCommand == null)
                {
                    m_addDialogCommand = new ReactiveCommand();
                    m_addDialogCommand.Subscribe(_ =>
                    {
                        Messenger.Raise(AddDialog);
                    });
                }
                return m_addDialogCommand;
            }
        }

        public void OnAddDialog(OpeningFileSelectionMessage m)
        {
            if (m.Response == null)
            {
                InfoMessage("キャンセルされました");
                return;
            }

            AddItems(m.Response.Select(f => new Uri(f)));
        }
        #endregion

        public DocumentViewModel()
        {
        }
    }
}
