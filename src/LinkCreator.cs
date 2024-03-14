using Hosihikari.NativeInterop.Utils;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Hosihikari.Minecraft.Extension.Addon;

public enum PackType
{
    ResourcePack,
    BehaviorPack,
    Unknown = -1 // todo support auto detect ?
}

[UnsupportedOSPlatform("windows")]
public static partial class PackHelper
{
    public static void AddPack(PackType packType, string packDirectory, PackInfo info)
    {
        if (!Directory.Exists(packDirectory))
        {
            throw new DirectoryNotFoundException($"packDirectory {packDirectory} not found");
        }

        string target = Path.Combine(
            Environment.CurrentDirectory,
            packType switch
            {
                PackType.BehaviorPack => "development_behavior_packs",
                PackType.ResourcePack => "development_resource_packs",
                PackType.Unknown => throw new IndexOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException(nameof(packType), packType, null)
            }
        );
        if (!Directory.Exists(target))
        {
            Directory.CreateDirectory(target);
        }

        string link = Path.Combine(target, info.PackId.ToString());
        if (Directory.Exists(link))
        {
            if (LinkUtils.IsLink(link))
            {
                string current = LinkUtils.ReadLink(link);
                if (current == packDirectory)
                {
                    LinkUtils.Unlink(link);
                }
            }
            else
            {
                Directory.Delete(link, true);
            }
        }

        if (!Directory.Exists(link))
        {
            try
            {
                LinkUtils.CreateDirectorySymlink(link, packDirectory);
            }
            catch (ExternalException)
            {
                // file system not support symlink
                // just copy folder
                if (!Directory.Exists(link))
                {
                    CopyFolder(packDirectory, link);

                    static void CopyFolder(string sourceFolder, string destFolder)
                    {
                        if (!Directory.Exists(destFolder))
                        {
                            Directory.CreateDirectory(destFolder);
                        }

                        string[] files = Directory.GetFiles(sourceFolder);
                        foreach (string file in files)
                        {
                            string name = Path.GetFileName(file);
                            string dest = Path.Combine(destFolder, name);
                            File.Copy(file, dest);
                        }

                        string[] folders = Directory.GetDirectories(sourceFolder);
                        foreach (string folder in folders)
                        {
                            string name = Path.GetFileName(folder);
                            string dest = Path.Combine(destFolder, name);
                            CopyFolder(folder, dest);
                        }
                    }
                }
            }
        }

        if (packType is PackType.BehaviorPack)
        {
            AddBehaviorPack(info);
        }
        else
        {
            AddResourcePack(info);
        }
    }
}