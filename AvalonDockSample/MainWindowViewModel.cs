using System;
using System.IO;
using System.Xml;
using System.Linq;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Xceed.Wpf.AvalonDock.Layout;

namespace AvalonDockSample
{
    public class MainWindowViewModel: UriListViewModel.UriListViewModel
    {
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

        LayoutAnchorable m_tool;
        public LayoutAnchorable Tool
        {
            get { return m_tool; }
            set
            {
                if (m_tool == value) return;
                m_tool = value;
                RaisePropertyChanged("Tool");
            }
        }

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

            // restore layout
            Byte[] bytes;
            try
            {
                bytes = System.IO.File.ReadAllBytes(LayoutFile);
            }
            catch (FileNotFoundException ex)
            {
                return;
            }

            if(!LoadLayout(dockManager, bytes)){
                return;
            }

            // 独自にxmlを解析する
            using (var stream = new MemoryStream(bytes))
            {
                var doc = new XmlDocument();
                doc.Load(stream);
                // ContentIDが"Document"のIDを探す
                var documents = doc.SelectNodes("//*[@ContentId=\"Document\"]");
                if (documents.Count > 0)
                {
                    var document = documents[0];
                    foreach (XmlAttribute attrib in document.Attributes)
                    {
                        if (attrib.Name == "FilePath")
                        {
                            Path = attrib.Value;
                            Load();
                            break;
                        }
                    }
                }
            }
        }

        bool LoadLayout(DockingManager dockManager, Byte[] bytes, EventHandler<LayoutSerializationCallbackEventArgs> callback=null)
        {           
            var serializer = new XmlLayoutSerializer(dockManager);

            serializer.LayoutSerializationCallback += (o, e) =>
            {
                if (e.Model.ContentId == "Tool")
                {
                    // シリアライズされて新しくできたLayoutAnchorableを保存しとく
                    Tool = (LayoutAnchorable)e.Model;
                }
            };

            if(callback!=null){
                serializer.LayoutSerializationCallback += callback;
            }
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
            var doc=new XmlDocument();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream);
                stream.Position = 0;
                doc.Load(stream);
            }

            if(!String.IsNullOrEmpty(Path)){
                // ContentIDが"Document"のIDを探す
                var documents=doc.SelectNodes("//*[@ContentId=\"Document\"]");
                if (documents.Count > 0)
                {
                    // documentのファイルパスを追記する
                    var document = documents[0];

                    var attrib=doc.CreateAttribute("FilePath");
                    attrib.Value=Path;
                    document.Attributes.Append(attrib);
                }
            }           

            using (var stream = new FileStream(LayoutFile, FileMode.Create))
            {
                doc.Save(stream);
            }
        }
    }
}
