using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using System;
using System.Windows;
using System.Windows.Input;

namespace AvalonDockUtil
{
    public abstract class DocumentViewModelBase : PaneViewModelBase
    {
        #region FilePath
        private string _filePath = null;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    RaisePropertyChanged("FilePath");
                    RaisePropertyChanged("FileName");
                    RaisePropertyChanged("Title");
                }
            }
        }

        public string FileName
        {
            get
            {
                if (FilePath == null)
                    return "Noname" + (IsDirty ? "*" : "");

                return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
            }
        }
        #endregion

        #region IsDirty
        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged("IsDirty");
                    RaisePropertyChanged("FileName");
                }
            }
        }
        #endregion

        #region SaveCommand
        RelayCommand _saveCommand = null;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(_ => Save(false), _ => IsDirty);
                }
                return _saveCommand;
            }
        }
        #endregion

        #region SaveAsCommand
        RelayCommand _saveAsCommand = null;
        public ICommand SaveAsCommand
        {
            get
            {
                if (_saveAsCommand == null)
                {
                    _saveAsCommand = new RelayCommand(_ => Save(true), _ => IsDirty);
                }
                return _saveAsCommand;
            }
        }
        #endregion

        #region CloseCommand
        RelayCommand _closeCommand = null;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(_ => Dispose(true));
                }
                return _closeCommand;
            }
        }
        #endregion

        #region MessageDialog
        protected void InfoMessage(String message)
        {
            Messenger.Raise(new InformationMessage(message, "Info", MessageBoxImage.Information, "MessageDialog"));
        }

        protected void ErrorMessage(Exception ex)
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
                if (String.IsNullOrEmpty(FilePath))
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
                        Title = "SaveDialog",
                        FileName = String.IsNullOrEmpty(FilePath) ? "list.txt" : FilePath,
                    };
                }
                return m_saveDialog;
            }
        }

        public void OnSaveDialog(SavingFileSelectionMessage msg)
        {
            if (msg.Response == null) return;

            FilePath = msg.Response[0];
            Save();
        }
        #endregion

        abstract public void Load(String path);
        void Save(bool AsFlag)
        {
            if (String.IsNullOrEmpty(FilePath) || AsFlag)
            {
                Messenger.Raise(SaveDialog);
            }
            Save();
        }
        abstract public void Save();

        protected DocumentViewModelBase()
        {
            CompositeDisposable.Add(() =>
            {
                if (IsDirty)
                {
                    SaveConfirmDialog.Text=string.Format("Save changes for file '{0}'?", FileName);
                    Messenger.Raise(SaveConfirmDialog);
                }
            });
        }
    }
}
