using R3;
using System;
using System.Collections.Generic;
using System.Text;

namespace MShrink.Components;

public interface IMediaTranscoder
{
    public Task<MediaTranscodeResult> CompressAsync(MediaTranscodeParameter parameter, CancellationToken cancellationToken = default);
    public ReactiveProperty<MediaTranscodeProgress> Progress { get; }
}
