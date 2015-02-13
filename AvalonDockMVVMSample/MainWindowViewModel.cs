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
    public class MainWindowViewModel : AvalonDockUtil.WorkspaceBase
    {
        protected override void InitializeTools()
        {
            Tools.Clear();
            Tools.Add(new MessageToolContent());
            Tools.Add(new StatusToolContent());
        }
    }
}
