# EZInventory — Shift‑Drag, Move‑By‑Type, Grab All & Deposit All for Schedule One

A lightweight, dual‑runtime (Mono + IL2CPP) inventory enhancement mod for **Schedule I**.  
Adds several powerful quality‑of‑life features to make inventory management fast, intuitive, and satisfying.

---

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

### **Grab All**
Press your configured **Grab All hotkey** to instantly pull **every item** from the target inventory into your own.

- Respects stack limits  
- Smart merging behavior  
- Configurable auto‑close.

### **Deposit All**
Press your configured **Deposit All hotkey** to move **all items** from your inventory into the target container.

- Great for unloading loot  
- Automatically merges into existing stacks  
- Honors container rules and capacity  
- Configurable auto‑close.
- 
### **Fully Configurable Hotkeys**
All hotkeys — including Grab All, Deposit All, and the modifier keys for Shift‑Drag and Move‑By‑Type — can be customized in the mod’s config file.

---

## 🧩 Compatibility

- ✔ **Mono and IL2CPP runtime** builds included  
- ✔ Safe to add/remove at any time  
- ✔ Works on all inventory screens  
- Built and compiled against v0.4.2f9  

---

## 📦 Installation

1. Install **MelonLoader** for Schedule One (built with MelonLoader v0.7.0).  
2. Download the mod and extract the `.dll` for your Schedule I branch (Mono for main, IL2CPP for alternate).  
3. Drop the `.dll` into:
	/Schedule I/Mods/
4. Launch the game.

---

## 🔧 Technical Notes

- Uses Harmony patches on `ItemUIManager`  
- IL2CPP build uses native field/method accessors  
- Mono build uses reflection fallbacks  
- Shared logic ensures identical behavior across runtimes  
- Config file auto‑generates on first launch  

---

## 🧪 Known Issues

- None currently reported  
- If you find something odd or a bug, please report it.

---

## 📣 Support

If you enjoy the mod, consider endorsing it on Nexus or giving it a thumbs‑up on Thunderstore.  
Feedback is always welcome.