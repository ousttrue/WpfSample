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

        protected override void InitializeTools()
        {
            ClearTools();
            AddTool(new StatusToolViewModel());
            AddTool(new MessageToolViewModel());
        }
    }
}
