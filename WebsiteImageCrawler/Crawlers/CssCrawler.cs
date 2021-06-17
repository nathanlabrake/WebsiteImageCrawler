using ExCSS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebsiteImageCrawler.Utilities;

namespace WebsiteImageCrawler.Crawlers
{
    public class CssCrawler
    {
        public CssCrawler()
        {
        }

        public HashSet<string> Crawl(string cssContent)
        {
            HashSet<string> imageList = new HashSet<string>();
            var parser = new StylesheetParser(
                includeUnknownRules: true, 
                includeUnknownDeclarations: true, 
                tolerateInvalidSelectors: true, 
                tolerateInvalidValues: true, 
                tolerateInvalidConstraints: true
            );
            var stylesheet = parser.Parse(cssContent);

            ImageParser imageParser = new ImageParser();

            if(stylesheet != null && stylesheet.StyleRules.Any())
            {
                foreach (IStyleRule rule in stylesheet.StyleRules)
                {
                    foreach (IProperty line in rule.Style)
                    {
                        imageList.UnionWith(imageParser.GetImages(line.Value));
                    }
                }
            }

            return imageList;
        }
    }
}
