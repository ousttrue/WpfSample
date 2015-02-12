using System.Windows.Media;

namespace AvalonDockUtil
{
    public interface IPaneContent
    {
        string Title { get; }
        string ContentId { get; }
        bool IsSelected { get; set; }
        bool IsActive { get; set; }
        ImageSource IconSource { get; }
    }
}
