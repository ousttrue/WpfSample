using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;


namespace UriListViewModel
{
    public class UriListViewModel: ViewModelBase
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

        Boolean m_isDirty = false;
        public Boolean IsDirty
        {
            get { return m_isDirty; }
            private set
            {
                if (m_isDirty == value) return;
                m_isDirty = value;
                RaisePropertyChanged("IsDirty");
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

        ViewModelCommand m_clearItemsCommand;
        public ICommand ClearItemsCommand
        {
            get
            {
                if (m_clearItemsCommand == null)
                {
                    m_clearItemsCommand = new ViewModelCommand(ClearItems);
                }
                return m_clearItemsCommand;
            }
        }
        void ClearItems()
        {
            Model.Items.Clear();
            IsDirty = true;
        }

        ViewModelCommand m_removeSelectedItemCommand;
        public ICommand RemoveSelectedItemCommand
        {
            get
            {
                if (m_removeSelectedItemCommand == null)
                {
                    m_removeSelectedItemCommand = new ViewModelCommand(RemoveSelectedItem);
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
        String m_path;
        public String Path
        {
            get { return m_path; }
            set
            {
                if (m_path == value) return;
                m_path = value;
                RaisePropertyChanged("Path");
            }
        }

        public void Save(bool saveAs)
        {
            // 保存する
            if (saveAs || String.IsNullOrEmpty(Path))
            {
                // 保存ファイ名が不明
                var path = SaveDialog("Save", Path);
                if (path == null)
                {
                    // キャンセルされた
                    return;
                }
                Path = path;
            }

            try
            {
                System.IO.File.WriteAllBytes(Path, Model.ToBytes());
                IsDirty = false;
            }
            catch (Exception ex)
            {
                ErrorDialog(ex);
            }
        }

        public void Load()
        {
            try
            {
                Model.FromBytes(System.IO.File.ReadAllBytes(Path));
                IsDirty = false;
            }
            catch (Exception ex)
            {
                ErrorDialog(ex);
            }
        }

        ViewModelCommand m_saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (m_saveCommand == null)
                {
                    m_saveCommand = new ViewModelCommand(() =>
                    {
                        Save(true);
                    });
                }
                return m_saveCommand;
            }
        }

        ViewModelCommand m_openCommand;
        public ICommand OpenCommand
        {
            get
            {
                if (m_openCommand == null)
                {
                    m_openCommand = new ViewModelCommand(()=>Open());
                }
                return m_openCommand;
            }
        }
        public bool Open()
        {
            var path = OpenDialog("Open", false);
            if (path == null) return false;
            Path = path[0];
            Load();
            return true;
        }
        #endregion

        #region AddDialogCommand
        ViewModelCommand m_addDialogCommand;
        public ICommand AddDialogCommand
        {
            get{
                if (m_addDialogCommand == null)
                {
                    m_addDialogCommand = new ViewModelCommand(()=>
                    {
                        var response=OpenDialog("追加", true);
                        if (response == null)
                        {
                            InfoDialog("キャンセルされました");
                            return;
                        }
                        AddItems(response.Select(f => new Uri(f)));
                    });
                }
                return m_addDialogCommand;
            }
        }
        #endregion

        public UriListViewModel(InteractionMessenger messenger=null)
        {
            if (messenger != null)
            {
                Messenger = messenger;
            }

            CompositeDisposable.Add(OnDispose);
        }

        void OnDispose()
        {
            if (IsDirty)
            {
                if (ConfirmDialog("変更されています。保存しますか？", "確認"))
                {
                    Save(false);
                }
            }
        }
    }
}
