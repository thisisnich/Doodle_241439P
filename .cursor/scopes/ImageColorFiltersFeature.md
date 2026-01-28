# Image Color Filters Feature

## Purpose & User Problem

Users want to apply color filters to images in the drawing application to:
- Enhance or modify the appearance of loaded images
- Create artistic effects (grayscale, sepia, etc.)
- Adjust color balance and RGB values
- Preview filter effects before applying them

Currently, users can load and place images, but there's no way to apply color transformations or filters to them.

## Success Criteria

1. **Clean Dialog Interface**
   - Professional, easy-to-use dialog box
   - Clear layout with filter selection and preview
   - Intuitive controls for preset and custom RGB filters

2. **Filter Presets**
   - Common preset filters available (e.g., Grayscale, Sepia, Invert, Brightness/Contrast)
   - Easy selection via buttons or dropdown
   - Each preset has a descriptive name

3. **Custom RGB Filter**
   - Ability to adjust Red, Green, Blue channels independently
   - Sliders or numeric inputs for RGB values (0-255 or percentage-based)
   - Real-time preview of changes

4. **Live Preview**
   - Preview pane showing filtered image
   - Updates immediately when filter selection or RGB values change
   - Side-by-side or toggle between original and filtered view

5. **Apply/Cancel Functionality**
   - Apply button to confirm and apply filter to image
   - Cancel button to discard changes
   - Filter applies to selected image (if in load/emoji mode) or entire canvas

## Scope & Constraints

### In Scope
- Dialog form with filter selection UI
- Preset filters: Grayscale, Sepia, Invert, Brightness, Contrast, Saturation
- Custom RGB filter with individual channel controls
- Live preview of filtered image
- Apply filter to:
  - Selected placed image (if one is selected in load/emoji mode)
  - Entire canvas bitmap (if no image is selected)
- Integration with existing image loading/placement system

### Out of Scope (For Now)
- Multiple filter stacking/combinations
- Filter presets saving/loading
- Histogram or advanced color correction tools
- Filter effects on brush strokes (only applies to images/canvas)
- Animation or transition effects

## Technical Considerations

### Filter Implementation
- Use `ColorMatrix` for efficient pixel transformations (already used in codebase for opacity)
- Preset filters can be predefined ColorMatrix configurations
- Custom RGB filter: adjust matrix values for R, G, B channels
- Performance: filter preview should update smoothly (may need optimization for large images)

### Dialog Design
- Windows Forms dialog (`Form` with `DialogResult`)
- Layout options:
  - Left: Filter selection (presets + RGB controls)
  - Right: Preview pane (PictureBox showing filtered image)
  - Bottom: Apply/Cancel buttons
- Size: ~800x600 pixels (adjustable)
- Modal dialog (blocks main form interaction)

### Integration Points
- Access point: Menu item, button, or context menu
- When to show: 
  - Option 1: New menu item "Image Filters" in menu strip
  - Option 2: Button next to Load/Save buttons
  - Option 3: Right-click context menu on placed images
- Filter application:
  - If `selectedImage != null` (in load/emoji mode): Apply to that image
  - Otherwise: Apply to entire canvas (`bm` bitmap)

### Filter Types to Implement

1. **Grayscale**
   - Convert RGB to grayscale using standard formula: `0.299*R + 0.587*G + 0.114*B`

2. **Sepia**
   - Warm brown tone effect
   - Matrix: `[0.393, 0.769, 0.189, 0, 0]` for R, `[0.349, 0.686, 0.168, 0, 0]` for G, `[0.272, 0.534, 0.131, 0, 0]` for B

3. **Invert**
   - Invert all color channels: `255 - value`

4. **Brightness**
   - Adjust overall brightness (add/subtract constant to RGB)

5. **Contrast**
   - Adjust contrast (multiply RGB values around midpoint)

6. **Saturation**
   - Adjust color saturation (desaturate or enhance colors)

7. **Custom RGB**
   - Independent sliders for Red, Green, Blue channels
   - Range: 0-200% (0% = remove channel, 100% = normal, 200% = double intensity)

## UI/UX Design

### Dialog Layout
```
┌─────────────────────────────────────────────────────┐
│  Image Color Filters                                │
├──────────────────┬──────────────────────────────────┤
│                  │                                  │
│  Filter Presets: │      Preview Pane               │
│  [Grayscale]     │      (Original/Filtered)        │
│  [Sepia]         │                                  │
│  [Invert]        │                                  │
│  [Brightness]    │                                  │
│  [Contrast]      │                                  │
│  [Saturation]    │                                  │
│                  │                                  │
│  Custom RGB:     │                                  │
│  Red:   [====]   │                                  │
│  Green: [====]   │                                  │
│  Blue:  [====]   │                                  │
│                  │                                  │
│  [Reset]         │                                  │
├──────────────────┴──────────────────────────────────┤
│              [Cancel]        [Apply]                │
└─────────────────────────────────────────────────────┘
```

### Controls
- **Preset Buttons**: Radio buttons or toggle buttons (only one active at a time)
- **RGB Sliders**: TrackBar controls (0-200, default 100)
- **Preview**: PictureBox with SizeMode.StretchImage
- **Reset Button**: Clear all filters, return to original
- **Apply/Cancel**: Standard dialog buttons

## Implementation Steps

1. Create `ImageFilterDialog` form class
2. Implement filter algorithms using ColorMatrix
3. Create UI with preset buttons, RGB sliders, and preview
4. Implement real-time preview update logic
5. Add menu item or button to open dialog
6. Integrate filter application to selected image or canvas
7. Test with various image sizes and filter combinations

## Questions to Clarify

1. **Access Point**: Where should users access the filter dialog?
   - Menu item in menu strip?
   - Button in toolbar?
   - Context menu on images?

2. **Default Behavior**: When no image is selected, should filter apply to:
   - Entire canvas?
   - Show message that an image must be selected?
   - Both options available?

3. **Preview Size**: Should preview be:
   - Fixed size (e.g., 400x300)?
   - Scaled to fit dialog?
   - Show actual size (may be too large)?

4. **Filter Persistence**: Should applied filters be:
   - Permanent (cannot undo except via general undo)?
   - Saved in undo stack?
