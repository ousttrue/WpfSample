using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Reactive.Linq;
using AvalonDockUtil;
using System.ComponentModel;

namespace AvalonDockMVVMSample
{
    public class DocumentContent: IDocumentContent, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(String prop)
        {
            var tmp = PropertyChanged;
            if (tmp != null)
            {
                tmp(this, new PropertyChangedEventArgs(prop));
            }
        }

        string m_ContentId;
        [ContentProperty]
        public string ContentId
        {
            get { return m_ContentId; }
        }

        string m_title;
        public string Title
        {
            get { return m_title; }
            set
            {
                if (m_title == value) return;
                m_title = value;
                RaisePropertyChanged("Title");
            }
        }

        public DocumentContent()
            : this(Guid.NewGuid().ToString())
        {
            Title = "Untitled";
        }

        public DocumentContent(String contentId)
        {
            m_ContentId = contentId;
        }
    }
}
