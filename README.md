# Dev Log

## 2025-05-04
- Integrated the start menu into the main game flow by merging its logic into RoomManager, enabling seamless room loading with a transition animation on launch.
- Successfully imported new models exported from MagicaVoxel; resolved the excessive brightness issue by adjusting global lighting intensity, avoiding material or shader changes.
- Calibrated the isometric camera system to preserve object scale and ratio under orthographic projection, ensuring accurate coordinate mapping without distortion.
- Refactored the room configuration to use a list of preconfigured button prefabs, replacing the legacy icon/model pair structure and simplifying item binding.
- Standardized button layout in the item box by forcing consistent RectTransform properties, ensuring all buttons display at a fixed 300×300 size without compression.
- Implemented animated wall transitions with WallVisibilityController, smoothly toggling wall visibility based on the active camera view for improved spatial clarity.
- Centralized icon visibility control within SetSpawned(bool), ensuring item buttons correctly hide when an item is spawned and reappear when the item is removed.
- Reinstated grid snapping by delegating TrySnapByEdge() to ItemAutoDestroy.StopDragging(), restoring accurate placement after drag-and-drop operations.
- Fixed subtitle feedback not appearing for newly added prefabs by generalizing the name check, leaving hooks for future feng shui-specific feedback customization.
- Fully integrated the new basketball prefab, validating drag, snap, feedback, icon control, and item slot behavior against the existing bed item pipeline.

## 2025-05-02
- Merged Feedback Text System. Integrated the placement feedback system into the main architecture. After placing an item, a short message appears at the top of the screen indicating success or failure, adding emotional feedback.
- Legality Check Aligned with Visual Highlight. Replaced hardcoded position checks with a legality flag derived from the placement highlight color, ensuring consistent logic and visual cues.
- CHI Progress Bar Finalized. Completed integration of the CHI fill bar. The bar visually reflects CHI updates in real time with a smooth fill animation. Max value is 9 for quick visual scaling.
- Fix: CHI Not Updating on Deletion. Resolved an issue where deleted items didn’t reduce the CHI immediately. Items are now detached before being destroyed, ensuring accurate scoring and UI updates

## 2025-04-28
- Developed dynamic generation of ItemBox buttons, prefab binding and instantiation, as well as object dragging with floor grid snapping and highlight display for placement zones.
- Improved the ItemBox interface with pixel art background images, button icons, and title text.
- Separated the loading page and functional UI into independent canvases with proper rendering order, ensuring the loading screen overlays 3D models correctly without interference.
- Completed the multi-camera switching logic, synchronized Canvas worldCamera references upon each switch.

## 2025-04-24
- Initial commit of .obj-files and PNG image files for the game.
    - Created in MagicaVoxel 0.99.7.1.
    - Basketball, bed, couch, room & table.
    - A work-in-progress that'll most likely require tweaking as development of the game continues.
    - More assets are being worked on.

## 2025.04.16
- Features Implemented
1. Implemented automatic snapping of dragged objects to the floor grid upon mouse release.  
2. Supported custom snapping anchors (e.g., top-right corner alignment) for precise placement.  
3. Enabled object rotation by 90 degrees during dragging via right-click, with dynamic projection updates.  
4. Ensured accurate screen-to-world coordinate mapping for object dragging under different camera angles.  
5. Supported grid configuration with adjustable direction, center offset, cell size, and dimensions.  
6. Temporarily disabled object colliders during dragging to avoid projection interference, and re-enabled them upon release.  
7. Added object auto-destruction logic when placed outside room boundaries, including item box reset handling.  
- Bugs Fixed & Development Notes
1. Fixed an issue where newly instantiated objects would embed into the floor due to uninitialized size.  
2. Note: Accessing bounds.size from a disabled collider yields invalid values; colliders must be enabled first.  
3. Fixed a bug where released objects could no longer be clicked or dragged due to colliders remaining disabled.  
4. Fixed incorrect corner offset calculations caused by flawed direction vector logic in snapping.  
5. Note: After rotating an object, its bounds.size must be recalculated to reflect the updated orientation.  
6. Fixed a bug where the highlight sprite appeared oversized due to improper PPU and scale configuration.  
7. Fixed incorrect out-of-bounds detection caused by evaluating the pre-snapped floating position.  
8. Fixed snapping failures near room boundaries caused by collider interference.  
9. Note: Prefabs cannot reference scene-specific components directly (e.g., FloorGrid) and must bind via lookup in Start().

## 2025.04.14
- Branch Main is created and set as default.
- Latest 3D Demo is committed to Main.
- 2D Demo can be found in Branch Master.
