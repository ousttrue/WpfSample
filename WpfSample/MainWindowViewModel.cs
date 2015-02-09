using Codeplex.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public MainWindowViewModel()
        {
            Text = "Drop File !";

            UriDropCommand = new ReactiveCommand();
            UriDropCommand.Subscribe(UriDrop);
        }
    }
}
