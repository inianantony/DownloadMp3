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
                var mp3Download = new Mp3Download();
                mp3Download.ProcessDownload();
                var tamilmp3FreeDownload = new Tamilmp3FreeDownload();
                await tamilmp3FreeDownload.ProcessDownloadAsync();
            }

            IsaiminisongDownload isaiminisongDownload = new IsaiminisongDownload();
            await isaiminisongDownload.ProcessDownloadAsync();

            Console.WriteLine("Download is Completed!");
            Console.Read();
        }
    }
}
