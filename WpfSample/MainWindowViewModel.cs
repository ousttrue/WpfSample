using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
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

        public Boolean IsDirty
        {
            get;
            private set;
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

        void Save()
        {
            if (String.IsNullOrEmpty(Path))
            {
                Messenger.Raise(SaveDialog);
            }

            try
            {
                System.IO.File.WriteAllBytes(Path, ToBytes());
            }
            catch(Exception ex)
            {
                ErrorMessage(ex);
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
                ErrorMessage(ex);
            }
        }

        ReactiveCommand m_saveCommand;
        public ReactiveCommand SaveCommand
        {
            get
            {
                if (m_saveCommand == null)
                {
                    m_saveCommand = new ReactiveCommand();
                    m_saveCommand.Subscribe(_ =>
                    {
                        Messenger.Raise(SaveDialog);
                    });
                }
                return m_saveCommand;
            }
        }

        ReactiveCommand m_openCommand;
        public ReactiveCommand OpenCommand
        {
            get
            {
                if (m_openCommand == null)
                {
                    m_openCommand = new ReactiveCommand();
                    m_openCommand.Subscribe(_ =>
                    {
                        Messenger.Raise(OpenDialog);
                    });
                }
                return m_openCommand;
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
            get{
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

        #region MessageDialog
        void InfoMessage(String message)
        {
            Messenger.Raise(new InformationMessage(message, "Info", MessageBoxImage.Information, "MessageDialog"));
        }

        void ErrorMessage(Exception ex)
        {
            Messenger.Raise(new InformationMessage(ex.Message, "Error", MessageBoxImage.Error, "MessageDialog"));
        }
        #endregion

        #region SaveConfirmDialog
        ConfirmationMessage m_saveConfirmDialog;
        public ConfirmationMessage SaveConfirmDialog
        {
            get
            {
                if (m_saveConfirmDialog == null)
                {
                    m_saveConfirmDialog = new ConfirmationMessage("変更されています。保存しますか？", "確認"
                        , MessageBoxImage.Question, MessageBoxButton.YesNo, "SaveConfirmDialog");
                }
                return m_saveConfirmDialog;
            }
        }

        public void OnSaveConfirmDialog(ConfirmationMessage msg)
        {
            if (msg.Response.HasValue && msg.Response.Value)
            {
                // 保存する
                if (String.IsNullOrEmpty(Path))
                {
                    // 名前不明
                    Messenger.Raise(SaveDialog);
                }
                else
                {
                    Save();
                }
            }
        }
        #endregion

        #region SaveDialog
        SavingFileSelectionMessage m_saveDialog;
        public SavingFileSelectionMessage SaveDialog
        {
            get
            {
                if (m_saveDialog == null)
                {
                    m_saveDialog = new SavingFileSelectionMessage("SaveDialog")
                    {
                        Title="SaveDialog",       
                        FileName=String.IsNullOrEmpty(Path) ? "list.txt" : Path,
                    };
                }
                return m_saveDialog;
            }
        }

        public void OnSaveDialog(SavingFileSelectionMessage msg)
        {
            if (msg.Response == null) return;

            Path = msg.Response[0];
            Save();
        }
        #endregion

        #region OpenDialog
        OpeningFileSelectionMessage m_openDialog;
        public OpeningFileSelectionMessage OpenDialog
        {
            get
            {
                if (m_openDialog == null)
                {
                    m_openDialog = new OpeningFileSelectionMessage("OpenDialog")
                    {
                        Title="OpenDialog",
                    };
                }
                return m_openDialog;
            }
        }

        public void OnOpenDialog(OpeningFileSelectionMessage msg)
        {
            if (msg.Response == null) return;

            Path = msg.Response[0];
            Load();
        }
        #endregion

        public MainWindowViewModel()
        {
            CompositeDisposable.Add(() =>
            {
                if (IsDirty)
                {
                    Messenger.Raise(SaveConfirmDialog);
                }
            });
        }

        #region Layout
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
        #endregion
    }
}
