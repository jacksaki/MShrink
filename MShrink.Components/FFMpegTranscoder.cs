using FFMpegCore;
using FFMpegCore.Enums;
using R3;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace MShrink.Components;

public class FFMpegTranscoder : IMediaTranscoder
{
    private TimeProvider TimeProvider { get; }
    public FFMpegTranscoder(TimeProvider provider)
    {
        this.TimeProvider = provider;
        this.Progress = new ReactiveProperty<MediaTranscodeProgress>(new MediaTranscodeProgress());
    }
    public FFMpegTranscoder()
        : this(TimeProvider.System)
    {
    }

    public ReactiveProperty<MediaTranscodeProgress> Progress { get; }

    public async Task<MediaTranscodeResult> CompressAsync(MediaTranscodeParameter parameter, CancellationToken cancellationToken = default)
    {
        if (File.Exists(parameter.DestPath))
        {
            File.Delete(parameter.DestPath);
        }
        using (File.Create(parameter.DestPath)) { /* 空のファイルを生成して即クローズ */ }

        return await TranscodeAsync(parameter);
    }

    private async Task<MediaTranscodeResult> TranscodeAsync(MediaTranscodeParameter parameter,CancellationToken cancellationToken=default)
    {
        var mediaInfo = await FFProbe.AnalyseAsync(parameter.SourcePath);
        var duration = mediaInfo.Duration.TotalSeconds;

        if (duration <= 0)
        {
            throw new InvalidOperationException("動画の長さを取得できません");
        }

        const int audioBitrateKbps = 128;
        const double sizeMargin = 0.92;

        var totalBitrateKbps = parameter.MaxFileSizeBytes * sizeMargin * 8 / duration / 1000;

        var videoBitrateKbps = (int)Math.Floor(totalBitrateKbps - audioBitrateKbps);

        if (videoBitrateKbps <= 0)
        {
            throw new InvalidOperationException("指定されたファイルサイズでは変換できません");
        }
        var start = this.TimeProvider.GetUtcNow();
        IProgress<double> progress = new Progress<double>(value =>
        {
            this.Progress.Value = new MediaTranscodeProgress()
            {
                Percent = value,
                Duration = TimeProvider.GetUtcNow() - start
            };
        });

        await FFMpegArguments.FromFileInput(parameter.SourcePath)
            .OutputToFile(parameter.DestPath, false, options => options
                .WithVideoCodec(VideoCodec.LibX264)
                .WithVideoBitrate(videoBitrateKbps)
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(audioBitrateKbps)
                .WithFastStart())
            .NotifyOnProgress(value => progress.Report(value), mediaInfo.Duration)
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();

        return new MediaTranscodeResult(parameter.DestPath, new System.IO.FileInfo(parameter.DestPath).Length, TimeProvider.GetUtcNow() - start);
    }
}
