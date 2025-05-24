# Video Cutscene MVP Implementation Plan

## Overview

This document outlines a **minimal-risk MVP** approach to replace Unity prefab-based cutscenes with video-based cutscenes exported from After Effects. The goal is to achieve video functionality with the **fewest possible changes** to the existing system.

## Design Philosophy: Minimal Modification

### What We Keep Unchanged ✅
- ✅ Existing `cutsceneMapping` list in RoomManager
- ✅ Same trigger logic (key object detection)
- ✅ Same timing and flow in `SwitchRoomCoroutine()`
- ✅ Same `PlayCutscene()` method signature
- ✅ All existing room management logic
- ✅ Same user experience and timing
- ✅ MainScene3D architecture (no scene loading needed)

### What We Minimally Change 🔧
- 🔧 `CutsceneEntry.cutscenePrefab` → `CutsceneEntry.videoClip` (1 line)
- 🔧 `PlayCutscene()` method content (replace animation with video)
- 🔧 Add VideoPlayer component to RoomManager
- 🔧 Create simple video display canvas

## Current System Analysis

### Existing Flow (Preserved)
1. Player places key objects (Basketball_Prefab, Bed_Prefab, Frame_Prefab)
2. RoomManager detects key objects when transitioning rooms
3. `PlayCutscene()` displays content for fixed duration
4. Content is hidden after animation completes
5. Room transition continues normally

### Current Code Structure (Mostly Preserved)
```csharp
// This stays exactly the same
[System.Serializable]
public class CutsceneEntry
{
    public string itemName;             // ✅ Keep unchanged
    public GameObject cutscenePrefab;   // 🔧 Only this changes to VideoClip
}

public List<CutsceneEntry> cutsceneMapping; // ✅ Keep unchanged
```

## MVP Implementation Plan

### Phase 1: Video Asset Preparation (You Handle)

#### 1.1 After Effects Export Settings
- **Resolution**: 1920x1080 (matching Unity canvas)
- **Frame Rate**: 30fps
- **Format**: MP4 with H.264 codec
- **Duration**: 3-5 seconds (matching current system)
- **Audio**: Optional AAC 48kHz

#### 1.2 Unity Asset Structure
```
Assets/
├── Videos/
│   ├── Basketball_Cutscene.mp4
│   ├── Bed_Cutscene.mp4
│   └── Frame_Cutscene.mp4
└── (existing structure unchanged)
```

#### 1.3 Unity Import Settings
- **Transcode**: Yes
- **Codec**: H.264
- **Quality**: Medium (balance size/quality)

### Phase 2: Minimal Code Changes (I Handle)

#### 2.1 Update CutsceneEntry (1 Line Change)
```csharp
[System.Serializable]
public class CutsceneEntry
{
    public string itemName;             // Keep exactly the same
    public VideoClip videoClip;         // CHANGE: GameObject → VideoClip
}
```

#### 2.2 Add Video Components to RoomManager (3 Fields)
```csharp
public class RoomManager : MonoBehaviour
{
    // ... all existing fields stay identical ...
    
    [Header("Video Player - MVP")]
    public VideoPlayer videoPlayer;     // Drag VideoPlayer component here
    public RawImage videoDisplay;       // Drag RawImage for display
    public GameObject videoCanvas;      // Canvas to show/hide

    // ... rest of class completely unchanged ...
}
```

#### 2.3 Replace PlayCutscene Method Content (Keep Same Signature)
```csharp
private IEnumerator PlayCutscene(VideoClip videoClip)  // Changed parameter type only
{
    if (videoClip == null || videoPlayer == null)
    {
        // Fallback: maintain same timing as before
        yield return new WaitForSeconds(3f);
        yield break;
    }

    // Show video (same timing as old system)
    videoCanvas.SetActive(true);
    videoPlayer.clip = videoClip;
    videoPlayer.Play();
    
    // Wait for video duration (preserves exact same user experience)
    yield return new WaitForSeconds(videoPlayer.clip.length);
    
    // Hide video (same cleanup as old system)
    videoPlayer.Stop();
    videoCanvas.SetActive(false);
}
```

#### 2.4 Update Call Site in SwitchRoomCoroutine (Minimal Change)
```csharp
// In SwitchRoomCoroutine(), replace the cutscene selection:
string chosenKeyObject = PlayerPrefs.GetString("next_key_object", "");
if (!string.IsNullOrEmpty(chosenKeyObject) && hasStarted)
{
    VideoClip selectedVideo = null;  // CHANGE: GameObject → VideoClip
    foreach (var entry in cutsceneMapping)
    {
        if (entry.itemName == chosenKeyObject && entry.videoClip != null)  // CHANGE: cutscenePrefab → videoClip
        {
            selectedVideo = entry.videoClip;  // CHANGE: assignment
            Debug.Log($"[RoomManager] Playing video cutscene: {chosenKeyObject}");
            break;
        }
    }

    PlayerPrefs.DeleteKey("next_key_object");
    if (selectedVideo != null)
    {
        yield return PlayCutscene(selectedVideo);  // CHANGE: parameter type
    }
}
```

### Phase 3: Unity Setup (5 Minutes)

#### 3.1 Create Video Canvas Prefab (Clean Environment)
Instead of working in the isometric MainScene3D:

1. **Create Canvas Prefab**:
   - Right-click in Project window → Create → UI → Canvas
   - Name it: `VideoCanvas.prefab`
   - Double-click to open prefab mode (nice orthographic view)

2. **Structure the VideoCanvas prefab**:
```
VideoCanvas (Canvas)
├── VideoBackground (Image)
│   ├── Color: Black (0,0,0,255)
│   ├── Anchor: Stretch to fill screen
│   ├── Raycast Target: ✓ (blocks input during video)
│   └── Source Image: None (solid color)
└── VideoDisplay (RawImage)
    ├── Anchor: Center, stretch to fit
    ├── Aspect Ratio: Preserve
    └── Texture: (will be assigned from RenderTexture)
```

3. **Configure Canvas Properties**:
   - Render Mode: Screen Space - Overlay
   - Sort Order: 100 (highest priority)
   - Pixel Perfect: ✓

#### 3.2 Setup VideoPlayer Component
1. **Add VideoPlayer to RoomManager GameObject**:
   - Select RoomManager in MainScene3D
   - Add Component → Video → Video Player

2. **Configure VideoPlayer properties**:
   ```
   Source: Video Clip
   Render Mode: Render Texture
   Target Texture: (Create new RenderTexture - see below)
   Play On Awake: ✗
   Is Looping: ✗
   Wait For First Frame: ✓
   ```

3. **Create RenderTexture asset**:
   - Right-click in Project → Create → Render Texture
   - Name: `VideoRenderTexture`
   - Size: 1920 x 1080
   - Assign to VideoPlayer's Target Texture field
   - Assign same texture to VideoDisplay's Texture field (in prefab)

#### 3.3 Canvas Hierarchy and Priorities
```
VideoCanvas (Prefab):    Sort Order = 100  (highest - blocks everything)
Main Canvas (Scene):     Sort Order = 50   (normal game UI)
LoadCanvas (Scene):      Sort Order = 75   (loading screens)
```

#### 3.4 Wire Inspector References
**In RoomManager inspector**:
- Drag VideoPlayer component → `videoPlayer` field
- Drag VideoDisplay RawImage (from prefab) → `videoDisplay` field
- Drag VideoCanvas prefab asset → `videoCanvas` field
- Replace old cutscene prefab references with video clip references

**Runtime Behavior**:
- VideoCanvas prefab automatically instantiates when referenced
- Acts exactly like a scene object
- Clean separation from main scene complexity

### Phase 4: Migration Process (Low Risk)

#### 4.1 Backup Everything
```bash
# Create backup scene
MainScene3D → Duplicate → "MainScene3D_Backup"

# Note current cutsceneMapping entries:
Basketball_Prefab → BasketballCutScene.prefab
(Record others for reference)
```

#### 4.2 Test Current System
1. Verify basketball cutscene works with old system
2. Note exact timing and behavior
3. Take screenshots for comparison

#### 4.3 Implement Changes (Order Matters)
1. **First**: Create video assets and import to Unity
2. **Second**: Create VideoCanvas and VideoPlayer setup
3. **Third**: Modify code (change 4-5 lines)
4. **Fourth**: Update inspector references
5. **Fifth**: Test immediately

#### 4.4 Validation Testing
```
✅ Basketball placement triggers video instead of animation
✅ Video duration matches previous experience (~3 seconds)
✅ Room transition continues normally after video
✅ No errors in console
✅ Same user experience overall
```

## Technical Implementation Details

### VideoPlayer Configuration
```csharp
// Applied automatically when component added:
videoPlayer.source = VideoSource.VideoClip;
videoPlayer.renderMode = VideoRenderMode.RenderTexture;
videoPlayer.isLooping = false;
videoPlayer.playOnAwake = false;
videoPlayer.waitForFirstFrame = true;
```

### Error Handling (Built-in Fallback)
```csharp
private IEnumerator PlayCutscene(VideoClip videoClip)
{
    if (videoClip == null || videoPlayer == null)
    {
        Debug.LogWarning("[RoomManager] Video or player missing, using fallback timing");
        yield return new WaitForSeconds(3f);  // Same as old system
        yield break;
    }
    
    // Normal video playback...
}
```

### Canvas Input Blocking
```
VideoBackground Image:
- Raycast Target: ✓
- Color: Black with full alpha
- Covers entire screen
→ Automatically blocks all input during video
```

## Risk Assessment: Extremely Low

### What Could Go Wrong? (Minimal)
- **Video doesn't load**: Falls back to 3-second wait (same timing)
- **VideoPlayer missing**: Falls back to 3-second wait
- **Inspector references broken**: Easy to re-wire, no code impact

### What Can't Go Wrong? (Everything Else)
- ✅ Room management logic unchanged
- ✅ Key object detection unchanged  
- ✅ Transition timing unchanged
- ✅ UI systems unchanged
- ✅ Camera systems unchanged
- ✅ Game progression unchanged

### Rollback Strategy (Instant)
```csharp
// To rollback: change one line back
public GameObject cutscenePrefab;  // Instead of VideoClip videoClip
// Re-assign prefab references in inspector
// Everything else stays identical
```

## Timeline: Same Day Implementation

### Morning (30 minutes - You)
- Export basketball video from After Effects
- Import to Unity with correct settings

### Afternoon (45 minutes - Me + You)
- **15 min**: Create VideoCanvas and VideoPlayer setup
- **15 min**: Modify 4-5 lines of code
- **15 min**: Wire inspector references and test

### Evening (15 minutes - Validation)
- Test basketball cutscene
- Verify identical user experience
- Ready for additional videos

**Total Time: 90 minutes maximum**

## Future Expansion (Optional)

This MVP foundation supports easy expansion:
- Add more videos: Just import and assign in inspector
- Improve video quality: Re-export from After Effects
- Add audio: Automatic with video files
- Add skip functionality: Simple input detection
- Multiple videos per object: Expand CutsceneEntry array

## Why This Approach Works

### ✅ **Zero Risk**
- Only changes what's absolutely necessary
- Preserves all existing logic and timing
- Built-in fallback for any issues

### ✅ **Immediate Results**  
- Working video cutscenes in under 2 hours
- Same user experience with better visuals
- No learning curve for existing systems

### ✅ **Future-Proof**
- Foundation for more sophisticated video systems
- Easy to enhance without breaking anything
- Maintains compatibility with existing content

### ✅ **Designer-Friendly**
- Same inspector workflow as before
- Just replace prefab references with video references
- No new concepts to learn

This MVP gets you professional video cutscenes **today** while keeping everything you've built intact! 