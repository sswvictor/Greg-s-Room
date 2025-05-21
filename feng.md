# Feng Shui Logic System: Implementation Plan (`feng.md`)

## 1. **Goal**

Implement a flexible Feng Shui logic system that evaluates furniture placement according to proximity-based rules (as defined by your teammate), with results output to the console for now. The system should work dynamically with whatever items are present in the room, as configured in the Unity Editor via the RoomManager's "rooms" and "Button Prefabs" properties.

---

## 2. **Dynamic Room & Item Setup**

- **RoomManager** holds a list of `RoomData` objects, each representing a room (e.g., College room).
- Each `RoomData` has a list of "Button Prefabs"—these are the furniture items available for that room.
- **Furniture is not hardcoded in scripts**: Items are added/removed in the Unity Editor by modifying the "Button Prefabs" list for each room.
- **Feng Shui logic must work for any set of items** present in the current room.

---

## 3. **Current Rule Set (as per teammate's logic)**

### **Bed**
- **Headboard Support:** The head of the bed must be touching or very close to a wall (raycast from headboard, max distance 0.5 units).
- **Commanding Position:** (Not yet implemented—focus on proximity for now.)

### **Couch**
- **Backing Support:** The back of the couch should be flush against a wall (raycast from back, max distance 0.5 units).
- **Open Space in Front:** (Not yet implemented.)

### **Bookshelf**
- **Stable Placement:** Should be close to a wall (raycast from back, max distance 0.5 units).
- **Light Access:** (Not yet implemented.)

*For now, only proximity-to-wall checks are required. Other rules can be added later.*

---

## 4. **How Walls Are Set Up (College Room)**

- **Walls are managed by the `WallGrid` component** in the room prefab.
- Each wall is a GameObject with a Collider and (ideally) the tag "Wall".
- The `WallGrid` provides grid snapping and boundary logic, but for proximity checks, use Physics raycasts against objects tagged "Wall".

---

## 5. **Implementation Approach**

### **A. FengShuiLogic Component**

- Attach a `FengShuiLogic` script to each major furniture prefab (Bed, Couch, Bookshelf, etc.).
- Set the `objectType` field in the Inspector (e.g., "Bed", "Couch", "Bookshelf").
- For beds, assign the `headTransform` (the headboard position).

### **B. Proximity Check Logic**

- On placement (or when requested), the script should:
  - For each item, if it requires a wall proximity check:
    - Cast a short ray (0.5 units) from the relevant "back" transform in the appropriate direction.
    - If the ray hits a collider tagged "Wall", log a success message.
    - Otherwise, log a warning/failure message.

- **Example (Bed):**
  ```csharp
  if (objectType == "Bed" && headTransform != null) {
      if (Physics.Raycast(headTransform.position, -transform.forward, out RaycastHit hit, 0.5f)) {
          if (hit.collider.CompareTag("Wall")) {
              Debug.Log("[FengShui] Bed headboard is against the wall.");
          } else {
              Debug.Log("[FengShui] Bed headboard is NOT against a wall.");
          }
      } else {
          Debug.Log("[FengShui] Bed headboard is NOT near any wall.");
      }
  }
  ```

- **Example (Couch/Bookshelf):**
  ```csharp
  if ((objectType == "Couch" || objectType == "Bookshelf")) {
      Vector3 back = -transform.forward; // Adjust if needed for prefab orientation
      if (Physics.Raycast(transform.position, back, out RaycastHit hit, 0.5f)) {
          if (hit.collider.CompareTag("Wall")) {
              Debug.Log($"[FengShui] {objectType} back is against the wall.");
          } else {
              Debug.Log($"[FengShui] {objectType} back is NOT against a wall.");
          }
      } else {
          Debug.Log($"[FengShui] {objectType} back is NOT near any wall.");
      }
  }
  ```

### **C. Dynamic Evaluation**

- The logic should be called for each item in the room, regardless of how many or which types are present.
- This is already supported by the current `CHIScoreManager` and `ItemAutoDestroy` flow, which iterates over all placed items and calls their `FengShuiLogic` if present.

---

## 6. **Console Output (for Now)**

- All results should be output to the Unity Console using `Debug.Log`.
- Example output:
  ```
  [FengShui] Bed headboard is against the wall.
  [FengShui] Couch back is NOT against a wall.
  [FengShui] Bookshelf back is against the wall.
  ```

---

## 7. **Extensibility**

- The system is designed so that new rules (e.g., line-of-sight, open space, light access) can be added as new methods in `FengShuiLogic`.
- The evaluation logic is not hardcoded to specific objects; it works with whatever is present in the room, as configured in the Editor.

---

## 8. **Step-by-Step Blocks to Rectify and Expand Implementation**

### **Block 1: Wall Tagging and Setup**
- Ensure all wall GameObjects in the College room prefab have the tag "Wall".
- Verify each wall has a Collider component.

### **Block 2: Attach and Configure FengShuiLogic**
- Attach the `FengShuiLogic` script to each major furniture prefab (Bed, Couch, Bookshelf, etc.).
- Set the `objectType` field appropriately in the Inspector.
- For beds, assign the `headTransform` to the headboard.

### **Block 3: Implement Proximity Checks**
- In `FengShuiLogic`, implement the proximity-to-wall logic for each object type as described above.
- Use `Debug.Log` to output the result of each check.

### **Block 4: Dynamic Evaluation on Placement**
- Ensure that after placement (via `ItemAutoDestroy` or similar), the `EvaluateFengShuiScore()` method is called for each item with a `FengShuiLogic` component.
- Confirm that the console output matches expectations for each item placed.

### **Block 5: Prepare for Future Expansion**
- Structure the code so that new rules (e.g., line-of-sight, open space, light access) can be added as new methods in `FengShuiLogic`.
- Keep the evaluation logic generic and data-driven, not hardcoded to specific objects.

---

## 9. **References**

- `FengShuiLogic.cs` (proximity check logic)
- `CHIScoreManager.cs` (dynamic evaluation of all items)
- `WallGrid.cs` (wall setup and grid logic)
- `RoomManager.cs` (dynamic room/item setup)

---

**This approach ensures the Feng Shui logic is dynamic, editor-driven, and ready for incremental expansion.**  
*Start with proximity checks and console output; add more rules and UI feedback as needed.* 