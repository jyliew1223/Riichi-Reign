# 🀄 Riichi Reign (Working Title)

**Riichi Reign** is a 2D Riichi Mahjong engine built in **Unity**, featuring a robust host-client architecture. This project prioritizes an authentic representation of traditional Riichi rules, modular code design, and a modern UI experience.

---

## 🎯 Project Overview

This project aims to create a highly extensible Mahjong framework. While Phase 1 establishes the multiplayer foundations and core rule enforcement, the architecture is specifically designed to support **ML-Agents** for advanced AI training in subsequent development phases.

### Key Technical Pillars:

- **Polymorphic Design:** Flexible `Meld` and `Tile` classes for easy rule modification.
- **UI Toolkit:** Leveraging Unity’s modern UXML/USS workflow for a crisp, responsive interface.
- **Authoritative Server Logic:** Ensuring game state integrity across host-client connections.

---

## 🕹️ Features (Phase 1)

- [-] **Full Turn-Based Loop:** Drawing, discarding, and turn-switching logic.
- [x] **Action System:** Validated implementation of **Chi**, **Pon**, **Kan**, and **Riichi**.
- [x] **Yaku Evaluation:** Core logic for standard winning hands and point calculation.
- **Multiplayer Sync:** Initial host-client connectivity for remote play.
- **Dynamic Hand UI:** Interactive tile management using UIToolkit.

---
