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
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;


namespace AvalonDockMVVMSample
{
    public class MainWindowViewModel: AvalonDockUtil.WorkspaceBase
    {
        public MainWindowViewModel()
        {
        }

        public override AvalonDockUtil.DocumentBase CreateDocument()
        {
            return new DocumentViewModel();
        }

        public override AvalonDockUtil.DocumentBase CreateDocumentFromFilePath(string filepath)
        {
            var document=new DocumentViewModel();
            document.FilePath = filepath;
            document.Load();
            return document;
        }

        protected override void MatchLayoutContent(object o, LayoutSerializationCallbackEventArgs e)
        {
            foreach (var tool in Tools)
            {
                if (e.Model.ContentId == tool.ContentId)
                {
                    e.Content = tool;
                    return;
                }
            }

            if(e.Model is LayoutDocument)
            {
                var path = e.Model.ContentId;
                if (!String.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    var document = OpenDocumentFromFilePath(path);
                    e.Content = document;
                }
                else
                {
                    var document=NewDocument();
                    e.Content = document;
                }
                return;
            }

            ErrorDialog(new Exception("Unknown ContentID: "+e.Model.ContentId));
        }

        protected override void InitializeTools()
        {
            ClearTools();
            AddTool(new StatusToolViewModel());
            AddTool(new MessageToolViewModel());
        }
    }
}
