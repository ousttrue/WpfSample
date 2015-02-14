using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Xml;
using System.Linq;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace AvalonDockUtil
{
    public abstract class WorkspaceBase: ViewModelBase
    {
        ObservableCollection<DocumentContent> m_documents;
        public ObservableCollection<DocumentContent> Documents
        {
            get
            {
                if (m_documents == null) m_documents = new ObservableCollection<DocumentContent>();
                return m_documents;
            }
        }

        private DocumentContent m_activeDocument;
        public DocumentContent ActiveDocument
        {
            get
            {
                return m_activeDocument;
            }
            set
            {
                if (m_activeDocument == value) return;
                m_activeDocument = value;
                RaisePropertyChanged("ActiveDocument");
            }
        }

        ObservableCollection<ToolContent> m_tools;
        public ObservableCollection<ToolContent> Tools
        {
            get
            {
                if (m_tools == null) m_tools = new ObservableCollection<ToolContent>();
                return m_tools;
            }
        }

        protected abstract void InitializeTools();

        protected DocumentContent GetDocumentByContentId(String contentId)
        {
            return Documents.FirstOrDefault(d => d.ContentId == contentId);
        }

        RelayCommand m_newDocumentCommand;
        public ICommand NewDocumentCommand
        {
            get
            {
                if (m_newDocumentCommand == null)
                {
                    m_newDocumentCommand = new RelayCommand(() =>
                    {
                        var document=NewDocument();
                        Documents.Add(document);
                    });
                }
                return m_newDocumentCommand;
            }
        }

        public virtual DocumentContent NewDocument()
        {
            return new DocumentContent();
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

        public void DefaultLayout(DockingManager dockManager)
        {
            LoadLayoutFromBytes(dockManager, m_defaultLayout);
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
            LoadLayoutFromBytes(dockManager, bytes);
        }

        bool LoadLayoutFromBytes(DockingManager dockManager, Byte[] bytes)
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

                RestoreDocumentsFromBytes(bytes);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        void MatchLayoutContent(object o, LayoutSerializationCallbackEventArgs e)
        {
            var contentId = e.Model.ContentId;

            if (e.Model is LayoutAnchorable)
            {
                // Tool Windows
                foreach (var tool in Tools)
                {
                    if (tool.ContentId == contentId)
                    {
                        e.Content = tool;
                        return;
                    }
                }

                // Unknown
                ErrorDialog(new Exception("unknown ContentID: " + contentId));
                return;
            }

            if (e.Model is LayoutDocument)
            {
                // load済みを探す
                foreach (var document in Documents)
                {
                    if (document.ContentId == contentId)
                    {
                        e.Content = document;
                        return;
                    }
                }

                // Document
                {
                    var document = NewDocument();
                    Documents.Add(document);
                    document.ContentId = contentId;
                    e.Content = document;
                }

                return;
            }

            ErrorDialog(new Exception("Unknown Model: " + e.Model.GetType()));
            return;
        }

        protected abstract void ModifySerializedXml(XmlDocument doc);
        protected abstract void RestoreDocumentsFromBytes(Byte[] bytes);

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

            ModifySerializedXml(doc);

            using (var stream = new FileStream(LayoutFile, FileMode.Create))
            {
                doc.Save(stream);
            }
        }
        #endregion
    }
}
