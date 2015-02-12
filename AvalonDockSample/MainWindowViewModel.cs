using System;
using System.IO;
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
            var serializer = new XmlLayoutSerializer(dockManager);
            try
            {
                using (var stream = new StreamReader(LayoutFile))
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
        }

        public void SaveLayout(DockingManager dockManager)
        {
            var serializer = new XmlLayoutSerializer(dockManager);
            using (var stream = new StreamWriter(LayoutFile))
            {
                serializer.Serialize(stream);
            }
        }
    }
}
