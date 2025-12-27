using HarmonyLib;
using System.Collections.Generic;
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
#elif IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.ItemFramework;
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
			var il2cppPrimary = __instance.PrimarySlots;
			var il2cppSecondary = __instance.SecondarySlots;

			List<ItemSlot> primarySlots = null;
			List<ItemSlot> secondarySlots = null;

			if (il2cppPrimary != null)
			{
				primarySlots = new List<ItemSlot>(il2cppPrimary.Count);
				foreach (var s in il2cppPrimary)
					primarySlots.Add(s);
			}

			if (il2cppSecondary != null)
			{
				secondarySlots = new List<ItemSlot>(il2cppSecondary.Count);
				foreach (var s in il2cppSecondary)
					secondarySlots.Add(s);
			}
#endif

			// determine source inventory
			bool inPrimary = primarySlots.Contains(clickedSlot);
			var sourceList = inPrimary ? primarySlots : secondarySlots;
			var destList = inPrimary ? secondarySlots : primarySlots;

			// get all items in the source inventory that match the type of the clicked item
			List<ItemSlot> matching = new List<ItemSlot>();
			foreach (var slot in sourceList)
			{
				if (slot.ItemInstance != null &&
					slot.ItemInstance.Definition == clickedItem.Definition)
				{
					matching.Add(slot);
				}
			}

			// Move each matching slot's contents
			foreach (var slot in matching)
			{
				EZInventoryUtils.MoveSlotContents(__instance, slot, destList);
			}

			__instance.onItemMoved?.Invoke();

			return false; // skip original SlotClicked
		}
	}


	[HarmonyPatch(typeof(ItemUIManager), "Update")]
	class EZInventory_ShiftDragMove_Patch
	{
		private static HashSet<ItemSlotUI> processed = new HashSet<ItemSlotUI>();
		private static readonly FieldInfo primaryField = AccessTools.Field(typeof(ItemUIManager), "PrimarySlots");
		private static readonly FieldInfo secondaryField = AccessTools.Field(typeof(ItemUIManager), "SecondarySlots");

		static void Postfix(ItemUIManager __instance)
		{
			if (!__instance.QuickMoveEnabled)
				return;

			// grab all
			if (EZInventoryMod.GrabAllKey != null)
			{
				string key = EZInventoryMod.GrabAllKey.Value.ToUpperInvariant();

				if (Enum.TryParse(key, true, out KeyCode grabKey))
				{
					if (Input.GetKeyDown(grabKey))
					{
						TakeEverything(__instance);

						//// auto-close if configured
						//if (EZInventoryMod.GrabAllAutoClose != null &&
						//	EZInventoryMod.GrabAllAutoClose.Value)
						//{
						//	__instance.gameObject.SetActive(false);
						//	return; //	assumes player cannot shift-drag and grab all in the same frame
						//}
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
						DepositAll(__instance);

						//if (EZInventoryMod.DepositAllAutoClose != null &&
						//	EZInventoryMod.DepositAllAutoClose.Value)
						//{
						//	__instance.gameObject.SetActive(false);
						//	return;	//	assumes player cannot shift-drag and deposit all in the same frame
						//}
					}
				}
				else
				{
					MelonLogger.Warning($"EZInventory: Invalid DepositAllKey '{dkey}'");
				}
			}

			
			// shift-click drag across items
			bool shiftHeld = GameInput.GetButton(GameInput.ButtonCode.QuickMove);
			bool lmbHeld = GameInput.GetButton(GameInput.ButtonCode.PrimaryClick);

			if (shiftHeld && lmbHeld)
			{
				var hovered = __instance.HoveredSlot;
				if (hovered != null && !processed.Contains(hovered))
				{
					processed.Add(hovered);
					QuickMoveSlot(__instance, hovered);
				}
			}
			else
			{
				// Reset when mouse or shift released
				processed.Clear();
			}
		}

		private static void QuickMoveSlot(ItemUIManager mgr, ItemSlotUI ui)
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
				if (!t.ItemInstance.CanStackWith(sourceSlot.ItemInstance, false)) continue;

				int cap = Mathf.Min(t.GetCapacityForItem(sourceSlot.ItemInstance, false),
									sourceSlot.Quantity - moved);

				if (cap <= 0) continue;

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
			var il2cppPrimary = mgr.PrimarySlots;
			var il2cppSecondary = mgr.SecondarySlots;

			List<ItemSlot> primarySlots = null;
			List<ItemSlot> secondarySlots = null;

			if (il2cppPrimary != null)
			{
				primarySlots = new List<ItemSlot>(il2cppPrimary.Count);
				foreach (var s in il2cppPrimary)
					primarySlots.Add(s);
			}

			if (il2cppSecondary != null)
			{
				secondarySlots = new List<ItemSlot>(il2cppSecondary.Count);
				foreach (var s in il2cppSecondary)
					secondarySlots.Add(s);
			}
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

				EZInventoryUtils.MoveSlotContents(mgr, slot, myInv);
			}

			mgr.onItemMoved?.Invoke();
		}

		private static void DepositAll(ItemUIManager mgr)
		{
			if (!mgr.QuickMoveEnabled)
				return;

#if MONO
			var primarySlots = primaryField.GetValue(mgr) as List<ItemSlot>;
			var secondarySlots = secondaryField.GetValue(mgr) as List<ItemSlot>;
#elif IL2CPP
			var il2cppPrimary = mgr.PrimarySlots;
			var il2cppSecondary = mgr.SecondarySlots;

			List<ItemSlot> primarySlots = null;
			List<ItemSlot> secondarySlots = null;

			if (il2cppPrimary != null)
			{
				primarySlots = new List<ItemSlot>(il2cppPrimary.Count);
				foreach (var s in il2cppPrimary)
					primarySlots.Add(s);
			}

			if (il2cppSecondary != null)
			{
				secondarySlots = new List<ItemSlot>(il2cppSecondary.Count);
				foreach (var s in il2cppSecondary)
					secondarySlots.Add(s);
			}
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

				EZInventoryUtils.MoveSlotContents(mgr, slot, theirInv);
			}

			mgr.onItemMoved?.Invoke();
		}
	}
}
