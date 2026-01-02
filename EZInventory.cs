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
		public static MelonPreferences_Entry<string> FillStacksKey;
		public static MelonPreferences_Entry<bool> FillStacksAutoClose;
		public static MelonPreferences_Entry<string> CopyAllFilters;
		public static MelonPreferences_Entry<string> PasteAllFilters;
		public static MelonPreferences_Entry<string> ClearAllFilters;

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

			FillStacksKey = Config.CreateEntry(
				"FillStacksKey",
				"Q", // default hotkey for Fill Stacks
				description: "Hotkey for Fill Stacks (case-insensitive)."
			);
			MelonLogger.Msg($"EZInventory: FillStacksKey set to '{FillStacksKey.Value}'");

			FillStacksAutoClose = Config.CreateEntry(
				"FillStacksAutoClose",
				true, // default: do NOT auto-close on fill stacks
				description: "Automatically close the inventory after Fill Stacks."
			);

			ClearAllFilters = Config.CreateEntry(
				"ClearAllFilters",
				"J", // default hotkey for Clear All Filters
				description: "Hotkey for Clear All Filters (case-insensitive)."
			);
			MelonLogger.Msg($"EZInventory: ClearAllFilters set to '{ClearAllFilters.Value}'");

			CopyAllFilters = Config.CreateEntry(
				"CopyAllFilters",
				"K", // default hotkey for Copy All Filters
				description: "Hotkey for Copy All Filters (case-insensitive)."
			);
			MelonLogger.Msg($"EZInventory: CopyAllFilters set to '{CopyAllFilters.Value}'");

			PasteAllFilters = Config.CreateEntry(
				"PasteAllFilters",
				"L", // default hotkey for Paste All Filters
				description: "Hotkey for Paste All Filters (case-insensitive)."
			);
			MelonLogger.Msg($"EZInventory: PasteAllFilters set to '{PasteAllFilters.Value}'");

			MelonPreferences.Save();
		}
	}
}
