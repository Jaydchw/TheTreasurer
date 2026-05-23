using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using BaseLib.Patches.Localization;

namespace TheTreasurer.TheTreasurerCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "TheTreasurer"; //Used for resource filepath
    public const string LocPrefix = "THETREASURER"; // Used by localization keys.
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        // Enable BaseLib SimpleLoc processing for this mod's localization table.
        SimpleLoc.EnableSimpleLoc(LocPrefix);

        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
        
        Harmony harmony = new(ModId);

        harmony.PatchAll();
    }
}
