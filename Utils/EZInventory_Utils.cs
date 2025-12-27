using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if MONO
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Items;
#elif IL2CPP
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.UI.Items;
#endif

namespace EZInventory.Utils
{
	internal static class EZInventoryUtils
	{
#if MONO
        public static void MoveSlotContents(ItemUIManager mgr, ItemSlot source, List<ItemSlot> dest)
        {
            if (source.ItemInstance == null)
                return;

            int remaining = source.Quantity;
            int moved = 0;

            // Merge pass
            foreach (var t in dest)
            {
                if (remaining <= 0) break;
                if (t.ItemInstance == null) continue;
                if (!t.ItemInstance.CanStackWith(source.ItemInstance, false)) continue;

                int cap = Math.Min(t.GetCapacityForItem(source.ItemInstance, false), remaining);
                if (cap <= 0) continue;

                t.AddItem(source.ItemInstance.GetCopy(cap), false);
                remaining -= cap;
                moved += cap;
            }

            // Empty slot pass
            foreach (var t in dest)
            {
                if (remaining <= 0) break;

                int cap = Math.Min(t.GetCapacityForItem(source.ItemInstance, false), remaining);
                if (cap <= 0) continue;

                t.AddItem(source.ItemInstance.GetCopy(cap), false);
                remaining -= cap;
                moved += cap;
            }

            // Safe cleanup
            if (moved > 0)
            {
                if (remaining <= 0)
                    source.ClearStoredInstance(false);
                else
                    source.ChangeQuantity(-moved, false);

                mgr.onItemMoved?.Invoke();
            }
        }
#elif IL2CPP
		public static void MoveSlotContents(Il2CppScheduleOne.UI.Items.ItemUIManager mgr, Il2CppScheduleOne.ItemFramework.ItemSlot source, List<Il2CppScheduleOne.ItemFramework.ItemSlot> dest)
		{
			if (source.ItemInstance == null)
				return;

			int remaining = source.Quantity;
			int moved = 0;

			// Merge pass
			foreach (var t in dest)
			{
				if (remaining <= 0) break;
				if (t.ItemInstance == null) continue;
				if (!t.ItemInstance.CanStackWith(source.ItemInstance, false)) continue;

				int cap = Math.Min(t.GetCapacityForItem(source.ItemInstance, false), remaining);
				if (cap <= 0) continue;

				t.AddItem(source.ItemInstance.GetCopy(cap), false);
				remaining -= cap;
				moved += cap;
			}

			// Empty slot pass
			foreach (var t in dest)
			{
				if (remaining <= 0) break;

				int cap = Math.Min(t.GetCapacityForItem(source.ItemInstance, false), remaining);
				if (cap <= 0) continue;

				t.AddItem(source.ItemInstance.GetCopy(cap), false);
				remaining -= cap;
				moved += cap;
			}

			// Safe cleanup
			if (moved > 0)
			{
				if (remaining <= 0)
					source.ClearStoredInstance(false);
				else
					source.ChangeQuantity(-moved, false);

				mgr.onItemMoved?.Invoke();
			}
		}
#endif
	}
}
