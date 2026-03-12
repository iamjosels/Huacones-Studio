# Cutting Module

This folder contains the active cutting minigame code.

## Runtime
- `DalgonaGameManager.cs`
  - Main gameplay loop for the Dalgona minigame.
  - Supports manual path authoring per round.
  - Handles input, validation, feedback, scoring, and finish callbacks.
- `FadeOut.cs`
  - Generic UI fade helper used by multiple scenes/prefabs.

## Editor
- `Editor/DalgonaGameManagerEditor.cs`
  - Round-based authoring tools in Inspector.
  - Figure preview per round.
  - Scene view path visualization for point placement.

## Cleanup Notes
- Removed obsolete legacy objects/components from the Dalgona prefab.
- Legacy scripts were reduced to compatibility stubs:
  - `CandyCuttingManager`
  - `TraceDrawer`
  - `TracePointSpawner`
  - `PixelColorDebugger`
- These stubs prevent local project-file compile issues and are intentionally not used by active scenes/prefabs.

## Team Guidelines
- Keep gameplay logic in `DalgonaGameManager`.
- Keep authoring-only tooling under `Editor/`.
- Avoid adding debug-only runtime scripts to production prefabs.
