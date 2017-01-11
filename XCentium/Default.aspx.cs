using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using HtmlAgilityPack;

namespace XCentium
{
    public partial class _Default : Page
    {
        protected void ScrapeUrl(object sender, EventArgs e)
        {
            // Clear output controls
            outputError.Visible = false;
            outputError.InnerHtml = string.Empty;
            outputWordList.InnerHtml = string.Empty;
            carouselPanel.Visible = false;

            // Set url from text box
            string inputUrl = urlInput.Text;

            // Add protocol if missing
            if (!inputUrl.Contains(@"http://") && !inputUrl.Contains(@"https://"))
            {
                // If no protocol specified, use the protocol the client used to access this site
                inputUrl = Request.Url.Scheme + @"://" + inputUrl;
            }

            // Make sure the URL is valid
            try
            {
                // Only pull HEAD data for speed
                WebRequest request = WebRequest.Create(inputUrl);
                request.Method = "HEAD";
                request.GetResponse();
            }
            catch (Exception ex)
            {
                // Explain issue with URL in Error
                outputError.InnerHtml = "URL does not resolve: " + inputUrl;
                // Allow debugMode to show full error information
                if (Request.QueryString["debugMode"] == "true")
                {
                    outputError.InnerHtml += "</br></br>" + ex.Message + "</br>" + ex.StackTrace;
                }
                outputError.Visible = true;
                return;
            }

            // Create Uri from provided url input
            Uri url = new Uri(inputUrl);

            // Initialize a WebClient
            WebClient client = new WebClient();

            // Download page Html
            string html = client.DownloadString(url);

            // Load the Html into HtmlAgilityPack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            FindImages(url, doc);
            FindWords(doc);
        }

        private void FindImages(Uri url, HtmlDocument doc)
        {
            Func<string, string> getSrc = src =>
            {
                // Check to see if img path is relative
                if (src[0] == '~')
                {
                    // Prefix with url if needed and return src
                    src = url.Scheme + @"://" + url.Host + "/" + src;
                    return src;
                }
                if (src[0] == '/')
                {
                    // Check to see if the image path is Url Scheme agnostic
                    if (src[1] == '/')
                    {
                        // Add appropriate url scheme and return src
                        src = url.Scheme + @":" + src;
                        return src;
                    }
                    // Prefix with url if needed and return src
                    src = url.Scheme + @"://" + url.Host + src;
                    return src;
                }

                // Return src if unmodified
                return src;
            };

            if (doc.DocumentNode.SelectNodes("//img") == null)
            {
                // Explain null image issue in Error
                outputError.InnerHtml = "No Images Found.</br>";
                outputError.Visible = true;
                return;
            }

            Func<HtmlNode, string> checkAlt = alt =>
                {
                    // Make sure alt isn't null.
                    if (alt.Attributes["alt"] != null)
                    {
                        return alt.Attributes["alt"].Value;
                    }
                    return string.Empty;
                };

            // Clean document of script tags to remove marketing/tracking pixels
            doc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "noscript")
                .ToList()
                .ForEach(n => n.Remove());

            // Use LINQ to get all Images
            var foundImages = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                               where (node.Attributes["src"] != null)
                               select new
                               {
                                   src = getSrc(node.Attributes["src"].Value),
                                   alt = checkAlt(node)
                               });

            indicators.DataSource = foundImages;
            indicators.DataBind();

            images.DataSource = foundImages;
            images.DataBind();

            // Show carousel if images are found
            if (foundImages.Count() > 0)
            {
                carouselPanel.Visible = true;
            }
        }

        private void FindWords(HtmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();

            if (doc.DocumentNode.SelectNodes("//body//*[not(self::script)]/text()") == null)
            {
                // Explain null image issue in Error
                outputError.InnerHtml = "No Body and/or Text Found.</br>";
                outputError.Visible = true;
                return;
            }

            // Parse Html for all text nodes in body tag ignoring scripts
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//body//*[not(self::script)]/text()"))
            {
                // Use HtmlDecode to decode any encoded characters and shift everything to lowercase to ensure proper comparisons later
                sb.Append(WebUtility.HtmlDecode(node.InnerText.ToLower() + " "));
            }
                       
            string cleanedOutput = sb.ToString();

            // Find wordcount 
            outputWordList.InnerHtml += "Word Count: " + Regex.Matches(cleanedOutput, @"[A-Za-z0-9]+").Count.ToString() + "</br></br>";

            if (outputWordList.InnerText.Count() > 0)
            {
                outputWordList.Visible = true;
            }

            // Use LINQ to find 10 most frequently used words and their usage count
            var wordRankings = Regex.Split(cleanedOutput, @"\W+")
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new
                {
                    key = g.Key,
                    count = g.Count()
                }).ToList();

            //Create Table, Row, and Cell controls
            HtmlTable table = new HtmlTable();
            table.Attributes.Add("class", "results-table");
            HtmlTableRow row;
            HtmlTableCell cell;

            // Add header values to the table
            row = new HtmlTableRow();
            cell = new HtmlTableCell();
            row.Attributes.Add("class", "table-row-header");
            cell.InnerText = "Count";
            row.Cells.Add(cell);
            cell = new HtmlTableCell();
            cell.InnerText = "Word";
            row.Cells.Add(cell);
            table.Rows.Add(row);
        
            // Fill the table with the results
            foreach (var word in wordRankings)
            {
                row = new HtmlTableRow();
                cell = new HtmlTableCell();
                cell.InnerText = word.count.ToString();
                row.Cells.Add(cell);
                cell = new HtmlTableCell();
                cell.InnerText = word.key;
                row.Cells.Add(cell);
                table.Rows.Add(row);
            }
            // Add the table to the placeholder
            phResultsTable.Controls.Add(table);
        }

        protected string GetItemClass(int index)
        {
            // Return "active" class for first item in carousel
            if (index == 0)
            {
                return "active";
            }
            // Return nothing for remaining items
            return string.Empty;
        }
    }
}