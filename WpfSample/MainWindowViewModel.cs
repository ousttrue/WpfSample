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


namespace WpfSample
{
    /// <summary>
    /// Livetのダイアログ４種類のヘルパー関数
    /// </summary>
    class MessagingViewModel : Livet.ViewModel
    {
        #region InformationMessage
        protected void InfoDialog(String message)
        {
            Messenger.Raise(new InformationMessage(message, "Info", MessageBoxImage.Information, "Info"));
        }

        protected void ErrorDialog(Exception ex)
        {
            Messenger.Raise(new InformationMessage(ex.Message, "Error", MessageBoxImage.Error, "Info"));
        }
        #endregion

        #region ConfirmationMessage
        ConfirmationMessage m_confirmationMessage;
        public ConfirmationMessage ConfirmationMessage
        {
            get { return m_confirmationMessage; }
            set
            {
                if (m_confirmationMessage == value) return;
                m_confirmationMessage = value;
                RaisePropertyChanged("ConfirmationMessage");
            }
        }

        protected bool ConfirmDialog(String text, String title)
        {
            ConfirmationMessage = new ConfirmationMessage(text, title
                        , MessageBoxImage.Question, MessageBoxButton.YesNo, "Confirm");
            Messenger.Raise(ConfirmationMessage);
            return ConfirmationMessage.Response.HasValue && ConfirmationMessage.Response.Value;
        }
        #endregion

        #region OpeningFileSelectionMessage
        OpeningFileSelectionMessage m_openingFileSelectionMessage;
        public OpeningFileSelectionMessage OpeningFileSelectionMessage
        {
            get { return m_openingFileSelectionMessage; }
            private set
            {
                if (m_openingFileSelectionMessage == value) return;
                m_openingFileSelectionMessage = value;
                RaisePropertyChanged("OpeningFileSelectionMessage");
            }
        }
        protected String[] OpenDialog(String title, bool multiSelect = false)
        {
            return OpenDialog(title, "すべてのファイル(*.*)|*.*", multiSelect);
        }
        protected String[] OpenDialog(String title, String filter, bool multiSelect)
        {
            OpeningFileSelectionMessage = new OpeningFileSelectionMessage("Open")
            {
                Title = title,
                Filter = filter,
                MultiSelect = multiSelect,
            };
            Messenger.Raise(OpeningFileSelectionMessage);
            return OpeningFileSelectionMessage.Response;
        }
        #endregion

        #region SavingFileSelectionMessage
        SavingFileSelectionMessage m_savingFileSelectionMessage;
        public SavingFileSelectionMessage SavingFileSelectionMessage
        {
            get{return m_savingFileSelectionMessage;}
            set
            {
                if (m_savingFileSelectionMessage == value) return;
                m_savingFileSelectionMessage = value;
                RaisePropertyChanged("SavingFileSelectionMessage");
            }
        }

        protected String SaveDialog(String title, string filename)
        {
            SavingFileSelectionMessage = new SavingFileSelectionMessage("Save")
            {
                Title = title,
                FileName = String.IsNullOrEmpty(filename) ? "list.txt" : filename,
            };
            Messenger.Raise(SavingFileSelectionMessage);
            return SavingFileSelectionMessage.Response != null ? SavingFileSelectionMessage.Response[0] : null;
        }
        #endregion
    }


    class MainWindowViewModel: MessagingViewModel
    {
        #region Items
        ObservableCollection<Uri> m_items;
        public ObservableCollection<Uri> Items {
            get
            {
                if (m_items == null)
                {
                    m_items = new ObservableCollection<Uri>();
                }
                return m_items;
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
                Items.Add(uri);
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
            Items.Clear();
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
                .Where(s=>!String.IsNullOrEmpty(s))
                .Select(s=>new Uri(s))
                );
        }
        #endregion

        #region Save & Load
        public String Path
        {
            get;
            private set;
        }

        void Save(bool saveAs)
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
                System.IO.File.WriteAllBytes(Path, ToBytes());
            }
            catch(Exception ex)
            {
                ErrorDialog(ex);
            }
        }

        void Load()
        {
            try
            {
                FromBytes(System.IO.File.ReadAllBytes(Path));
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
                    m_saveCommand = new ViewModelCommand(()=>
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
                    m_openCommand = new ViewModelCommand(()=>
                    {
                        var path = OpenDialog("Open");
                        if (path == null) return;
                        Path=path[0];
                        Load();
                    });
                }
                return m_openCommand;
            }
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

        public MainWindowViewModel()
        {
            Items.CollectionChanged += (o, e) =>
            {
                HasAnyItem = Items.Any();
            };

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
