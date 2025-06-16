using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using UnityEngine;
using LethalLib;
using System.IO;
using System.Reflection;
using ThrowableBrick.Patches;

namespace ThrowableBrick;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class ThrowableBrick : BaseUnityPlugin
{
    public static ThrowableBrick Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    public static AssetBundle BrickAsset;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        Logger.LogInfo("FUCK!");

        InitializeItem();
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }

    internal static void InitializeItem()
    {
        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        BrickAsset = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "brickassetbundle"));
        if (BrickAsset == null)
        {
            Logger.LogInfo("Failed to load custom assets");
            return;
        }

        int rarity = 100000;
        Item throwableBrickItem = BrickAsset.LoadAsset<Item>("Assets/BRICK/BrickItem.asset");
        BrickBehavior brickBehavior = throwableBrickItem.spawnPrefab.AddComponent<BrickBehavior>();
        brickBehavior.grabbable = true;
        brickBehavior.grabbableToEnemies = true;
        brickBehavior.itemProperties = throwableBrickItem;
        throwableBrickItem.minValue = 40;
        throwableBrickItem.maxValue = 80;
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(throwableBrickItem.spawnPrefab);
        LethalLib.Modules.Items.RegisterScrap(throwableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.All, null);
    }
}
