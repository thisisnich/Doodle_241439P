# Brush Types Feature Suggestion

## Suggested Brush Types (Similar to MS Paint)

1. **Paintbrush** (Current - Soft, rounded edges)
   - Smooth, painterly effect
   - Rounded caps and joins
   - Semi-transparent edges

2. **Crayon** (Textured, rough edges)
   - Rough, grainy texture
   - Irregular edges
   - More opaque, solid color

3. **Marker** (Smooth, solid)
   - Clean, solid lines
   - Sharp edges
   - Fully opaque

4. **Pencil** (Sharp, thin lines)
   - Hard edges
   - No anti-aliasing (optional)
   - Sharp, precise lines

5. **Airbrush** (Soft, gradient edges)
   - Very soft, feathered edges
   - Gradient opacity from center to edge
   - Smooth, spray-like effect

6. **Pure Black** (Solid, no transparency)
   - Completely opaque
   - Sharp edges
   - Maximum opacity

## Implementation Approach

### Option 1: Simple Dropdown/ComboBox
- Add a ComboBox next to brush size slider
- Options: Paintbrush, Crayon, Marker, Pencil, Airbrush, Pure Black
- Changes brush rendering style immediately

### Option 2: Icon Buttons (Like MS Paint)
- Add small icon buttons in a row
- Each button represents a brush type
- Visual representation of each brush type

### Option 3: Menu/Submenu
- Add brush type selection in a menu
- Or as a dropdown from the brush tool button

## Technical Implementation

Each brush type would modify:
- `LineCap` style (Round, Square, Flat)
- `LineJoin` style (Round, Miter, Bevel)
- Opacity/Alpha blending
- Edge smoothing
- Texture/noise (for crayon effect)

## Recommended: Option 1 (ComboBox)
- Simple to implement
- Easy to use
- Doesn't clutter the UI
- Can be placed next to brush size slider
