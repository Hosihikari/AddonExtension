using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[assembly: UnsupportedOSPlatform("windows")]

namespace Hosihikari.Minecraft.Extension.Addon;

internal static class EntryPoint
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Main() // must be loaded
    {
        PackHelper.Init();
    }
}