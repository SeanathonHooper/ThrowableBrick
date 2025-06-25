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
        BrickCustomization itemJson = JsonUtility.FromJson<BrickCustomization>(File.ReadAllText(Path.Combine(sAssemblyLocation, "settings.json")));
        BrickAsset = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "brickassetbundle"));
        if (BrickAsset == null)
        {
            Logger.LogInfo("Failed to load custom assets");
            return;
        }

        Item throwableBrickItem = BrickAsset.LoadAsset<Item>("Assets/BRICK/BrickItem.asset");
        BrickBehavior brickBehavior = throwableBrickItem.spawnPrefab.AddComponent<BrickBehavior>();

        brickBehavior.grabbable = true;
        brickBehavior.itemProperties = throwableBrickItem;

        int rarity = itemJson.itemRarity;
        throwableBrickItem.minValue = (int)(itemJson.minValue / .4);
        throwableBrickItem.maxValue = (int)(itemJson.maxValue / .4);
        throwableBrickItem.weight = (float)((itemJson.weight / 105f) + 1);
        brickBehavior.isExplosive = itemJson.funnyMode;
        brickBehavior.grabbableToEnemies = itemJson.grabbableToEnemies;
        brickBehavior.health = itemJson.brickHealth;
        brickBehavior.explosiveDamange = itemJson.funnyModeExplosionDamage;
        brickBehavior.entityDamage = itemJson.entityDamage;
        brickBehavior.playerDamage = itemJson.playerDamage;
        brickBehavior.damagePlayers = itemJson.damagePlayers;

        Debug.Log(throwableBrickItem.minValue);
        Debug.Log(throwableBrickItem.weight);
        Debug.Log(brickBehavior.grabbableToEnemies);

        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(throwableBrickItem.spawnPrefab);
        LethalLib.Modules.Items.RegisterScrap(throwableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.All);
    }
}
