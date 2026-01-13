using System;

public static class Ext
{
    public static string ToApiString(this PavStimulesKind sk) => sk switch
    {
        PavStimulesKind.Zap => "zap",
        PavStimulesKind.Buzz => "vibe",
        PavStimulesKind.Beep => "beep",
        _ => throw new ArgumentOutOfRangeException(nameof(sk), sk, null)
    };
}