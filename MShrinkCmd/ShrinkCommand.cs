using ConsoleAppFramework;
using FFMpegCore;
using MShrink.Components;
using R3;
using System;
using System.Collections.Generic;
using System.Text;

namespace MShrinkCmd;

public class ShrinkCommand
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="srcPath">-s, source path</param>
    /// <param name="destPath">-d, dest path</param>
    /// <param name="fileSizeMB">-m, file size (MB)</param>
    /// <param name="useFfmpeg">-f, use ffmpeg</param>
    /// <returns></returns>
    [Command("")]
    public async Task ExecuteAsync(string srcPath, string destPath,long fileSizeMB, bool useFfmpeg)
    {
        var p = new MediaTranscodeParameter()
        {
            SourcePath = srcPath,
            DestPath = destPath,
            MaxFileSizeBytes = fileSizeMB * 1024L * 1024L
        };
        var transcoder = await CreateMediaTranscoderAsync(useFfmpeg);
        transcoder.Progress.SubscribeAwait(async(x,ct) =>
        {
            if (x.Percent <= 0)
            {
                return;
            }
            var progressLength = (int)(x.Percent / 100 * 40);
            var progressBar = new string('*', progressLength);
            var spaces = new string(' ', 40 - progressLength);
            Console.Write($"\r[{progressBar}{spaces}] {x.Percent:F2}%");

            await Task.Delay(50);
        });
        var result = await transcoder.CompressAsync(p);
        Console.WriteLine();
        Console.WriteLine($"{result.FileSizeBytes/1024/1024:F2}MB {result.Duration.TotalMilliseconds/1000:F2} s");
    }
    private async Task<IMediaTranscoder> CreateMediaTranscoderAsync(bool ffmpeg)
    {
        if (ffmpeg)
        {
            GlobalFFOptions.Configure(options => options.BinaryFolder = "./bin");
            await FFMpegCore.Extensions.Downloader.FFMpegDownloader.DownloadBinaries();
            return new FFMpegTranscoder();
        }
        else
        {
            return new MediaTranscoder();
        }
    }
}
