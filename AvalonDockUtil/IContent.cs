using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonDockUtil
{
    public interface IContent
    {
        String ContentId { get; }
    }

    public interface IDocumentContent: IContent
    {

    }

    public interface IToolContent: IContent
    {

    }
}
