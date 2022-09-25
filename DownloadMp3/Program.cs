using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Transactions;

namespace DownloadMp3
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            if (args != null && args.Length > 2)
            {
                var mp3Download = new Mp3Download();
                mp3Download.ProcessDownload();
                var tamilmp3FreeDownload = new Tamilmp3FreeDownload();
                await tamilmp3FreeDownload.ProcessDownloadAsync();
            }
            




            Console.WriteLine("Download is Completed!");
            Console.Read();
        }
    }
}
