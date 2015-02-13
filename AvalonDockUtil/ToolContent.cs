using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.AvalonDock.Converters;

namespace AvalonDockUtil
{
    public class ToolContent: INotifyPropertyChanged
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

        string m_contentId;
        [ContentProperty]
        public string ContentId
        {
            get { return m_contentId; }
            set
            {
                if (m_contentId == value) return;
                m_contentId = value;
                RaisePropertyChanged("ContentId");
            }
        }

        String m_title;
        [ContentProperty]
        public String Title
        {
            get { return m_title; }
            set
            {
                if (m_title == value) return;
                m_title = value;
                RaisePropertyChanged("Title");
            }
        }

        Visibility m_visiblity;
        [ContentProperty(BindingMode=BindingMode.TwoWay)]
        public Visibility Visibility
        {
            get { return m_visiblity; }
            set
            {
                if (m_visiblity == value) return;
                m_visiblity = value;
                RaisePropertyChanged("Visibility");
                RaisePropertyChanged("IsVisible");
            }
        }

        public bool IsVisible
        {
            get { return m_visiblity == System.Windows.Visibility.Visible; }
            set
            {
                if (IsVisible == value)return;
                if (value)
                {
                    Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        public ToolContent(String contentId, String title=null)
        {
            m_contentId = contentId;
            Title = String.IsNullOrEmpty(title) ? contentId : title;
        }
    }
}
