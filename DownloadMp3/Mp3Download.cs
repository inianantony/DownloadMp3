using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using CsvHelper;
using CsvHelper.Configuration;

namespace DownloadMp3
{
    public class Mp3Download
    {
        public void ProcessDownload()
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            };
            var linksList = new Dictionary<string, Dictionary<string, string>>();
            using (var stream = File.OpenRead("2021.csv"))
            {
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, csvConfiguration);
                var records = csv.GetRecords<Mp3Info>();
                records = records.OrderBy(a => a.Mp3Name);
                foreach (var mp3Info in records)
                {
                    var movieName = mp3Info.MovieName.Replace("(2020)", "").Trim();
                    var strings = mp3Info.Mp3Link.Split('/');
                    var decode = HttpUtility.UrlDecode(strings[^1]) ?? string.Empty;
                    var mp3Name = decode.Replace(" - Masstamilan.In", "");

                    var downloadedMovie = @"D:\Music\Music\Movie\2020 Hits\FOLDERS\" + movieName;
                    if (Directory.Exists(downloadedMovie))
                        continue;
                    if (!linksList.ContainsKey(movieName))
                    {
                        linksList[movieName] = new Dictionary<string, string>();
                    }
                    if (!linksList[movieName].ContainsKey(mp3Info.Mp3Link))
                    {
                        linksList[movieName][mp3Info.Mp3Link] = mp3Name;
                    }
                }
            }

            Parallel.ForEach(linksList, new ParallelOptions { MaxDegreeOfParallelism = 2 }, movieKvp =>
            {
                try
                {
                    if (!Directory.Exists(movieKvp.Key))
                        Directory.CreateDirectory(movieKvp.Key);
                }
                catch (Exception e)
                {
                    File.AppendAllText("Folder-Error.txt",
                        $"FailedToCreateDirectory : ,{movieKvp.Key},{e}");
                    throw;
                }

                foreach (var mp3Kvp in movieKvp.Value)
                {
                    if (File.Exists($"{movieKvp.Key}\\{mp3Kvp.Value}"))
                        continue;
                    try
                    {
                        using var client = new WebClient();
                        client.DownloadFile(mp3Kvp.Key, $"{movieKvp.Key}\\{mp3Kvp.Value}");
                    }
                    catch (Exception e)
                    {
                        File.AppendAllText("Mp3-Error.txt", $"FailedToDownloadFile : ,{mp3Kvp.Key},{e.ToString()}");
                    }
                }
            });
        }
    }
}