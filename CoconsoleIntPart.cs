using System;
using Unity.Mathematics;

namespace Coconsole
{
    public class CoconsoleIntPart : ICoconsolePart
    {
        public string humanView => "<int>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => Int32.TryParse(part, out _);
        public static int ToValue(string part) => Int32.Parse(part);
    }

    public class CoconsoleLongPart : ICoconsolePart
    {
        public string humanView => "<long>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => long.TryParse(part, out _);
        public static long ToValue(string part) => long.Parse(part);
    }

    public class CoconsoleULongPart : ICoconsolePart
    {
        public string humanView => "<ulong>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => ulong.TryParse(part, out _);
        public static ulong ToValue(string part) => ulong.Parse(part);
    }

    public class CoconsoleUIntPart : ICoconsolePart
    {
        public string humanView => "<uint>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => uint.TryParse(part, out _);
        public static uint ToValue(string part) => uint.Parse(part);
    }

    public class CoconsoleBytePart : ICoconsolePart
    {
        public string humanView => "<byte>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => byte.TryParse(part, out _);
        public static byte ToValue(string part) => byte.Parse(part);
    }

    public class CoconsoleShortPart : ICoconsolePart
    {
        public string humanView => "<short>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => short.TryParse(part, out _);
        public static short ToValue(string part) => short.Parse(part);
    }

    public class CoconsoleUShortPart : ICoconsolePart
    {
        public string humanView => "<ushort>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => ushort.TryParse(part, out _);
        public static ushort ToValue(string part) => ushort.Parse(part);
    }

    public class CoconsoleInt2Part : ICoconsolePart
    {
        public string humanView => "<int,int>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;

        public bool Validate(string part)
        {
            var ch = part.Split(',');
            if (ch.Length != 2) return false;
            return Int32.TryParse(ch[0], out _) && Int32.TryParse(ch[1], out _);
        }

        public static int2 ToValue(string part)
        {
            var ch = part.Split(',');
            return new int2(Int32.Parse(ch[0]), Int32.Parse(ch[1]));
        }
    }

    public class CoconsoleFloat2Part : ICoconsolePart
    {
        public string humanView => "<float,float>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;

        public bool Validate(string part)
        {
            var ch = part.Split(',');
            if (ch.Length != 2) return false;
            return float.TryParse(ch[0].Replace('.',','), out _) && float.TryParse(ch[1].Replace('.',','), out _);
        }

        public static float2 ToValue(string part)
        {
            var ch = part.Split(',');
            return new float2(float.Parse(ch[0].Replace('.',',')), float.Parse(ch[1].Replace('.',',')));
        }
    }

    public class CoconsoleFloat3Part : ICoconsolePart
    {
        public string humanView => "<float,float,float>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;

        public bool Validate(string part)
        {
            var ch = part.Split(',');
            if (ch.Length != 3) return false;
            return float.TryParse(ch[0].Replace('.',','), out _) && float.TryParse(ch[1].Replace('.',','), out _) && float.TryParse(ch[2].Replace('.',','), out _);
        }

        public static float3 ToValue(string part)
        {
            var ch = part.Split(',');
            return new float3(float.Parse(ch[0].Replace('.',',')), float.Parse(ch[1].Replace('.',',')), float.Parse(ch[2].Replace('.',',')));
        }
    }

    public class CoconsoleVector2Part : CoconsoleFloat2Part
    {
    }

    public class CoconsoleVector3Part : CoconsoleFloat3Part
    {
    }

    public class CoconsoleInt3Part : ICoconsolePart
    {
        public string humanView => "<int,int,int>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;

        public bool Validate(string part)
        {
            var ch = part.Split(',');
            if (ch.Length != 3) return false;
            return Int32.TryParse(ch[0], out _) && Int32.TryParse(ch[1], out _) && Int32.TryParse(ch[2], out _);
        }

        public static int3 ToValue(string part)
        {
            var ch = part.Split(',');
            return new int3(Int32.Parse(ch[0]), Int32.Parse(ch[1]), Int32.Parse(ch[2]));
        }
    }

    public class CoconsoleFloatPart : ICoconsolePart
    {
        public string humanView => "<float>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => float.TryParse(part, out _);
        public static float ToValue(string part) => float.Parse(part);
    }

    public class CoconsoleDoublePart : ICoconsolePart
    {
        public string humanView => "<double>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => double.TryParse(part, out _);
        public static double ToValue(string part) => double.Parse(part);
    }

    public class CoconsoleStringPart : ICoconsolePart
    {
        public string humanView => "<string>";
        public string[] suggestions => Array.Empty<string>();
        public bool hasSuggestions => false;
        public bool Validate(string part) => true;
        public static string ToValue(string part) => part;
    }

    public class CoconsoleEnumPart<T> : ICoconsolePart where T : Enum
    {
        public string humanView => $"<{typeof(T).Name}>";
        public string[] suggestions => Enum.GetNames(typeof(T));
        public bool hasSuggestions => true;
        public bool Validate(string part) => Enum.TryParse(typeof(T), part, out _);
        public static T ToValue(string part) => (T)Enum.Parse(typeof(T), part);
    }

    public class CoconsoleBoolPart : ICoconsolePart
    {
        public string humanView => "<bool>";
        public string[] suggestions => new[] { "true", "false" };
        public bool hasSuggestions => true;
        public bool Validate(string part) => bool.TryParse(part, out _);
        public static bool ToValue(string part) => bool.TryParse(part, out var b);
    }
}