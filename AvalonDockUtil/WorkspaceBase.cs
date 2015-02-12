using Livet.Commands;
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
using System.Xml;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace AvalonDockUtil
{
    public abstract class WorkspaceBase : ViewModelBase
    {
        #region Documents
        ObservableCollection<DocumentBase> m_documents = new ObservableCollection<DocumentBase>();
        ReadOnlyObservableCollection<DocumentBase> _readonyFiles = null;
        public ReadOnlyObservableCollection<DocumentBase> Documents
        {
            get
            {
                if (_readonyFiles == null)
                    _readonyFiles = new ReadOnlyObservableCollection<DocumentBase>(m_documents);
                return _readonyFiles;
            }
        }

        public void SaveAllDocuments()
        {
            foreach(var d in Documents)
            {
                d.ConfirmSave();
            }
        }
        #endregion

        #region ActiveDocument
        private DocumentBase _activeDocument = null;
        public DocumentBase ActiveDocument
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
        ObservableCollection<ToolContentBase> m_tools = new ObservableCollection<ToolContentBase>();
        ReadOnlyObservableCollection<ToolContentBase> _readonyTools = null;
        public ReadOnlyObservableCollection<ToolContentBase> Tools
        {
            get
            {
                if (_readonyTools == null)
                    _readonyTools = new ReadOnlyObservableCollection<ToolContentBase>(m_tools);
                return _readonyTools;
            }
        }

        protected void ClearTools()
        {
            m_tools.Clear();
        }

        protected void AddTool(ToolContentBase tool)
        {
            m_tools.Add(tool);
        }
        #endregion

        ViewModelCommand m_newDocumentCommand;
        public ICommand NewDocumentCommand
        {
            get
            {
                if (m_newDocumentCommand == null) m_newDocumentCommand = new ViewModelCommand(() => NewDocument());
                return m_newDocumentCommand;
            }
        }

        ViewModelCommand m_openDocumentCommand;
        public ICommand OpenDocumentCommand
        {
            get
            {
                if (m_openDocumentCommand == null) m_openDocumentCommand = new ViewModelCommand(() => OpenDocument());
                return m_openDocumentCommand;
            }
        }

        public abstract DocumentBase CreateDocument();
        public abstract DocumentBase CreateDocumentFromFilePath(string filepath);
        protected abstract void InitializeTools();

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
        Byte[] m_defaultLayout;

        public void LoadDefaultLayout(DockingManager dockManager)
        {
            LoadLayout(dockManager, m_defaultLayout);
        }

        public void LoadLayout(DockingManager dockManager)
        {
            // backup default layout
            using (var ms = new MemoryStream())
            {
                var serializer = new XmlLayoutSerializer(dockManager);
                serializer.Serialize(ms);
                m_defaultLayout = ms.ToArray();
            }

            // load file
            Byte[] bytes;
            try
            {
                bytes = System.IO.File.ReadAllBytes(LayoutFile);
            }
            catch (FileNotFoundException ex)
            {
                return;
            }

            // restore layout
            if (!LoadLayout(dockManager, bytes))
            {
                return;
            }
        }

        void MatchLayoutContent(object o, LayoutSerializationCallbackEventArgs e)
        {
            if (e.Model is LayoutAnchorable)
            {
                // Tool Windows
                foreach (var tool in Tools)
                {
                    if (tool.ContentId == e.Model.ContentId)
                    {
                        e.Content = tool;
                        return;
                    }
                }

                // Unknown
                ErrorDialog(new Exception("unknown ContentID: " + e.Model.ContentId));
                return;
            }

            if (e.Model is LayoutDocument)
            {
                // load済みを探す
                foreach (var document in Documents)
                {
                    if (document.ContentId == e.Model.ContentId)
                    {
                        e.Content = document;
                        return;
                    }
                }

                // Document
                var path = e.Model.ContentId;
                if (!String.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    var document = OpenDocumentFromFilePath(path);
                    e.Content = document;
                }
                else
                {
                    var document = NewDocument();
                    e.Content = document;
                }
                return;
            }

            ErrorDialog(new Exception("Unknown Model: " + e.Model.GetType()));
            return;
        }

        bool LoadLayout(DockingManager dockManager, Byte[] bytes)
        {
            InitializeTools();

            var serializer = new XmlLayoutSerializer(dockManager);

            serializer.LayoutSerializationCallback += MatchLayoutContent;

            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    serializer.Deserialize(stream);
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorDialog(ex);
                return false;
            }
        }

        public void SaveLayout(DockingManager dockManager)
        {
            var serializer = new XmlLayoutSerializer(dockManager);
            var doc = new XmlDocument();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream);
                stream.Position = 0;
                doc.Load(stream);
            }

            using (var stream = new FileStream(LayoutFile, FileMode.Create))
            {
                doc.Save(stream);
            }
        }
        #endregion

        #region DocumentsLogic
        public DocumentBase NewDocument()
        {
            var document = CreateDocument();
            AddDocument(document);
            ActiveDocument = document;
            return document;
        }

        public DocumentBase OpenDocument()
        {
            var response = OpenDialog("Open");
            if (response == null)
            {
                return null;
            }
            var document=OpenDocumentFromFilePath(response[0]);
            ActiveDocument = document;
            return document;
        }

        public DocumentBase OpenDocumentFromFilePath(String filepath)
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

        private void AddDocument(DocumentBase document)
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
