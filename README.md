# Dev Log

## 2025.04.14
- Branch Main is created and set as default.
- Latest 3D Demo is committed to Main.
- 2D Demo can be found in Branch Master.

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

## 2025-04-24
- Initial commit of .obj-files and PNG image files for the game.
    - Created in MagicaVoxel 0.99.7.1.
    - Basketball, bed, couch, room & table.
    - A work-in-progress that'll most likely require tweaking as development of the game continues.
    - More assets are being worked on.
    
