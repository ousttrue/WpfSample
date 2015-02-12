
using System.Windows.Media;
namespace AvalonDockUtil
{
    public class ToolContentBase : ViewModelBase, IPaneContent
    {
        #region IsVisible
        bool m_isVisible = true;
        public bool IsVisible
        {
            get { return m_isVisible; }
            set
            {
                if (m_isVisible == value) return;
                m_isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }
        #endregion

        #region Document
        DocumentBase m_document;
        public DocumentBase Document
        {
            get { return m_document; }
            set
            {
                if(m_document==value)return;
                m_document = value;
                RaisePropertyChanged("Document");
            }
        }
        #endregion

        #region Title
        string m_title;
        public string Title
        {
            get { return m_title; }
            set
            {
                if (m_title == value) return;
                m_title = value;
                ContentId = "tool:" + Title;
                RaisePropertyChanged("Title");
            }
        }
        #endregion

        #region ContentId
        string m_contentId;
        public string ContentId
        {
            get { return m_contentId; }
            private set
            {
                if (m_contentId == value) return;
                m_contentId = value;
                RaisePropertyChanged("ContentId");
            }
        }
        #endregion

        #region IsSelected
        bool m_isSelected;
        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if (m_isSelected == value) return;
                m_isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        #endregion

        #region IsActive
        bool m_isActive;
        public bool IsActive
        {
            get { return m_isActive; }
            set
            {
                if (m_isActive == value) return;
                m_isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }
        #endregion

        #region IconSource
        ImageSource m_iconSource;
        public ImageSource IconSource
        {
            get { return null; }
        }
        #endregion

        public ToolContentBase(string title)
        {
            Title = title;
        }
    }
}
