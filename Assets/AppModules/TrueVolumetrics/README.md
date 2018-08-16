# TrueVolumetrics

## How to set up volumetric objects

1. Import the asset or files. The TrueVolumetrics folder can be moved around to
wherever you want it to be in your Assets folder. It depends on the Leap Motion
Core Assets for the hand assets to function properly.

2. Enable the depth buffer on the Main Camera(s) in your scene. Core provides
an EnableDepthBuffer script for this; you can just add it to your Main Camera.

3. Add any objects in your scene with attached OutputVolumeInfo materials. 
TrueVolumetric effects are only supported for _closed meshes_. For hands, you
should use OutputVolumeInfo_LeftHand or -_RightHand as appropriate, which uses
a slightly modified hand shader that supports late-latching for slightly
reduced latency.

4. Add the True Volumetric Renderer component do your Main Camera(s). Make sure
the Blend Material is set to the provided VolumetricBlend material.

5. Add **all** objects that are to be volumetric to the "To Draw" list of the
TrueVolumetricRenderer script. These objects are not scanned for automatically
and must be explicitly declared to the TrueVolumetricRenderer.

## How it works

The TrueVolumetricRenderer script attaches a Command Buffer to the rendering
camera which it then uses to render depth information from each object to a
separate buffer and then blend the resulting depth information with the rest of
the rendered scene that depth information. Depth information from each object
is compared to the scene's depth buffer so that the closest depth is always
chosen, which allows scene geometry to occlude the volumetric effects. (This is
why the camera's depth buffer must be enabled.)

To render depth information for a given object, the command buffer scans through
the To Draw list and draws each object with its attached OutputVolumeInfo.
Front-facing triangles write positive depth, while back-facing triangles write
negative depth. Because the shader additively combines pixel information, the
result of all front-facing triangle depths minus all back-facing triangle depths
for a given pixel results in the total depth of "fog" through that pixel.

Alternatively, if the scene geometry is in front of any fog, the pixel will
simply get that depth value, flattening any fog depth information that is behind
scene geometry and perceptually resulting in the scene geometry correctly
occluding fog that it blocks.

Applying black fog to a tracked hand is a great way to provide occlusion for AR
experiences, as hand edges blend into any holograms and the occlusion is
accordingly relatively forgiving.
