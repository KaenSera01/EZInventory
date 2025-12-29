using EZInventory;
using MelonLoader;

[assembly: MelonInfo(typeof(EZInventoryMod), "EZInventory", "1.0.2", "Kaen01")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace EZInventory
{
    public class EZInventoryMod : MelonMod
    {
		public static MelonPreferences_Category Config;
		public static MelonPreferences_Entry<string> GrabAllKey;
		public static MelonPreferences_Entry<bool> GrabAllAutoClose;
		public static MelonPreferences_Entry<string> DepositAllKey;
		public static MelonPreferences_Entry<bool> DepositAllAutoClose;

		public override void OnInitializeMelon()
		{
            base.OnInitializeMelon();
			MelonLogger.Msg("EZInventory loaded!");

			Config = MelonPreferences.CreateCategory("EZInventory", "EZInventory Settings");

			GrabAllKey = Config.CreateEntry(
				"GrabAllKey",
				"G", // default
				description: "Hotkey for Grab All (case-insensitive)."
			);
			MelonLogger.Msg($"EZInventory: GrabAllKey set to '{GrabAllKey.Value}'");

			GrabAllAutoClose = Config.CreateEntry(
				"GrabAllAutoClose",
				true, // default: auto-close ON
				description: "Automatically close the inventory screen after Grab All."
			);

			DepositAllKey = Config.CreateEntry(
				"DepositAllKey",
				"H", // default hotkey for deposit-all
				description: "Hotkey for Deposit All (case-insensitive)."
			);
			MelonLogger.Msg($"EZInventory: DepositAllKey set to '{DepositAllKey.Value}'");

			DepositAllAutoClose = Config.CreateEntry(
				"DepositAllAutoClose",
				false, // default: do NOT auto-close on deposit-all
				description: "Automatically close the inventory after Deposit All."
			);

			MelonPreferences.Save();
		}
	}
}
