# Shapes Feature Specification

## Purpose & User Problem
Users need the ability to draw geometric shapes (lines, squares, circles, and polygons) on the canvas. Currently, the application only supports freehand drawing with brush/pen tools, text, and image placement. Adding shape tools will enable users to create precise geometric drawings more efficiently than freehand drawing.

## Success Criteria
- Users can select from 4 shape tools: Line, Square, Circle, and N-gon
- Shapes are drawn using the current brush color and size settings
- For N-gon, users can specify the number of sides (e.g., 3 for triangle, 5 for pentagon, 8 for octagon)
- Shapes are drawn on mouse drag (click and drag to define size/position)
- Shapes integrate with existing undo functionality
- Shape tools follow the same UI pattern as existing tools (brush, eraser, text, load)

## Scope & Constraints

### In Scope
1. **Line Tool**
   - Draw straight lines from start point to end point
   - Use current brush color and width

2. **Square Tool**
   - Draw rectangles/squares (click and drag to define opposite corners)
   - Free-form rectangles by default; hold Shift for perfect squares
   - Use current brush color and width
   - Support both filled and outlined modes (toggle control)

3. **Circle Tool**
   - Draw circles/ellipses (click and drag to define bounding rectangle)
   - Free-form ellipses by default; hold Shift for perfect circles
   - Use current brush color and width
   - Support both filled and outlined modes (toggle control)

4. **N-gon Tool**
   - Draw regular polygons with user-specified number of sides
   - TrackBar (slider) control to select number of sides
   - Minimum: 3 sides (triangle)
   - Maximum: 20 sides
   - Default: 5 sides (pentagon)
   - Use current brush color and width
   - Support both filled and outlined modes (toggle control)

### UI/UX Considerations
- Shape tools will be placed in a new section under the existing tool buttons (brush, eraser, text, load)
- Separate buttons for each shape tool (Line, Square, Circle, N-gon)
- Shapes show preview while dragging, then commit to canvas on mouse up
- N-gon side selection: TrackBar (slider) control
- Toggle control for filled vs outlined mode (applies to all shapes except line)

### Technical Constraints
- Must work with existing undo system
- Must respect current brush color and size settings
- Must follow existing tool switching pattern (flagBrush, flagErase, etc.)
- Should integrate with existing mouse event handlers

## Out of Scope (for now)
- Shape editing after placement (resize, move, rotate)
- Shape styling options (dashed lines, etc.)
- Shape layers or grouping
- Copy/paste of shapes

## Decisions Made

1. **Shape rendering**: Both filled and outlined modes supported, with a toggle control
2. **N-gon sides**: Minimum 3, Maximum 20, Default 5
3. **UI placement**: New section under existing tool buttons with individual buttons for each shape
4. **N-gon input method**: TrackBar (slider) control
5. **Drawing behavior**: Preview while dragging, commit on mouse up
6. **Square/Circle**: Free-form by default, hold Shift for perfect squares/circles

## Technical Approach

### State Variables
- Add boolean flags: `flagLine`, `flagSquare`, `flagCircle`, `flagNgon`
- Add variable for N-gon sides: `int ngonSides = 5`
- Add variable for filled mode: `bool shapeFilled = false`
- Store temporary shape preview data during drag

### UI Components
- Add 4 PictureBox buttons for shape tools (Line, Square, Circle, N-gon) in new section
- Add TrackBar for N-gon sides (3-20, default 5) with label
- Add CheckBox or Toggle button for filled/outlined mode
- Update `SetToolBorder` to include shape tools

### Mouse Event Handlers
- `picBoxMain_MouseDown`: Save undo state, store start point, set drawing flag
- `picBoxMain_MouseMove`: Draw preview shape on canvas (using Paint event, not directly to bitmap)
- `picBoxMain_MouseUp`: Finalize shape by drawing to bitmap, clear preview

### Drawing Methods
- Use Graphics.DrawLine for line tool
- Use Graphics.DrawRectangle or FillRectangle for square tool (based on filled mode)
- Use Graphics.DrawEllipse or FillEllipse for circle tool (based on filled mode)
- Use Graphics.DrawPolygon or FillPolygon for N-gon tool (calculate polygon points)
- For N-gon: Calculate regular polygon points around center point
- Handle Shift key modifier for perfect squares/circles

### Integration
- Integrate with existing undo system (SaveUndoState before drawing)
- Respect current brush color and size settings
- Follow existing tool switching pattern
- Auto-stamp images when switching to shape tools (if in load mode)
