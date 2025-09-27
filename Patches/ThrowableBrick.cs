using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using LethalConfig;

namespace ThrowableBrick.Patches;

[BepInDependency("FlipMods.ReservedItemSlotCore", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("ainavt.lc.lethalconfig")]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ThrowableBrick : BaseUnityPlugin
{
    public static ThrowableBrick Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;

    public static AssetBundle BrickAsset;

    public static ConfigEntry<int> MinimumValue;
    public static ConfigEntry<int> MaximumValue;
    public static ConfigEntry<int> Weight;
    public static ConfigEntry<int> EntityDamage;
    public static ConfigEntry<int> PlayerDamage;
    public static ConfigEntry<int> BrickHealth;
    public static ConfigEntry<bool> GrabbableToEnemies;
    public static ConfigEntry<int> ItemRarity;
    public static ConfigEntry<bool> FunnyMode;
    public static ConfigEntry<int> FunnyModeExplosionDamage;
    public static ConfigEntry<bool> DamagePlayers;
    public static ConfigEntry<float> BrickValueLoss;
    public static ConfigEntry<int> FracturedWeight;
    public static ConfigEntry<int> FracturedEntityDamage;
    public static ConfigEntry<int> FracturedPlayerDamage;
    public static ConfigEntry<bool> ReservedItemSlot;



    public static bool useItemSlot = true;


    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");

        GetModConfig();
        InitializeItem();
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("FlipMods.ReservedItemSlotCore") && useItemSlot)
            ReservedItemSlotCompat.CreateSlotsAddItems();
    }


    internal void GetModConfig()
    {
        MinimumValue = Config.Bind(
            "General",
            "MinimumValue",
            32,
            "Minimum value of the brick"
        );
        MaximumValue = Config.Bind(
            "General",
            "MaximumValue",
            80,
            "Maximum value of the brick"
        );
        Weight = Config.Bind(
            "General",
            "Weight",
            10,
            "Weight of the brick");
        EntityDamage = Config.Bind(
            "General",
            "EntityDamage",
            3,
            "How many shovel hits worth of damage the brick does on impact");
        PlayerDamage = Config.Bind(
            "General",
            "PlayerDamage",
            20,
            "Damage dealt to the player in terms of percentage of health");
        BrickHealth = Config.Bind(
            "General",
            "BrickHealth",
            5,
            "How many times the brick can be thrown before being destroyed.");
        GrabbableToEnemies = Config.Bind("General",
            "GrabbableToEnemies",
            true,
            "If true, enemies can pick up this item.");
        ItemRarity = Config.Bind("General",
            "ItemRarity",
            100,
            "Controls how often this item spawns (higher = more common).");
        FunnyMode = Config.Bind("General",
            "FunnyMode",
            true,
            "Enables explosions on brick destruction.");
        FunnyModeExplosionDamage = Config.Bind("General",
            "FunnyModeExplosionDamage",
            37,
            "Explosion damage dealt when FunnyMode is enabled.");
        DamagePlayers = Config.Bind("General",
            "DamagePlayers",
            true,
            "If true, it damages players when thrown.");
        BrickValueLoss = Config.Bind("General",
            "BrickValueLoss",
            0.28f,
            "Percentage of scrap value lost on use (0.0–1.0).");
        FracturedWeight = Config.Bind("General",
            "FractureWeight",
            7,
            "Weight of the brick when fractured.");
        FracturedEntityDamage = Config.Bind("General",
            "FracturdedEntityDamage",
            3,
            "Damage dealt to enemies by the fractured brick.");
        FracturedPlayerDamage = Config.Bind("General",
            "FracturedPlayerDamage",
            20,
            "Damage dealt to players by the fractured brick.");
        ReservedItemSlot = Config.Bind("General",
            "ReservedItemSlot",
            true,
            "If true, enables the ReservedItemSlot.");
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

        int rarity = ItemRarity.Value;
        throwableBrickItem.minValue = MinimumValue.Value;
        throwableBrickItem.maxValue = MaximumValue.Value;
        throwableBrickItem.weight = Weight.Value / 105f + 1;
        brickBehavior.isExplosive = FunnyMode.Value;
        brickBehavior.grabbableToEnemies = GrabbableToEnemies.Value;
        brickBehavior.health = BrickHealth.Value;
        brickBehavior.explosiveDamange = FunnyModeExplosionDamage.Value;
        brickBehavior.entityDamage = EntityDamage.Value;
        brickBehavior.playerDamage = PlayerDamage.Value;
        brickBehavior.damagePlayers = DamagePlayers.Value;
        brickBehavior.brickValueLoss = 1 - BrickValueLoss.Value;
        brickBehavior.fracturedBrick = fracturedThrowableBrickItem;

        fracturedBrickBehavior.grabbable = true;
        fracturedBrickBehavior.itemProperties = fracturedThrowableBrickItem;

        fracturedThrowableBrickItem.minValue = MinimumValue.Value;
        fracturedThrowableBrickItem.maxValue = MaximumValue.Value;
        fracturedThrowableBrickItem.weight = FracturedWeight.Value / 105f + 1;
        fracturedBrickBehavior.isExplosive = FunnyMode.Value;
        fracturedBrickBehavior.grabbableToEnemies = GrabbableToEnemies.Value;
        fracturedBrickBehavior.health = BrickHealth.Value;
        fracturedBrickBehavior.explosiveDamange = FunnyModeExplosionDamage.Value;
        fracturedBrickBehavior.entityDamage = FracturedEntityDamage.Value;
        fracturedBrickBehavior.playerDamage = FracturedPlayerDamage.Value;
        fracturedBrickBehavior.damagePlayers = DamagePlayers.Value;
        fracturedBrickBehavior.brickValueLoss = 1 - BrickValueLoss.Value;

        useItemSlot = itemJson.reservedItemSlot;



        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(throwableBrickItem.spawnPrefab);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(fracturedThrowableBrickItem.spawnPrefab);
        LethalLib.Modules.Items.RegisterScrap(throwableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.All);
        LethalLib.Modules.Items.RegisterScrap(fracturedThrowableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.None);
    }
}
