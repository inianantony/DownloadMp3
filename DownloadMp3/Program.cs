using System;
using System.Threading.Tasks;

namespace DownloadMp3
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            if (args != null && args.Length > 2)
            {
                //var mp3Download = new Mp3Download();
                //mp3Download.ProcessDownload();
            }

            //IsaiminisongDownload isaiminisongDownload = new IsaiminisongDownload();
            //await isaiminisongDownload.ProcessDownloadAsync();

            var tamilmp3WebDownload = new Tamilmp3WebDownload();
            await tamilmp3WebDownload.ProcessDownloadAsync();

            Console.WriteLine("Download is Completed!");
            Console.Read();
        }
    }
}
