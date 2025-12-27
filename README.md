# EZInventory — Shift‑Drag & Move‑By‑Type for Schedule One

A lightweight, dual‑runtime (Mono + IL2CPP) inventory enhancement mod for **Schedule I**.  
Adds two powerful quality‑of‑life features:

## ✨ Features

### **Shift‑Drag Quick Move**
Hold **Shift + Left Click** and sweep your cursor across items to rapidly move them between inventories.

- Works across all inventory screens
- Automatically merges stacks
- Fills empty slots intelligently
- Smooth, continuous movement while dragging

### **Ctrl + Shift Move‑By‑Type**
Hold **Ctrl + Shift** and click an item to move **all items of that type** from one inventory to the other.

- Perfect for sorting loot
- Moves stacks and partial stacks
- Preserves stack limits and item rules

---

## 🧩 Compatibility

- ✔ **Mono and IL2CPP runtime** are available for download. 
- ✔ Safe to add/remove at any time
- Built and compiled against v0.4.2f8.

---

## 📦 Installation

1. Install **MelonLoader** for Schedule One (built with MelonLoader v0.7.1)
2. Download the mod and extract the `.dll` file for your Schedule I branch (Mono for main, IL2CPP for alternate).
3. Drop the `.dll` into:

```
Schedule I/Mods/
```

4. Launch the game

---

## 🔧 Technical Notes

- Uses Harmony patches on `ItemUIManager`
- IL2CPP build uses native field/method accessors
- Mono build uses reflection fallbacks
- Shared logic ensures identical behavior across runtimes

---

## 🧪 Known Issues

- None currently reported  
- If you find something odd or a bug, please report it.

---

## 📣 Support

If you enjoy the mod, consider endorsing it on Nexus or giving it a thumbs‑up on Thunderstore. Feedback is always welcome.