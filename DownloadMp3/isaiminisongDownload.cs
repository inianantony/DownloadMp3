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
    /// GET SESSION ID BY VISITING THE LINK FROM BROWSER
    /// </summary>
    public class IsaiminisongDownload
    {
        private static CookieContainer cookieContainer = new CookieContainer();
        private static HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookieContainer };
        private static HttpClient client = new HttpClient(handler);

        public async Task ProcessDownloadAsync()
        {
            var homePath = "https://isaiminisong.com";
            var pagePath = $"{homePath}/songs/tamil-2022-songs/page/";

            for (int page = 1; page <= 35; page++)
            {
                var url = pagePath + page;
                string responseData = await GetResponse(url);
                var selector = ".entry-title a";
                var elements = await GetElements(responseData, selector);
                List<(string movieName, string moviePath)> moviePaths = new List<(string, string)>();

                foreach (var item in elements)
                {
                    var moviePath = item.GetAttribute("href");
                    var tempName = item.TextContent.Replace(" Tamil Mp3 Songs Download", "",
                        StringComparison.OrdinalIgnoreCase);
                    var unreplaced = tempName;
                    if (tempName.TrimEnd().EndsWith("Tamil"))
                    {
                        tempName = tempName.Replace("Tamil", "(Tamil)", StringComparison.OrdinalIgnoreCase);
                    }
                    else if (tempName.TrimEnd().EndsWith("Tamil Album"))
                    {
                        tempName = tempName.Replace("Tamil Album", "(Tamil Album)", StringComparison.OrdinalIgnoreCase);
                    }
                    else if (tempName.TrimEnd().EndsWith("Album"))
                    {
                        tempName = tempName.Replace("Album", "(Album)", StringComparison.OrdinalIgnoreCase);
                    }
                    var movieName = tempName;
                    if (Directory.Exists(movieName) || Directory.Exists(unreplaced) || moviePaths.Any(a => a.movieName == movieName))
                        continue;

                    moviePaths.Add((movieName, $"{moviePath}"));
                }

                foreach (var movieEntry in moviePaths)
                {
                    Console.WriteLine($"Processing movie {movieEntry}");

                    List<(string mp3Name, string mp3Path)> mp3Paths = new List<(string, string)>();

                    url = movieEntry.moviePath;
                    responseData = await GetResponse(url);
                    selector = ".downloadinfo a:nth-child(2)";
                    elements = await GetElements(responseData, selector);
                    var songNameElements = await GetElements(responseData, ".songname");
                    int i = 0;
                    foreach (var item in elements)
                    {
                        var mp3Path = item.GetAttribute("href");
                        var mp3Name = songNameElements[i].TextContent.Split(".")[1].Trim();
                        mp3Paths.Add((mp3Name, $"{homePath}{mp3Path}"));
                        i++;
                    }

                    foreach (var mp3Entry in mp3Paths)
                    {
                        Console.WriteLine($"Processing movie {movieEntry} and mp3 {mp3Entry.mp3Name}");

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

                        var filePath = $"{movieEntry.movieName}\\{mp3Entry.mp3Name}.mp3";
                        if (File.Exists(filePath))
                        {
                            Console.WriteLine($"Skipping for movie {movieEntry} and mp3 {mp3Entry.mp3Name}");
                            continue;
                        }

                        //var targetElem = mp3DownloadLinks[0];
                        //var fullLink = targetElem.GetAttribute("href")?.Split("?");
                        //var basePath = fullLink?[0];
                        //var search = HttpUtility.UrlPathEncode(fullLink?[1].Split("&file=")[1]);
                        //var mp3SongLink = $"{basePath}?ses=061b1c46&file={search}";
                        //var mp3SongLink = $"{basePath}?{"ses=061b1c46&file=Tamil%202022%20Songs/Sita%20Ramam%20(Tamil)/Piriyadhey.mp3"}";
                        try
                        {
                            var restClient = new RestClient();
                            var restRequest = new RestRequest(mp3Entry.mp3Path, Method.Get);
                            restRequest.AddHeader("method", " GET");
                            restRequest.AddHeader("referer", " https://tamilmp3free.net/");
                            restRequest.AddHeader("user-agent",
                                " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
                            var response = await restClient.DownloadDataAsync(restRequest);
                            await using var fs = new FileStream(filePath, FileMode.Create);
                            fs.Write(response);
                        }
                        catch (Exception e)
                        {
                            File.AppendAllText("Mp3-Error.txt",
                                $"FailedToDownloadFile : ,{movieEntry.movieName} , {mp3Entry.mp3Name} ,  {mp3Entry.mp3Name},{e.ToString()}");
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