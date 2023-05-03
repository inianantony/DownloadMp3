using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using RestSharp;

namespace DownloadMp3
{
    /// <summary>
    /// GET SESSION ID BY VISITING THE HOME LINK "https://tamilmp3web.net/" FROM BROWSER
    /// click any movie and click any song
    /// on the Download 320Kbps link you can see the session id when you mouse over
    /// </summary>
    public class Tamilmp3WebDownload
    {
        private static CookieContainer cookieContainer = new();
        private static HttpClientHandler handler = new() { CookieContainer = cookieContainer };
        private static HttpClient client = new(handler);
        private static string SessionId = "cb56a03c";

        public async Task ProcessDownloadAsync()
        {
            var preDownloadedFullFolder = System.IO.Directory.GetDirectories("C:\\Users\\inian\\Downloads\\2022 Hits Check");
            var preDownloaded = preDownloadedFullFolder.Select(a => a.Replace("C:\\Users\\inian\\Downloads\\2022 Hits Check\\", string.Empty));
            var homePath = "https://tamilmp3web.net";
            var pagePath = $"{homePath}/tamil-2023-songs/?page=";
            for (int page = 1; page <= 4; page++)
            {
                var url = pagePath + page;
                string responseData = await GetResponse(url);
                var selector = ".f a";
                var elements = await GetElements(responseData, selector);

                List<(string movieName, string moviePath)> moviePaths = new List<(string, string)>();
                foreach (var item in elements)
                {
                    var moviePath = item.GetAttribute("href");
                    var movieName = item.TextContent;
                    moviePaths.Add((movieName, $"{homePath}{moviePath}"));
                }

                foreach (var movieEntry in moviePaths)
                {
                    Console.WriteLine($"{DateTime.Now:hh:mm:ss} Processing movie {movieEntry}");

                    List<(string mp3Name, string mp3Path)> mp3Paths = new List<(string, string)>();

                    url = movieEntry.moviePath;
                    responseData = await GetResponse(url);
                    selector = ".f a";
                    elements = await GetElements(responseData, selector);
                    foreach (var item in elements)
                    {
                        var mp3Path = item.GetAttribute("href");
                        var mp3Name = item.TextContent;
                        mp3Paths.Add((mp3Name, $"{homePath}{mp3Path}"));
                    }

                    foreach (var mp3Entry in mp3Paths)
                    {
                        Console.WriteLine($"{DateTime.Now:hh:mm:ss} Processing movie {movieEntry} and mp3 {mp3Entry.mp3Name}");

                        url = mp3Entry.mp3Path;
                        responseData = await GetResponse(url);
                        selector = ".dlink a";
                        elements = await GetElements(responseData, selector);

                        var mp3DownloadLinks = elements.Where(a => a.InnerHtml.Contains("320Kbps")).ToArray();
                        if (mp3DownloadLinks.Length == 1)
                        {
                            if (File.Exists($"C:\\Users\\inian\\Downloads\\2022 Hits Check\\{movieEntry.movieName}\\{mp3Entry.mp3Name}.mp3"))
                            {
                                Console.WriteLine($"{DateTime.Now:hh:mm:ss} Skipping as its previously downloaded in 2022 Hits Check  for movie {movieEntry} and mp3 {mp3Entry.mp3Name}");
                                continue;
                            }

                            if (File.Exists($"C:\\Users\\inian\\Downloads\\2022 Hits\\{movieEntry.movieName}\\{mp3Entry.mp3Name}.mp3"))
                            {
                                Console.WriteLine($"{DateTime.Now:hh:mm:ss} Skipping as its previously downloaded in 2022 Hits for movie {movieEntry} and mp3 {mp3Entry.mp3Name}");
                                continue;
                            }

                            if (File.Exists($"C:\\Users\\inian\\Downloads\\ToDelete\\{mp3Entry.mp3Name}.mp3"))
                            {
                                Console.WriteLine($"{DateTime.Now:hh:mm:ss} Skipping as its previously downloaded in ToDelete for movie {movieEntry} and mp3 {mp3Entry.mp3Name}");
                                continue;
                            }

                            var filePath = $"{movieEntry.movieName}\\{mp3Entry.mp3Name}.mp3";
                            if (File.Exists(filePath))
                            {
                                Console.WriteLine($"{DateTime.Now:hh:mm:ss} Skipping for movie {movieEntry} and mp3 {mp3Entry.mp3Name}");
                                continue;
                            }

                            try
                            {
                                if (!Directory.Exists(movieEntry.movieName))
                                    Directory.CreateDirectory(movieEntry.movieName);
                            }
                            catch (Exception e)
                            {
                                File.AppendAllText("Folder-Error.txt",
                                    $"FailedToCreateDirectory : ,{movieEntry.movieName},{e}");
                                throw;
                            }

                            var targetElem = mp3DownloadLinks[0];
                            var originalLink = targetElem.GetAttribute("href");
                            var fullLink = targetElem.GetAttribute("href")?.Split("?");
                            var basePath = fullLink?[0];
                            var search = HttpUtility.UrlPathEncode(fullLink?[1].Split("&file=")[1]);
                            var mp3SongLink = $"{basePath}?ses={SessionId}&file={search}";
                            var mp3SongLink1 = $"https://isai.sbs/down.php?ses=cb56a03c&file=Tamil%202022%20Songs/Pathaan/Zoom%20Boom%20Doom%20Pathaan.mp3";
                            try
                            {
                                var restClient = new RestClient();
                                restClient.Options.MaxTimeout = 10;
                                var restRequest = new RestRequest(mp3SongLink, Method.Get);
                                restRequest.AddHeader("method", " GET");
                                restRequest.AddHeader("referer", " https://tamilmp3web.net/");
                                restRequest.AddHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.64");
                                int retryCount = 1;
                            Found:
                                var mp3DownloadTask = Task.Run(async () => await restClient.DownloadDataAsync(restRequest));
                                for (int i = 0; i <= 10; i++)
                                {
                                    if (mp3DownloadTask.IsCompleted) { break; }
                                    if (i == 10 && !mp3DownloadTask.IsCompleted)
                                    {
                                        retryCount++;
                                        Console.WriteLine($"{DateTime.Now:hh:mm:ss} Retrying for movie {movieEntry} and mp3 {mp3Entry.mp3Name}");
                                        if (retryCount < 5) goto Found;
                                        else throw new Exception("Failed");
                                    }

                                    Thread.Sleep(2000 * retryCount);
                                }

                                var response = mp3DownloadTask.Result;
                                await using var fs = new FileStream(filePath, FileMode.Create);
                                fs.Write(response);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{DateTime.Now:hh:mm:ss} FailedToDownloadFile for movie {movieEntry} and mp3 {mp3Entry.mp3Name}");
                                File.AppendAllText("Mp3-Error.txt", $"FailedToDownloadFile : ,{movieEntry.movieName} , {mp3Entry.mp3Name} ,  {mp3SongLink},{e.Message}{Environment.NewLine}{Environment.NewLine}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Two links found for movie {movieEntry} and mp3 {mp3Entry.mp3Name}");
                        }
                        Thread.Sleep(1000);

                    }

                }



            }
        }

        private static async Task<string> GetResponse(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }

        private static async Task<IHtmlCollection<IElement>> GetElements(string responseData, string selector)
        {
            IBrowsingContext context = BrowsingContext.New(Configuration.Default);
            IDocument document = await context.OpenAsync(virtualResponse => virtualResponse.Content(responseData));

            IHtmlCollection<IElement> elements = document.QuerySelectorAll(selector);
            return elements;
        }
    }
}