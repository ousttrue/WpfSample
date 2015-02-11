using Livet.Messaging;
using Livet.Messaging.IO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace AvalonDockUtil
{
    public abstract class WorkspaceBase : Livet.ViewModel
    {
        #region Documents
        ObservableCollection<DocumentViewModelBase> m_documents = new ObservableCollection<DocumentViewModelBase>();
        ReadOnlyObservableCollection<DocumentViewModelBase> _readonyFiles = null;
        public ReadOnlyObservableCollection<DocumentViewModelBase> Documents
        {
            get
            {
                if (_readonyFiles == null)
                    _readonyFiles = new ReadOnlyObservableCollection<DocumentViewModelBase>(m_documents);
                return _readonyFiles;
            }
        }
        #endregion

        #region ActiveDocument
        private DocumentViewModelBase _activeDocument = null;
        public DocumentViewModelBase ActiveDocument
        {
            get
            {
                return _activeDocument;
            }
            set
            {
                if (_activeDocument == value) return;
                _activeDocument = value;
                foreach (var t in Tools)
                {
                    t.Document = value;
                }
                RaisePropertyChanged("ActiveDocument");
            }
        }
        #endregion

        #region Tools
        ObservableCollection<ToolViewModelBase> m_tools = new ObservableCollection<ToolViewModelBase>();
        ReadOnlyObservableCollection<ToolViewModelBase> _readonyTools = null;
        public ReadOnlyObservableCollection<ToolViewModelBase> Tools
        {
            get
            {
                if (_readonyTools == null)
                    _readonyTools = new ReadOnlyObservableCollection<ToolViewModelBase>(m_tools);
                return _readonyTools;
            }
        }

        protected void AddTool(ToolViewModelBase tool)
        {
            m_tools.Add(tool);
        }
        #endregion

        #region NewCommand
        RelayCommand _newCommand = null;
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null) _newCommand = new RelayCommand(p => NewDocument(p));
                return _newCommand;
            }
        }
        #endregion

        #region OpenCommand
        RelayCommand _openCommand = null;
        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null) _openCommand = new RelayCommand(OpenDocument);
                return _openCommand;
            }
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
                        Title = "OpenDialog",
                    };
                }
                return m_openDialog;
            }
        }

        public void OnOpenDialog(OpeningFileSelectionMessage msg)
        {
            if (msg.Response == null) return;

            CreateDocumentFromFilePath(msg.Response[0]);
        }
        #endregion

        #region LoadLayoutCommand
        RelayCommand _loadLayoutCommand = null;
        public ICommand LoadLayoutCommand
        {
            get
            {
                if (_loadLayoutCommand == null)
                {
                    _loadLayoutCommand = new RelayCommand(p => LoadLayout((DockingManager)p), _ => File.Exists(LayoutFile));
                }
                return _loadLayoutCommand;
            }
        }
        #endregion

        #region SaveLayoutCommand
        RelayCommand _saveLayoutCommand = null;
        public ICommand SaveLayoutCommand
        {
            get
            {
                if (_saveLayoutCommand == null)
                {
                    _saveLayoutCommand = new RelayCommand(p => SaveLayout((DockingManager)p));
                }
                return _saveLayoutCommand;
            }
        }
        #endregion

        #region DefaultLayoutCommand
        RelayCommand m_defaultLayoutCommand;
        public ICommand DefaultLayoutCommand
        {
            get
            {
                if (m_defaultLayoutCommand == null)
                {
                    m_defaultLayoutCommand = new RelayCommand(_ =>
                    {
                        DefaultLayout();
                    });
                }
                return m_defaultLayoutCommand;
            }
        }
        #endregion

        public abstract DocumentViewModelBase CreateDocument();
        public abstract DocumentViewModelBase CreateDocumentFromFilePath(string filepath);
        protected abstract void MatchLayoutContent(Object o, LayoutSerializationCallbackEventArgs e);

        protected WorkspaceBase()
        {
            CompositeDisposable.Add(() =>
            {
                while (Documents.Any())
                {
                    Documents.First().Dispose();
                }
            });
        }

        #region Layout
        String LayoutFile
        {
            get
            {
                return System.IO.Path.ChangeExtension(
                    Environment.GetCommandLineArgs()[0]
                    , ".AvalonDock.config"
                    );
            }
        }

        public void LoadLayout(DockingManager dockManager)
        {
            var layoutSerializer = new XmlLayoutSerializer(dockManager);
            layoutSerializer.LayoutSerializationCallback += MatchLayoutContent;
            try
            {
                layoutSerializer.Deserialize(LayoutFile);
            }
            catch (Exception ex)
            {
                DefaultLayout();
            }
        }

        abstract public void DefaultLayout();

        public void SaveLayout(DockingManager dockManager)
        {
            var serializer = new XmlLayoutSerializer(dockManager);
            using (var stream = new StreamWriter(LayoutFile))
                serializer.Serialize(stream);
        }
        #endregion

        #region DocumentsLogic
        public DocumentViewModelBase NewDocument(object parameter)
        {
            var document = CreateDocument();
            AddDocument(document);
            ActiveDocument = document;
            return document;
        }

        private void OpenDocument(object parameter)
        {
            var dlg = new OpenFileDialog();
            if (!dlg.ShowDialog().GetValueOrDefault()) return;
            var document=OpenDocumentFromFilePath(dlg.FileName);
        }

        public DocumentViewModelBase OpenDocumentFromFilePath(String filepath)
        {
            var document = m_documents.FirstOrDefault(fm => fm.FilePath == filepath);
            if (document == null)
            {
                document = CreateDocumentFromFilePath(filepath);
                if (document==null)
                {
                    return null;
                }
                AddDocument(document);
            }
            ActiveDocument = document;
            return document;
        }

        void AddDocument(DocumentViewModelBase document)
        {
            document.CompositeDisposable.Add(() =>
            {
                if (ActiveDocument == document)
                {
                    ActiveDocument = null;
                }
                m_documents.Remove(document);
            });
            m_documents.Add(document);
        }
        #endregion
    }
}
