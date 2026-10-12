using System;
using System.Collections.Generic;
using System.Text;

namespace MShrink.Components;

public sealed record MediaTranscodeResult(string DestPath, long FileSizeBytes, TimeSpan Duration);