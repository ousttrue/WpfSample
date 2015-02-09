using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSample
{
    class MainWindowViewModel
    {
        public String Text { get; private set; }

        public MainWindowViewModel()
        {
            Text = "Hello !";
        }
    }
}
