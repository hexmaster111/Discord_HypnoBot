using System;

namespace HypnoBot;

public static class Ext
{
    public static string ToApiString(this StimKind sk) => sk switch
    {
        StimKind.Zap => "zap",
        StimKind.Buzz => "vibe",
        StimKind.Beep => "beep",
        _ => throw new ArgumentOutOfRangeException(nameof(sk), sk, null)
    };
}