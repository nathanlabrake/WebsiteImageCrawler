using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteImageCrawler.Store
{
    public interface IListStore
    {
        public Task Store(HashSet<string> list, Uri pageUri);
    }
}
