using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.IO;
using System.Reflection;
using ThrowableBrick.Patches;

namespace ThrowableBrick;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ThrowableBrick : BaseUnityPlugin
{
    public static ThrowableBrick Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;

    public static AssetBundle BrickAsset;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");

        InitializeItem();
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

        

        int rarity = 100;
        Item throwableBrickItem = BrickAsset.LoadAsset<Item>("Assets/BRICK/BrickItem.asset");
        BrickBehavior brickBehavior = throwableBrickItem.spawnPrefab.AddComponent<BrickBehavior>();

        brickBehavior.grabbable = true;
        brickBehavior.grabbableToEnemies = true;
        brickBehavior.itemProperties = throwableBrickItem;

        throwableBrickItem.minValue = 80;
        throwableBrickItem.maxValue = 200;
        throwableBrickItem.weight = 1.0952f;

        brickBehavior.isExplosive = bool.Parse(File.ReadAllText(Path.Combine(sAssemblyLocation, "settings.txt")));

        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(throwableBrickItem.spawnPrefab);
        LethalLib.Modules.Items.RegisterScrap(throwableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.All);
    }
}
