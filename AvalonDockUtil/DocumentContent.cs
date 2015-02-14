using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AvalonDockUtil;
using System.ComponentModel;

namespace AvalonDockUtil
{
    public class DocumentContent: INotifyPropertyChanged
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

        Guid m_guid;
        public Guid Guid
        {
            get { return m_guid; }
            set
            {
                if (m_guid == value) return;
                m_guid = value;
                RaisePropertyChanged("Guid");
                RaisePropertyChanged("ContentId");
            }
        }

        [ContentProperty]
        public string ContentId
        {
            get { return m_guid.ToString(); }
            set
            {
                var guid = Guid.Parse(value);
                Guid = guid;
            }
        }

        string m_title;
        [ContentProperty]
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
            m_guid = Guid.Parse(contentId);
        }
    }
}
