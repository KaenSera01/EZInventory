using HarmonyLib;
using UnityEngine;
using MelonLoader;
using System.Reflection;
using EZInventory.Utils;
using System;


#if MONO
using ScheduleOne;
using ScheduleOne.UI;
using ScheduleOne.UI.Items;
using ScheduleOne.ItemFramework;
using System.Collections.Generic;
#elif IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.ItemFramework;
using Il2CppSystem.Collections.Generic;
#endif

namespace EZInventory.HarmonyPatches
{
	[HarmonyPatch(typeof(ItemUIManager), "SlotClicked")]
	class EZInventory_MoveByType_Patch
	{
		private static readonly FieldInfo primaryField = AccessTools.Field(typeof(ItemUIManager), "PrimarySlots");
		private static readonly FieldInfo secondaryField = AccessTools.Field(typeof(ItemUIManager), "SecondarySlots");

		static bool Prefix(ItemUIManager __instance, ItemSlotUI ui)
		{
			bool shiftHeld = GameInput.GetButton(GameInput.ButtonCode.QuickMove);
			bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

			if (!(shiftHeld && ctrlHeld))
				return true;

			if (!__instance.QuickMoveEnabled)
				return true;

			var clickedSlot = ui.assignedSlot;
			var clickedItem = clickedSlot.ItemInstance;
			if (clickedItem == null)
				return true;

			// skip cash for now
			if (clickedItem.Definition.Name == "Cash")
				return true;

			// get the to and from slots, dependent on build config
#if MONO
			var primarySlots = primaryField.GetValue(__instance) as List<ItemSlot>;
			var secondarySlots = secondaryField.GetValue(__instance) as List<ItemSlot>;
#elif IL2CPP
			var primarySlots = EZInventoryUtils.ToManagedList<ItemSlot>(__instance.PrimarySlots);
			var secondarySlots = EZInventoryUtils.ToManagedList<ItemSlot>(__instance.SecondarySlots);
#endif

			// determine source inventory
			bool inPrimary = primarySlots.Contains(clickedSlot);
			var sourceList = inPrimary ? primarySlots : secondarySlots;
			var destList = inPrimary ? secondarySlots : primarySlots;

			// get all items in the source inventory that match the type of the clicked item
			List<ItemSlot> matching = new List<ItemSlot>();
			foreach (var slot in sourceList)
			{
				if (!slot.IsLocked && 
					!slot.IsRemovalLocked &&
					slot.ItemInstance != null &&
					slot.ItemInstance.Definition == clickedItem.Definition)
				{
					matching.Add(slot);
				}
			}

			// Move each matching slot's contents
			foreach (var slot in matching)
			{
				EZInventoryUtils.MoveSlotContents(__instance, slot, destList, true, ctrlHeld);
			}

			__instance.onItemMoved?.Invoke();

			return false; // skip original SlotClicked
		}
	}


	[HarmonyPatch(typeof(ItemUIManager), "Update")]
	class EZInventory_Input_Patch
	{
		private static System.Collections.Generic.HashSet<ItemSlotUI> processed = new System.Collections.Generic.HashSet<ItemSlotUI>();
		private static readonly FieldInfo primaryField = AccessTools.Field(typeof(ItemUIManager), "PrimarySlots");
		private static readonly FieldInfo secondaryField = AccessTools.Field(typeof(ItemUIManager), "SecondarySlots");

#if MONO
    private static readonly FieldInfo itemSlotOwnerField =
        AccessTools.Field(typeof(ItemUIManager), "ItemSlotOwner"); // or whatever the actual name is
#elif IL2CPP
		private static readonly FieldInfo itemSlotOwnerField =
			AccessTools.Field(typeof(ItemUIManager), "ItemSlotOwner");
#endif

		private static IItemSlotOwner GetCurrentOwner(ItemUIManager mgr)
		{
			if (itemSlotOwnerField == null)
				return null;

			return itemSlotOwnerField.GetValue(mgr) as IItemSlotOwner;
		}

		private static System.Collections.Generic.List<ItemSlot> GetContainerSlots(ItemUIManager mgr)
		{
#if MONO
			var secondarySlots = secondaryField.GetValue(mgr) as System.Collections.Generic.List<ItemSlot>;
			return secondarySlots;
#elif IL2CPP
			// mgr.SecondarySlots is Il2CppSystem.Collections.Generic.List<ItemSlot>
			return EZInventoryUtils.ToManagedList<ItemSlot>(mgr.SecondarySlots);
#endif
		}

		static void Postfix(ItemUIManager __instance)
		{
			if (__instance == null)
				return;

			if (!__instance.QuickMoveEnabled)
				return;

			if (StorageMenu.Instance == null)
				return;

			bool shiftHeld = GameInput.GetButton(GameInput.ButtonCode.QuickMove);   //	shift
			bool ctrlHeld = GameInput.GetButton(GameInput.ButtonCode.Crouch);		//	ctrl
			bool lmbHeld = GameInput.GetButton(GameInput.ButtonCode.PrimaryClick);  //	left mouse button

			// grab all
			if (EZInventoryMod.GrabAllKey != null)
			{
				string key = EZInventoryMod.GrabAllKey.Value.ToUpperInvariant();

				if (Enum.TryParse(key, true, out KeyCode grabKey))
				{
					if (Input.GetKeyDown(grabKey))
					{
						TakeEverything(__instance);

						// auto-close if configured
						if (EZInventoryMod.GrabAllAutoClose != null &&
							EZInventoryMod.GrabAllAutoClose.Value)
						{
							var storage = StorageMenu.Instance;
							if (storage != null && storage.OpenedStorageEntity != null)	
								StorageMenu.Instance.OpenedStorageEntity.Close();

							return; //	assumes player cannot shift-drag and grab all in the same frame
						}
					}
				}
				else
				{
					MelonLogger.Warning($"EZInventory: Invalid GrabAllKey '{key}'");
				}
			}

			// deposit all
			if (EZInventoryMod.DepositAllKey != null)
			{
				string dkey = EZInventoryMod.DepositAllKey.Value.ToUpperInvariant();

				if (Enum.TryParse(dkey, true, out KeyCode depositKey))
				{
					if (Input.GetKeyDown(depositKey))
					{
						DepositAll(__instance, ctrlHeld);

						if (EZInventoryMod.DepositAllAutoClose != null &&
							EZInventoryMod.DepositAllAutoClose.Value)
						{
							if (StorageMenu.Instance != null && StorageMenu.Instance.OpenedStorageEntity != null)
								StorageMenu.Instance.OpenedStorageEntity.Close();

							return; //	assumes player cannot shift-drag and deposit all in the same frame
						}
					}
				}
				else
				{
					MelonLogger.Warning($"EZInventory: Invalid DepositAllKey '{dkey}'");
				}
			}

			if (EZInventoryMod.FillStacksKey != null)
			{
				string dkey = EZInventoryMod.FillStacksKey.Value.ToUpperInvariant();

				if (Enum.TryParse(dkey, true, out KeyCode FillStacksKey))
				{
					if (Input.GetKeyDown(FillStacksKey))
					{
						// Fill stacks
						FillStacks(__instance);

						if (EZInventoryMod.FillStacksAutoClose != null &&
							EZInventoryMod.FillStacksAutoClose.Value)
						{
							if (StorageMenu.Instance != null && StorageMenu.Instance.OpenedStorageEntity != null) 
								StorageMenu.Instance.OpenedStorageEntity.Close();

							return; //	assumes player cannot shift-drag and deposit all in the same frame
						}
					}

				}
			}

			if (EZInventoryMod.CopyAllFilters != null)
			{
				string key = EZInventoryMod.CopyAllFilters.Value.ToUpperInvariant();
				if (Enum.TryParse(key, true, out KeyCode copyKey))
				{
					if (Input.GetKeyDown(copyKey))
					{
						var itemSlots = GetContainerSlots(__instance);
						if (itemSlots == null)
							return;

#if MONO
						var slotFilters = new System.Collections.Generic.List<SlotFilter>(itemSlots.Count);
						foreach (var slot in itemSlots)
						{
							if (slot.CanPlayerSetFilter)
								slotFilters.Add(slot.PlayerFilter != null ? slot.PlayerFilter.Clone() : null);
							else
								slotFilters.Add(null);
						}
						EZIClipboard.Copy(slotFilters);
#elif IL2CPP
						var slotFilters = new Il2CppSystem.Collections.Generic.List<SlotFilter>(itemSlots.Count);
						foreach (var slot in itemSlots)
						{
							if (slot.CanPlayerSetFilter)
								slotFilters.Add(slot.PlayerFilter != null ? slot.PlayerFilter.Clone() : null);
							else
								slotFilters.Add(null);
						}
						EZIClipboard.Copy(slotFilters);
#endif
					}
				}
			}

			if (EZInventoryMod.PasteAllFilters != null)
			{
				string key = EZInventoryMod.PasteAllFilters.Value.ToUpperInvariant();
				if (Enum.TryParse(key, true, out KeyCode pasteKey))
				{
					if (Input.GetKeyDown(pasteKey))
					{
						if (!EZIClipboard.HasFilters)
							return;

						var itemSlots = GetContainerSlots(__instance);
						if (itemSlots == null)
							return;

#if MONO
						var copied = EZIClipboard.CopiedFilters; // List<SlotFilter>
						int count = Math.Min(copied.Count, itemSlots.Count);

						for (int i = 0; i < count; i++)
						{
							var slot = itemSlots[i];
							if (!slot.CanPlayerSetFilter)
								continue;

							var copiedFilter = copied[i];
							if (copiedFilter == null)
								slot.SetPlayerFilter(new SlotFilter());
							else
								slot.SetPlayerFilter(copiedFilter.Clone());
						}
#elif IL2CPP
						var copied = EZIClipboard.CopiedFilters; // Il2CppSystem.Collections.Generic.List<SlotFilter>
						int count = Math.Min(copied.Count, itemSlots.Count);

						for (int i = 0; i < count; i++)
						{
							var slot = itemSlots[i];
							if (!slot.CanPlayerSetFilter)
								continue;

							var copiedFilter = copied[i];
							if (copiedFilter == null)
								slot.SetPlayerFilter(new SlotFilter());
							else
								slot.SetPlayerFilter(copiedFilter.Clone());
						}
#endif
						__instance.onItemMoved?.Invoke();
					}
				}
			}

			if (EZInventoryMod.ClearAllFilters != null)
			{
				string key = EZInventoryMod.ClearAllFilters.Value.ToUpperInvariant();
				if (Enum.TryParse(key, true, out KeyCode clearKey))
				{
					if (Input.GetKeyDown(clearKey))
					{
						var itemSlots = GetContainerSlots(__instance);
						if (itemSlots == null)
							return;

						foreach (var slot in itemSlots)
						{
							if (slot.CanPlayerSetFilter)
								slot.SetPlayerFilter(new SlotFilter());
						}

						__instance.onItemMoved?.Invoke();
					}
				}
			}

			// shift-click drag across items
			if (shiftHeld && lmbHeld && __instance.QuickMoveEnabled)
			{
				var hovered = __instance.HoveredSlot;
				if (hovered != null && !processed.Contains(hovered))
				{
					processed.Add(hovered);
					QuickMoveSlot(__instance, hovered, ctrlHeld);
				}
			}
			else
			{
				// Reset when mouse or shift released
				processed.Clear();
			}
		}

		private static void QuickMoveSlot(ItemUIManager mgr, ItemSlotUI ui, bool filterAware = false)
		{
			if (!mgr.CanDragFromSlot(ui))
				return;

			var sourceSlot = ui.assignedSlot;

			// BLOCK CASH
			if (sourceSlot.ItemInstance != null && sourceSlot.ItemInstance.Definition.Name == "Cash")
				return;

#if MONO
			var method = AccessTools.Method(typeof(ItemUIManager), "GetQuickMoveSlots");
			var targets = method.Invoke(mgr, new object[] { sourceSlot }) as List<ItemSlot>;
#elif IL2CPP
			var il2cppTargets = mgr.GetQuickMoveSlots(sourceSlot);
			List<ItemSlot> targets = null;

			if (il2cppTargets != null)
			{
				targets = new List<ItemSlot>(il2cppTargets.Count);
				foreach (var slot in il2cppTargets)
					targets.Add(slot);
			}
#endif

			if (targets == null || targets.Count == 0)
				return;

			int moved = 0;

			// Merge into existing stacks
			foreach (var t in targets)
			{
				if (moved >= sourceSlot.Quantity) break;
				if (t.ItemInstance == null) continue;
				if (t.IsLocked || t.IsAddLocked || t.IsRemovalLocked) continue;
				if (!t.ItemInstance.CanStackWith(sourceSlot.ItemInstance, false)) continue;

				int cap = Mathf.Min(t.GetCapacityForItem(sourceSlot.ItemInstance, false),
									sourceSlot.Quantity - moved);

				if (cap <= 0) continue;

				if (filterAware && !t.DoesItemMatchPlayerFilters(sourceSlot.ItemInstance))
					continue;

				if (sourceSlot.IsRemovalLocked || sourceSlot.IsLocked)
					continue;

				t.AddItem(sourceSlot.ItemInstance.GetCopy(cap), false);
				moved += cap;
			}

			// Fill empty slots
			foreach (var t in targets)
			{
				if (moved >= sourceSlot.Quantity) break;

				int cap = Mathf.Min(t.GetCapacityForItem(sourceSlot.ItemInstance, false),
									sourceSlot.Quantity - moved);

				if (cap <= 0) continue;
				
				if (sourceSlot.IsRemovalLocked || sourceSlot.IsLocked)
					continue;
				
				if (filterAware && !t.DoesItemMatchPlayerFilters(sourceSlot.ItemInstance))
					continue;

				t.AddItem(sourceSlot.ItemInstance.GetCopy(cap), false);
				moved += cap;
			}

			if (moved > 0)
			{
				sourceSlot.ChangeQuantity(-moved, false);
				mgr.onItemMoved?.Invoke();
			}
		}

		private static void TakeEverything(ItemUIManager mgr)
		{
			if (!mgr.QuickMoveEnabled)
				return;

#if MONO
			var primarySlots = primaryField.GetValue(mgr) as List<ItemSlot>;
			var secondarySlots = secondaryField.GetValue(mgr) as List<ItemSlot>;
#elif IL2CPP
			var primarySlots = EZInventoryUtils.ToManagedList<ItemSlot>(mgr.PrimarySlots);
			var secondarySlots = EZInventoryUtils.ToManagedList<ItemSlot>(mgr.SecondarySlots);
#endif

			//	take from secondary → primary
			var myInv = primarySlots;
			var theirInv = secondarySlots;

			if (myInv == null || theirInv == null)
				return;

			foreach (var slot in theirInv)
			{
				if (slot.ItemInstance == null)
					continue;

				// Skip cash for now
				if (slot.ItemInstance.Definition.Name == "Cash")
					continue;

				if (slot.IsRemovalLocked || slot.IsLocked)
					continue;

				EZInventoryUtils.MoveSlotContents(mgr, slot, myInv);
			}

			mgr.onItemMoved?.Invoke();
		}

		private static void DepositAll(ItemUIManager mgr, bool filterAware = false)
		{
#if MONO
			var primarySlots = primaryField.GetValue(mgr) as List<ItemSlot>;
			var secondarySlots = secondaryField.GetValue(mgr) as List<ItemSlot>;
#elif IL2CPP
			var primarySlots = EZInventoryUtils.ToManagedList<ItemSlot>(mgr.PrimarySlots);
			var secondarySlots = EZInventoryUtils.ToManagedList<ItemSlot>(mgr.SecondarySlots);
#endif

			// deposit from primary → secondary
			var myInv = primarySlots;
			var theirInv = secondarySlots;

			if (myInv == null || theirInv == null)
				return;

			foreach (var slot in myInv)
			{
				if (slot.ItemInstance == null)
					continue;

				// Skip cash for now
				if (slot.ItemInstance.Definition.Name == "Cash")
					continue;

				if (slot.IsAddLocked || slot.IsLocked)
					continue;

				EZInventoryUtils.MoveSlotContents(mgr, slot, theirInv, true, filterAware);
			}

			mgr.onItemMoved?.Invoke();
		}

		private static void FillStacks(ItemUIManager mgr)
		{
			if (!mgr.QuickMoveEnabled)
				return;

#if MONO
			var primarySlots = primaryField.GetValue(mgr) as List<ItemSlot>;
			var secondarySlots = secondaryField.GetValue(mgr) as List<ItemSlot>;
#elif IL2CPP
			var primarySlots = EZInventoryUtils.ToManagedList<ItemSlot>(mgr.PrimarySlots);
			var secondarySlots = EZInventoryUtils.ToManagedList<ItemSlot>(mgr.SecondarySlots);
#endif

			//	take from secondary → primary
			var playerInv = primarySlots;
			var storageInv = secondarySlots;

			if (storageInv == null || playerInv == null)
				return;

			foreach (var slot in storageInv)
			{
				if (slot.ItemInstance == null)
					continue;

				// Skip cash for now
				if (slot.ItemInstance.Definition.Name == "Cash")
					continue;

				if (slot.IsRemovalLocked || slot.IsLocked)
					continue;

				EZInventoryUtils.MoveSlotContents(mgr, slot, playerInv, false);
			}

			mgr.onItemMoved?.Invoke();
		}
	}
}
