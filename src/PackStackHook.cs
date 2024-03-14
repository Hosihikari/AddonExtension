using Hosihikari.NativeInterop.Hook.ObjectOriented;
using Hosihikari.NativeInterop.Unmanaged.STL;
using Hosihikari.NativeInterop.Utils;
using System.Text.Json.Nodes;

namespace Hosihikari.Minecraft.Extension.Addon;

internal sealed class PackStackHook(Action<JsonArray> proceedFunc)
    : HookBase<PackStackHook.HookDelegateType>(ResourcePackStack.Original.Deserialize)
{
    protected override unsafe HookDelegateType HookDelegate =>
        (resourcePackStack, stream, a3) =>
        {
            // load original steam (maybe a file stream from world_resource_packs.json)
            StdInputStream stdInputStream = new(stream);
            // convert to byte[] (utf8 string)
            byte[] originalData = stdInputStream.ReadToEnd();
            JsonArray array = [];
            if (originalData.Length > 2)
            {
                // parse world_resource_packs.json
                array = JsonNode.Parse(originalData)?.AsArray()!;
            }

            // add custom pack
            {
                proceedFunc(array);
            }
            // add fake stream from modified world_resource_packs.json and pass to original function
            StdInputStream fakeStream = new(StringUtils.StringToManagedUtf8(array.ToString()));
            GC.KeepAlive(fakeStream);
            return Original(resourcePackStack, fakeStream, a3);
        };

    internal unsafe delegate void* HookDelegateType(void* @this, void* a2, void* a3);
}