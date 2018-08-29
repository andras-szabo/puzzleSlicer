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
0.6:
	o saving game also when leaving the app via the back button
	o code cleanup
	o adjusting masks so that they fit better
	o adding in-game help overlay linked to a "?" button
	o build size improvements

0.5:
	o basic back button management
	o much better use of device space		
	o small ui tweaks (quit confirmation, version number on start menu)

0.4:
	o performance improvement for slicing
	o performance improvement for normal gameplay; 
	o some sound effects.

0.3:
	o outline matching pieces
	o "no edge" edges
	o smoothen the rough edges

0.2:
	o fix bug with keeping the preferred aspect ratio of _previous_ img
	o flash matching pieces
	o changed background colour for lighter one

	o auto save / load progress
	o some cleanup

0.1:
	o initial release
