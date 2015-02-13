using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Xml;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace AvalonDockUtil
{
    public abstract class WorkspaceBase: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(String prop)
        {
            var tmp = PropertyChanged;
            if (tmp != null)
            {
                tmp(this, new PropertyChangedEventArgs(prop));
            }
        }

        protected virtual void ErrorMessage(Exception ex)
        {
            Console.WriteLine(ex);
        }

        protected abstract IDocumentContent OpenDocument(String contentId);

        ObservableCollection<IDocumentContent> m_documents;
        public ObservableCollection<IDocumentContent> Documents
        {
            get
            {
                if (m_documents == null) m_documents = new ObservableCollection<IDocumentContent>();
                return m_documents;
            }
        }

        private IDocumentContent m_activeDocument;
        public IDocumentContent ActiveDocument
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

        ObservableCollection<IToolContent> m_tools;
        public ObservableCollection<IToolContent> Tools
        {
            get
            {
                if (m_tools == null) m_tools = new ObservableCollection<IToolContent>();
                return m_tools;
            }
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

        bool LoadLayout(DockingManager dockManager, Byte[] bytes)
        {
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
                ErrorMessage(new Exception("unknown ContentID: " + contentId));
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
                e.Content = OpenDocument(contentId);

                return;
            }

            ErrorMessage(new Exception("Unknown Model: " + e.Model.GetType()));
            return;
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
    }
}
