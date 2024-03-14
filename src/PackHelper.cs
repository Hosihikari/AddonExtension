using System.Text.Json.Nodes;

namespace Hosihikari.Minecraft.Extension.Addon;

public sealed record PackInfo(Guid PackId, (int, int, int) Version, string? SubPack = null);

public sealed class PackAlreadyLoadedException() :
    Exception("Pack already loaded. Please add pack before server started.");

public sealed class PackAlreadyAddedException(Guid packId) :
    Exception($"Pack {packId} already added.");

public static partial class PackHelper
{
    private static List<PackInfo>? s_resourcePacks = [];
    private static List<PackInfo>? s_behaviorPacks = [];

    private static readonly PackStackHook s_hook = new(ProcessWorldPacksJson);

    public static void AddResourcePack(PackInfo packInfo)
    {
        if (s_resourcePacks is null)
        {
            throw new PackAlreadyLoadedException();
        }

        if (s_resourcePacks.Any(x => x.PackId == packInfo.PackId))
        {
            throw new PackAlreadyAddedException(packInfo.PackId);
        }

        s_resourcePacks.Add(packInfo);
    }

    public static void AddBehaviorPack(PackInfo packInfo)
    {
        if (s_behaviorPacks is null)
        {
            throw new PackAlreadyLoadedException();
        }

        if (s_behaviorPacks.Any(x => x.PackId == packInfo.PackId))
        {
            throw new PackAlreadyAddedException(packInfo.PackId);
        }

        s_behaviorPacks.Add(packInfo);
    }

    public static void Init()
    {
        // ResourcePackStack::deserialize
        s_hook.Install();
    }

    /*
     * world_resource_packs.json & world_behavior_packs.json
     * {
     *     "pack_id" : "7d5214c2-6ca0-4aaf-8cd5-448a955f06ed",
     *     "version" : [ 1, 3, 0 ],
     *     "subpack" : "xxx"
     * }
     */
    public static void ProcessWorldPacksJson(JsonArray array)
    {
        // add customize pack
        if (s_resourcePacks is not null) // first call from resource pack
        {
            Process(s_resourcePacks);
            s_resourcePacks = null;
        }
        else if (s_behaviorPacks is not null) // second call
        {
            Process(s_behaviorPacks);
            s_behaviorPacks = null;
            // no longer need, uninstall hook
            LevelTick.PostTick(s_hook.Uninstall);
        }

        return;

        void Process(List<PackInfo> target)
        {
            foreach ((Guid id, (int a, int b, int c), string? subPack) in target)
            {
                JsonObject pack = new()
                {
                    ["pack_id"] = id.ToString(),
                    ["version"] = new JsonArray { a, b, c }
                };
                if (subPack is not null)
                {
                    pack.Add("subpack", subPack);
                }

                array.Add(pack);
            }
        }
    }
}