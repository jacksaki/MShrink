using Windows.Media.MediaProperties;
using Windows.Storage;
using R3;

namespace MShrink.Components;

public class MediaTranscoder : IMediaTranscoder
{
    private TimeProvider TimeProvider { get; }
    public MediaTranscoder(TimeProvider provider)
    {
        this.TimeProvider = provider;
        this.Progress = new ReactiveProperty<MediaTranscodeProgress>(new MediaTranscodeProgress());
    }
    public MediaTranscoder()
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

        // 出力ファイルを StorageFile に変換
        var profile = await GetDestProfileAsync(parameter);

        return await TranscodeAsync(parameter, profile);
    }

    private async Task<MediaTranscodeResult> TranscodeAsync(MediaTranscodeParameter parameter, MediaEncodingProfile profile) 
    {
        // 入力ファイルを StorageFile に変換
        var inputFile = await StorageFile.GetFileFromPathAsync(parameter.SourcePath);
        var outputFile = await StorageFile.GetFileFromPathAsync(parameter.DestPath);

        // 3. トランスコード実行
        var transcoder = new Windows.Media.Transcoding.MediaTranscoder { HardwareAccelerationEnabled = false };
        var prepareOp = await transcoder.PrepareFileTranscodeAsync(inputFile, outputFile, profile);
        var start = this.TimeProvider.GetUtcNow();
        if (prepareOp.CanTranscode)
        {
            // 変換処理の非同期タスクを開始
            var transcodeOp = prepareOp.TranscodeAsync();
            transcodeOp.Progress += (sender, progressPercent) =>
            {
                this.Progress.Value = new MediaTranscodeProgress()
                {
                    Percent = progressPercent,
                    Duration = TimeProvider.GetUtcNow() - start
                };
            };

            // 変換の完了を待機
            await transcodeOp;
            return new MediaTranscodeResult(parameter.DestPath, new System.IO.FileInfo(parameter.DestPath).Length, TimeProvider.GetUtcNow() - start);
        }
        else
        {
            throw new InvalidOperationException($"変換の準備に失敗しました: {prepareOp.FailureReason}");
        }
    }
    private async Task<MediaEncodingProfile> GetDestProfileAsync(MediaTranscodeParameter parameter)
    {
        // 入力ファイルを StorageFile に変換
        var inputFile = await StorageFile.GetFileFromPathAsync(parameter.SourcePath);

        // 1. 元動画の解像度・フレームレート・音声設定をそのまま維持してベースを作成
        var profile = await MediaEncodingProfile.CreateFromFileAsync(inputFile);

        // 2. 元動画の再生時間を取得して、目標サイズ（20MB）に収まるビットレートを計算
        var properties = await inputFile.Properties.GetVideoPropertiesAsync();
        double durationSeconds = properties.Duration.TotalSeconds;
        if (durationSeconds <= 0)
        {
            throw new Exception($"media length <= 0");
        }

        // 目補ファイルサイズをビット（bits）に変換 
        var targetBits = parameter.MaxFileSizeBytes * 8;

        // 必要な総ビットレート (bps)
        var totalBitrate = (uint)(targetBits / durationSeconds);

        // 音声ストリームのビットレートを取得（変更しない）
        var audioBitrate = profile.Audio?.Bitrate ?? 128000;

        // 映像に割り当てるビットレートを算出 (最低でも100kbpsを確保する安全弁)
        var videoBitrate = totalBitrate > audioBitrate ? totalBitrate - audioBitrate : 100000;

        // 映像のビットレートのみを上書き変更
        profile.Video.Bitrate = videoBitrate;
        Console.WriteLine($"動画長: {durationSeconds:F2}秒 | 計算された映像ビットレート: {videoBitrate / 1000} kbps");
        Console.WriteLine();
        return profile;
    }
}
