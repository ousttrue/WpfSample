
namespace AvalonDockUtil
{
    public class ToolViewModelBase : PaneViewModelBase
    {
        #region IsVisible
        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged("IsVisible");
                }
            }
        }
        #endregion

        #region Document
        DocumentViewModelBase m_document;
        public DocumentViewModelBase Document
        {
            get { return m_document; }
            set
            {
                if (m_document == value) return;
                m_document = value;
                RaisePropertyChanged("Document");
            }
        }
        #endregion

        public override string ContentId
        {
            get { return "tool:" + Title; }
        }

        public ToolViewModelBase(string title)
        {
            Title = title;
        }
    }
}
