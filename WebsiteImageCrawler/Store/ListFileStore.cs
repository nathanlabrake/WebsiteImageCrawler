using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteImageCrawler.Store
{
    public class ListFileStore : IListStore
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        string FilePath;

        public ListFileStore(string filePath)
        {
            FilePath = filePath;
        }

        public async Task Store(HashSet<string> list, Uri pageUri)
        {
            semaphore.Wait();
            try
            {
                // It's safe for this thread to access from the shared resource.
                using (StreamWriter file = new(FilePath, append: true))
                {
                    await file.WriteLineAsync(
                        pageUri.AbsoluteUri + "\n\t" +
                        string.Join("\n\t", list) +
                        "\n"
                    );

                    file.Close();
                }
            }
            finally
            {
                // Release semaphore
                semaphore.Release();
            }

        }
    }
}
