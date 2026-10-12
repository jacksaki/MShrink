using System;
using System.Collections.Generic;
using System.Text;

namespace MShrink.Components;

public sealed class MediaTranscodeProgress
{
    public double Percent { get; internal set; } = 0d;
    public TimeSpan Duration { get; internal set; } = TimeSpan.Zero;
}
