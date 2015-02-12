using System;
using System.IO;
using System.Xml;
using System.Linq;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

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

        public void LoadLayout(DockingManager dockManager)
        {
            var bytes = System.IO.File.ReadAllBytes(LayoutFile);
            
            var serializer = new XmlLayoutSerializer(dockManager);
            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    serializer.Deserialize(stream);
                }
            }
            catch (FileNotFoundException ex)
            {

            }
            catch (Exception ex)
            {
                ErrorDialog(ex);
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
