using BepInEx;
using BepInEx.Logging;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace ThrowableBrick.Patches;

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
        Item fracturedThrowableBrickItem = BrickAsset.LoadAsset<Item>("Assets/BRICK/FracturedBrickItem.asset");
        BrickBehavior brickBehavior = throwableBrickItem.spawnPrefab.AddComponent<BrickBehavior>();
        FracturedBrickBehavior fracturedBrickBehavior = fracturedThrowableBrickItem.spawnPrefab.AddComponent<FracturedBrickBehavior>();

        brickBehavior.grabbable = true;
        brickBehavior.itemProperties = throwableBrickItem;

        int rarity = itemJson.itemRarity;
        throwableBrickItem.minValue = (int)(itemJson.minValue / .4);
        throwableBrickItem.maxValue = (int)(itemJson.maxValue / .4);
        throwableBrickItem.weight = itemJson.weight / 105f + 1;
        brickBehavior.isExplosive = itemJson.funnyMode;
        brickBehavior.grabbableToEnemies = itemJson.grabbableToEnemies;
        brickBehavior.health = itemJson.brickHealth;
        brickBehavior.explosiveDamange = itemJson.funnyModeExplosionDamage;
        brickBehavior.entityDamage = itemJson.entityDamage;
        brickBehavior.playerDamage = itemJson.playerDamage;
        brickBehavior.damagePlayers = itemJson.damagePlayers;
        brickBehavior.brickValueLoss = 1 - itemJson.brickValueLoss;
        brickBehavior.fracturedBrick = fracturedThrowableBrickItem;

        fracturedBrickBehavior.grabbable = true;
        fracturedBrickBehavior.itemProperties = fracturedThrowableBrickItem;

        fracturedThrowableBrickItem.minValue = (int)(itemJson.minValue / .4);
        fracturedThrowableBrickItem.maxValue = (int)(itemJson.maxValue / .4);
        fracturedThrowableBrickItem.weight = itemJson.fractureWeight / 105f + 1;
        fracturedBrickBehavior.isExplosive = itemJson.funnyMode;
        fracturedBrickBehavior.grabbableToEnemies = itemJson.grabbableToEnemies;
        fracturedBrickBehavior.health = itemJson.brickHealth;
        fracturedBrickBehavior.explosiveDamange = itemJson.funnyModeExplosionDamage;
        fracturedBrickBehavior.entityDamage = itemJson.fracturedEntityDamage;
        fracturedBrickBehavior.playerDamage = itemJson.fracturedPlayerDamage;
        fracturedBrickBehavior.damagePlayers = itemJson.damagePlayers;
        fracturedBrickBehavior.brickValueLoss = 1 - itemJson.brickValueLoss;



        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(throwableBrickItem.spawnPrefab);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(fracturedThrowableBrickItem.spawnPrefab);
        LethalLib.Modules.Items.RegisterScrap(throwableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.All);
        LethalLib.Modules.Items.RegisterScrap(fracturedThrowableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.None);
    }
}
