using System;

namespace DownloadMp3
{
    public class Program
    {
        static void Main(string[] args)
        {
            var mp3Download = new Mp3Download();
            mp3Download.ProcessDownload();
            Console.WriteLine("Download is Completed!");
            Console.Read();
        }
    }
}
