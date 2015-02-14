using Livet.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonDockMVVMSample
{
    public class UriListDocument : AvalonDockUtil.DocumentContent
    {
        UriListViewModel.UriListViewModel m_viewModel;
        public UriListViewModel.UriListViewModel ViewModel
        {
            get{
                return m_viewModel;
            }
        }

        public UriListDocument(InteractionMessenger messenger)
        {
            m_viewModel = new UriListViewModel.UriListViewModel(messenger);
            m_viewModel.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "Path")
                {
                    Title = System.IO.Path.GetFileName(m_viewModel.Path);
                }
            };
        }
    }
}
