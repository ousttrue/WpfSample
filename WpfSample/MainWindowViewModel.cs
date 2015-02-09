using Codeplex.Reactive;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfSample
{
    class MainWindowViewModel: Livet.ViewModel
    {
        String m_text;
        public String Text
        {
            get { return m_text; }
            private set {
                if (m_text == value) return;
                m_text = value;
                RaisePropertyChanged("Text");
            }
        }

        public ReactiveCommand UriDropCommand { get; private set; }
        void UriDrop(Object arg)
        {
            var urilist = (IEnumerable<Uri>)arg;

            Text = "Droped files is ...\n" + String.Join("\n", urilist);

            Messenger.Raise(new InformationMessage(Text, "Dropped", MessageBoxImage.Information, "Info"));
        }

        public ListenerCommand<OpeningFileSelectionMessage> OpenCommand { get; private set; }
        void Open(OpeningFileSelectionMessage m)
        {
            if (m.Response == null)
            {
                Messenger.Raise(new InformationMessage("Cancel", "Error", MessageBoxImage.Error, "Info"));
                return;
            }

            UriDrop(m.Response.Select(f => new Uri(f)));
        }

        public MainWindowViewModel()
        {
            Text = "Drop File !";

            UriDropCommand = new ReactiveCommand();
            UriDropCommand.Subscribe(UriDrop);

            OpenCommand = new ListenerCommand<OpeningFileSelectionMessage>(Open, () => true);
        }
    }
}
