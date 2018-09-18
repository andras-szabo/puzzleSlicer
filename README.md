# puzzleSlicer
Create a jigsaw puzzle from any photo on your mobile device.

A small app built in Unity 2018.2.0f2, so far only tested on Android.

This is how it works:
- given an initial image, it cuts it up into small, overlapping rectangles ("pieces");
- it then applies masks to these pieces, to "cut" connecting shapes into the pieces;
- it then copies the masked pieces, with some additional padding, into a new texture;
- and uses this new texture, saved and exported into a PNG file, to show pieces on the 
  board using RawImage components, with appropriate UVs. This means that all pieces drawn
  on the board use the same material and same texture; only their UVs differ.
  
A barebones shader is used to draw pieces, which is very similar to the Unity default
UI shader, but without clipping (for performance reasons). And another simple flat colour
shader is used to draw the outline of matching connecting pieces.

Changelog:
0.10:
- fix a bug with sometimes missing piece outline
- adjust drag-and-drop sensitivity
- setting menu; change original image intensity and bacgkround colour

0.9:
- drag-and-drop from the queue to the play field
- queue now has a scroll bar
- better help overlay
- under-the-hood refactoring

0.8:
- fixing a bug with disappearing background/outlines
- fixing a bug with viewport positioning
- small tweak to mask textures

0.7:
- mask creation performance improvements
- zoom on mouse wheel when in editor
- smooth lerping camera when centering it
- basic "you've won!" indicator and behaviour

0.6:
- saving game also when leaving the app via the back button
- code cleanup
- adjusting masks so that they fit better
- adding in-game help overlay linked to a "?" button
- build size improvements

0.5:
- basic back button management
- much better use of device space
- small ui tweaks (quit confirmation, version number on start menu)

0.4:
- performance improvement for slicing
- performance improvement for normal gameplay;
- some sound effects.

0.3:
- outline matching pieces
- "no edge" edges
- smoothen the rough edges

0.2:
- fix bug with keeping the preferred aspect ratio of _previous_ img
- flash matching pieces
- changed background colour for lighter one
- auto save / load progress
- some cleanup

0.1:
- initial release
