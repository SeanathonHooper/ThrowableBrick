using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace ThrowableBrick.Patches;

[BepInDependency("FlipMods.ReservedItemSlotCore", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ThrowableBrick : BaseUnityPlugin
{
    public static ThrowableBrick Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;

    public static AssetBundle? BrickAsset;

    public static ConfigEntry<int>? MinimumValue = null;
    public static ConfigEntry<int>? MaximumValue = null;
    public static ConfigEntry<int>? Weight = null;
    public static ConfigEntry<int>? EntityDamage = null;
    public static ConfigEntry<int>? PlayerDamage = null;
    public static ConfigEntry<int>? BrickHealth = null;
    public static ConfigEntry<bool>? GrabbableToEnemies = null;
    public static ConfigEntry<int>? ItemRarity = null;
    public static ConfigEntry<bool>? FunnyMode = null;
    public static ConfigEntry<int>? FunnyModeExplosionDamage = null;
    public static ConfigEntry<bool>? DamagePlayers = null;
    public static ConfigEntry<float>? BrickValueLoss = null;
    public static ConfigEntry<int>? FracturedWeight = null;
    public static ConfigEntry<int>? FracturedEntityDamage = null;
    public static ConfigEntry<int>? FracturedPlayerDamage = null;
    public static ConfigEntry<bool>? ReservedItemSlot = null;



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
            "Percentage of scrap value lost on when thrown (0.0–1.0), set to 1 for no value lost.");
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

#pragma warning disable CS8602 // Dereference of a possibly null reference.
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

        useItemSlot = ReservedItemSlot.Value;



        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(throwableBrickItem.spawnPrefab);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(fracturedThrowableBrickItem.spawnPrefab);
        LethalLib.Modules.Items.RegisterScrap(throwableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.All);
        LethalLib.Modules.Items.RegisterScrap(fracturedThrowableBrickItem, rarity, LethalLib.Modules.Levels.LevelTypes.None);
#pragma warning restore CS8602
    }
}
