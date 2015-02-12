using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Reactive.Linq;

namespace AvalonDockMVVMSample
{
    public class DocumentViewModel: AvalonDockUtil.DocumentBase
    {
        #region Items
        UriListModel.UriListModel m_model;
        UriListModel.UriListModel Model
        {
            get
            {
                if (m_model == null)
                {
                    m_model = new UriListModel.UriListModel();
                    m_model.Items.CollectionChanged += (o, e) =>
                    {
                        HasAnyItem = m_model.Items.Any();
                    };
                }
                return m_model;
            }
        }

        public ObservableCollection<Uri> Items
        {
            get
            {
                return Model.Items;
            }
        }

        bool m_hasAnyItem;
        public bool HasAnyItem
        {
            get { return m_hasAnyItem; }
            private set
            {
                if (m_hasAnyItem == value) return;
                m_hasAnyItem = value;
                RaisePropertyChanged("HasAnyItem");
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

        ListenerCommand<IEnumerable<Uri>> m_addItemsCommand;
        public ICommand AddItemsCommand
        {
            get
            {
                if (m_addItemsCommand == null)
                {
                    m_addItemsCommand = new ListenerCommand<IEnumerable<Uri>>(AddItems);
                }
                return m_addItemsCommand;
            }
        }
        void AddItems(IEnumerable<Uri> urilist)
        {
            foreach (var uri in urilist)
            {
                Model.Items.Add(uri);
            }
            IsDirty = true;
        }

        ReactiveCommand m_clearItemsCommand;
        public ICommand ClearItemsCommand
        {
            get
            {
                if (m_clearItemsCommand == null)
                {
                    var hasAnyItem = this.ObserveProperty(o => o.HasAnyItem);
                    m_clearItemsCommand = hasAnyItem.ToReactiveCommand(false);
                    m_clearItemsCommand.Subscribe(_ => ClearItems());
                }
                return m_clearItemsCommand;
            }
        }
        void ClearItems()
        {
            Model.Items.Clear();
            IsDirty = true;
        }

        ReactiveCommand m_removeSelectedItemCommand;
        public ICommand RemoveSelectedItemCommand
        {
            get
            {
                if (m_removeSelectedItemCommand == null)
                {
                    var hasSelectedItem=this.ObserveProperty(o=>o.SelectedItem)
                        .Select(item=>item!=null)
                        ;
                    m_removeSelectedItemCommand = hasSelectedItem.ToReactiveCommand(false);
                    m_removeSelectedItemCommand.Subscribe(_=>RemoveSelectedItem());                        
                }
                return m_removeSelectedItemCommand;
            }
        }
        void RemoveSelectedItem()
        {
            Model.Items.Remove(SelectedItem);
            IsDirty = true;
        }
        #endregion

        #region Save & Load
        public override void Save(bool asFlag)
        {
            try
            {
                if (asFlag|| String.IsNullOrEmpty(FilePath))
                {
                    var response = SaveDialog("Save", FilePath);
                    if (response == null)
                    {
                        return;
                    }
                    FilePath = response;
                }
                System.IO.File.WriteAllBytes(FilePath, Model.ToBytes());
                IsDirty = false;
            }
            catch (Exception ex)
            {
                ErrorDialog(ex);
            }
        }

        public override void Load()
        {
            try
            {
                Model.FromBytes(System.IO.File.ReadAllBytes(FilePath));
                IsDirty = false;
            }
            catch (Exception ex)
            {
                ErrorDialog(ex);
            }
        }
        #endregion

        ViewModelCommand m_addCommand;
        public ICommand AddCommand
        {
            get
            {
                if (m_addCommand == null)
                {
                    m_addCommand = new ViewModelCommand(() =>
                    {
                        var response = OpenDialog("Add", true);
                        if (response == null) return;
                        AddItems(response.Select(path => new Uri(path)));
                    });
                }
                return m_addCommand;
            }
        }
        public DocumentViewModel()
        {
        }
    }
}
