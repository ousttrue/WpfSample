using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using Livet.Commands;
using System;
using System.Windows.Input;
using System.Windows.Media;

namespace AvalonDockUtil
{
    public abstract class DocumentBase: ViewModelBase, IPaneContent
    {
        bool m_isSelected;
        public bool IsSelected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                if(m_isSelected==value)return;
                m_isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        bool m_isActive;
        public bool IsActive
        {
            get
            {
                return m_isActive;
            }
            set
            {
                if (m_isActive == value) return;
                m_isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        ImageSource m_iconSource;
        public ImageSource IconSource
        {
            get { return m_iconSource; }
            set
            {
                if (m_iconSource == value) return;
                m_iconSource = value;
                RaisePropertyChanged("IconSource");
            }
        }

        readonly String DefaultFileName="list.txt";

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
                    Title = FileName;
                    RaisePropertyChanged("FilePath");
                    RaisePropertyChanged("FileName");
                    RaisePropertyChanged("ContentId");
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

        public String ContentId
        {
            get
            {
                return FilePath;
            }
        }

        string m_title = "Untitleed";
        public String Title
        {
            get { return m_title; }
            set
            {
                if (m_title == value) return;
                m_title = value;
                RaisePropertyChanged("Title");
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
        ReactiveCommand m_saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (m_saveCommand == null)
                {
                    m_saveCommand = this.ObserveProperty(o => o.IsDirty).ToReactiveCommand(false);
                    m_saveCommand.Subscribe(_=>Save(false));
                }
                return m_saveCommand;
            }
        }
        #endregion

        #region SaveAsCommand
        ReactiveCommand m_saveAsCommand;
        public ICommand SaveAsCommand
        {
            get
            {
                if (m_saveAsCommand == null)
                {
                    m_saveAsCommand = this.ObserveProperty(o => o.IsDirty).ToReactiveCommand(false);
                    m_saveCommand.Subscribe(_ => Save(true));
                }
                return m_saveAsCommand;
            }
        }
        #endregion

        #region CloseCommand
        ViewModelCommand m_closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (m_closeCommand == null)
                {
                    m_closeCommand = new ViewModelCommand(() => Dispose());
                }
                return m_closeCommand;
            }
        }
        #endregion

        abstract public void Load();
        abstract public void Save(bool AsFlag);
        public void ConfirmSave()
        {
            if (!IsDirty)
            {
                return;
            }
            if (!ConfirmDialog(string.Format("Save changes for file '{0}'?", FileName), "Confirm"))
            {
                return;
            }
            Save(false);
        }

        protected DocumentBase()
        {
            CompositeDisposable.Add(() =>
            {
                ConfirmSave();
            });
        }
    }
}
