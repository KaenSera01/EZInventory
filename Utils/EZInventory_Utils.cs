using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;


#if MONO
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Items;
using System.Collections.Generic;
#elif IL2CPP
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.UI.Items;
using Il2CppSystem.Collections.Generic;
#endif

namespace EZInventory.Utils
{
	internal static class EZInventoryUtils
	{
#if MONO
        public static void MoveSlotContents(ItemUIManager mgr, ItemSlot source, List<ItemSlot> dest, bool fillEmptySlots = true, bool filterAware = false)
        {
            if (source.ItemInstance == null)
                return;

            int remaining = source.Quantity;
            int moved = 0;
			SlotFilter blankFilter = new SlotFilter();

			// Merge pass
			foreach (var t in dest)
            {
                if (remaining <= 0) break;
                if (t.ItemInstance == null) continue;
                if (!t.ItemInstance.CanStackWith(source.ItemInstance, false)) continue;
				if (t.IsLocked || t.IsAddLocked) continue;

                int cap = Math.Min(t.GetCapacityForItem(source.ItemInstance, false), remaining);
                if (cap <= 0) continue;

				if (filterAware && t.PlayerFilter != blankFilter && !t.DoesItemMatchPlayerFilters(source.ItemInstance))
					continue;

				t.AddItem(source.ItemInstance.GetCopy(cap), false);
                remaining -= cap;
                moved += cap;
            }

			if (fillEmptySlots)
			{
				// Empty slot pass
				foreach (var t in dest)
				{
					if (remaining <= 0) break;
					if (t.IsLocked || t.IsAddLocked) continue;

					int cap = Math.Min(t.GetCapacityForItem(source.ItemInstance, false), remaining);
					if (cap <= 0) continue;

					if (filterAware && t.PlayerFilter != blankFilter && !t.DoesItemMatchPlayerFilters(source.ItemInstance))
						continue;

					t.AddItem(source.ItemInstance.GetCopy(cap), false);
					remaining -= cap;
					moved += cap;
				}
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
		
		public static System.Collections.Generic.List<T> ToManagedList<T>(Il2CppSystem.Collections.Generic.List<T> il2cppList)
        {
            if (il2cppList == null)
                return null;

            var managed = new System.Collections.Generic.List<T>(il2cppList.Count);
            foreach (var item in il2cppList)
                managed.Add(item);

            return managed;
        }

		public static void MoveSlotContents(ItemUIManager mgr, ItemSlot source, System.Collections.Generic.List<ItemSlot> dest, bool fillEmptySlots = true, bool filterAware = false)
		{
			if (source.ItemInstance == null)
				return;

			int remaining = source.Quantity;
			int moved = 0;

			foreach (var t in dest)
			{
				if (remaining <= 0) break;
				if (t.ItemInstance == null) continue;
				if (!t.ItemInstance.CanStackWith(source.ItemInstance, false)) continue;

				int cap = Math.Min(t.GetCapacityForItem(source.ItemInstance, false), remaining);
				if (cap <= 0) continue;

				if (filterAware && !t.DoesItemMatchPlayerFilters(source.ItemInstance))
					continue;

				t.AddItem(source.ItemInstance.GetCopy(cap), false);
				remaining -= cap;
				moved += cap;
			}

			if (fillEmptySlots)
			{
				foreach (var t in dest)
				{
					if (remaining <= 0) break;

					int cap = Math.Min(t.GetCapacityForItem(source.ItemInstance, false), remaining);
					if (cap <= 0) continue;

					if (filterAware && !t.DoesItemMatchPlayerFilters(source.ItemInstance))
						continue;

					t.AddItem(source.ItemInstance.GetCopy(cap), false);
					remaining -= cap;
					moved += cap;
				}
			}

			// clean up
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

	public static class EZIClipboard
	{
#if MONO
		public static System.Collections.Generic.List<SlotFilter> CopiedFilters { get; private set; }

		public static void Copy(System.Collections.Generic.List<SlotFilter> filters)
		{
			if (filters == null)
			{
				CopiedFilters = null;
				return;
			}

			
			CopiedFilters = new System.Collections.Generic.List<SlotFilter>(filters.Count);
			foreach (var f in filters)
			{
				// allow null entries just in case
				CopiedFilters.Add(f != null ? f.Clone() : null);
			}
		}

#elif IL2CPP
        public static Il2CppSystem.Collections.Generic.List<SlotFilter> CopiedFilters { get; private set; }

        public static void Copy(Il2CppSystem.Collections.Generic.List<SlotFilter> filters)
        {
            if (filters == null)
            {
                CopiedFilters = null;
                return;
            }

            // Deep copy via SlotFilter.Clone()
            CopiedFilters = new Il2CppSystem.Collections.Generic.List<SlotFilter>(filters.Count);
            foreach (var f in filters)
            {
                CopiedFilters.Add(f != null ? f.Clone() : null);
            }
        }
#endif

		public static void ClearEZIClipboard()
		{
			CopiedFilters = null;
		}

		public static bool HasFilters =>
			CopiedFilters != null && CopiedFilters.Count > 0;
	}
}
