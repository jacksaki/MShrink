using System;
using System.Collections.Generic;
using System.Text;

namespace MShrink.Components;

public sealed class MediaTranscodeParameter
{
    public long MaxFileSizeBytes { get; init; }
    public required string SourcePath { get; init; }
    public required string DestPath { get; init; }
    public void Validate()
    {
        if (!File.Exists(this.SourcePath))
        {
            throw new FileNotFoundException($"{this.SourcePath}: file not found.");
        }

        if (string.Equals(
                Path.GetFullPath(this.SourcePath),
                Path.GetFullPath(this.DestPath),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"input and output refer to the same file.");
        }

        if (this.MaxFileSizeBytes is <= 0)
        {
            throw new ArgumentOutOfRangeException($"max file size must be larger than 0.");
        }
    }
}