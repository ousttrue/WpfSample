using AvalonDockUtil;
using Livet.Messaging;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;


namespace AvalonDockMVVMSample
{
    public class MainWindowViewModel : WorkspaceBase
    {
        public override AvalonDockUtil.DocumentContent NewDocument()
        {
            return new UriListDocument(this.Messenger);
        }

        RelayCommand m_openDocumentCommand;
        public ICommand OpenDocumentCommand
        {
            get
            {
                if (m_openDocumentCommand == null)
                {
                    m_openDocumentCommand = new RelayCommand(() =>
                    {
                        var document = NewDocument() as UriListDocument;
                        if (!document.ViewModel.Open())
                        {
                            return;
                        }
                        Documents.Add(document);
                    });
                }
                return m_openDocumentCommand;
            }
        }

        protected override void ModifySerializedXml(System.Xml.XmlDocument doc)
        {
            var nodes = doc.GetElementsByTagName("LayoutDocument");
            for (int i = 0; i < nodes.Count; ++i)
            {
                var node = nodes[i];
                var contentId = node.Attributes["ContentId"].Value;
                var document = GetDocumentByContentId(contentId) as UriListDocument;
                if (document != null && !String.IsNullOrEmpty(document.ViewModel.Path))
                {
                    // documentのファイルパスを追記する
                    var attrib = doc.CreateAttribute("FilePath");
                    attrib.Value = document.ViewModel.Path;
                    node.Attributes.Append(attrib);
                }
            }
        }

        protected override void RestoreDocumentsFromBytes(Byte[] bytes)
        {
            // 独自にxmlを解析する
            using (var stream = new MemoryStream(bytes))
            {
                var doc = new XmlDocument();
                doc.Load(stream);
                // ContentIDが"Document"のIDを探す
                var nodes = doc.GetElementsByTagName("LayoutDocument");
                for (int i = 0; i < nodes.Count; ++i)
                {
                    var node = nodes[i];

                    var document = GetDocumentByContentId(node.Attributes["ContentId"].Value) as UriListDocument;
                    if (document != null)
                    {
                        var viewModel = document.ViewModel;
                        foreach (XmlAttribute attrib in node.Attributes)
                        {
                            if (attrib.Name == "FilePath")
                            {
                                viewModel.Path = attrib.Value;
                                viewModel.Load();
                            }
                        }
                    }
                }
            }
        }

        public void SaveAllDocuments()
        {
            foreach(UriListDocument d in Documents)
            {
                if (d.ViewModel.IsDirty)
                {
                    d.ViewModel.Save(false);
                }
            }
        }

        protected override void InitializeTools()
        {
            Tools.Clear();
            Tools.Add(new MessageToolContent());
            Tools.Add(new StatusToolContent());
        }
    }
}
