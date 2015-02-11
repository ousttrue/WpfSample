using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using Livet.Messaging;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;


namespace WpfSample
{
    public class MainWindowViewModel: AvalonDockUtil.WorkspaceBase
    {
        public MainWindowViewModel()
        {
        }

        public override void DefaultLayout()
        {
            AddTool(new StatusToolViewModel());
        }

        public override AvalonDockUtil.DocumentViewModelBase CreateDocument()
        {
            return new DocumentViewModel();
        }

        public override AvalonDockUtil.DocumentViewModelBase CreateDocumentFromFilePath(string filepath)
        {
            var document=new DocumentViewModel();
            document.Load(filepath);
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
            //throw new NotImplementedException();
        }
    }
}
