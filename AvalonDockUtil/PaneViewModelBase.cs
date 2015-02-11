using System.Windows.Media;

namespace AvalonDockUtil
{
    public abstract class PaneViewModelBase : Livet.ViewModel
    {
        #region Title
        private string _title = null;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value) return;
                _title = value;
                RaisePropertyChanged("Title");
            }
        }
        #endregion

        #region ContentId
        public abstract string ContentId { get; }
        #endregion

        #region IsSelected
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        #endregion

        #region IsActive
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }
        #endregion

        public ImageSource IconSource
        {
            get;
            protected set;
        }

        public PaneViewModelBase()
        { }
    }
}
