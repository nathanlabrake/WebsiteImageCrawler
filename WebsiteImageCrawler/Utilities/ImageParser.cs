using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebsiteImageCrawler.Utilities
{
    public class ImageParser
    {
        public ImageParser()
        {

        }

        public HashSet<string> GetImages(string content)
        {
            HashSet<string> imageList = new HashSet<string>();
            Regex rx = new Regex(@"((https?:)?//)?[^'""<>\s;:]+?\.(jpg|jpeg|gif|png|svg|webp|bmp)");

            if (rx.IsMatch(content))
            {
                foreach (Match match in rx.Matches(content))
                {
                    imageList.Add(match.Value);
                }
            }

            return imageList;
        }
    }
}
