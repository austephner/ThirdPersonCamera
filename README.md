# Third Person Camera
A highly configurable feature-rich third person camera.

#### Features
- Highly Configurable
- Layer based reposition clipping
- Over-the-shoulder horizontal offset and realtime shoulder swapping
- Zoom, rotate horizontally/vertically, adjust height
- Separate horizontal and vertical rotation speeds and sensitivities
- Extensible
- Inlcluded prefab works out of the box

![Example](https://i.imgur.com/6IetXIF.gif)

# Getting Started
1. Add the package to Unity through the Package Manager or download the zipped version and extract it to your assets folder.
2. Drag and drop the `Samples/ThirdPersonCamera.prefab` into your scene. 
3. Assign a follow target if needed.
<br><br>
That's it!

# Configuration
Nearly every field on the `ThirdPersonCamera` component has a tooltip.

![Inspector](https://i.imgur.com/Rkm9KKe.png)

| Field                           | Description                                                                                                                                    | Example Value                 |
|---------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------|
| Draw Debug                      | Enables debug logging and additional debug lines.                                                                                              |                               |
| Draw Gizmos                     | Enables gizmos.                                                                                                                                |                               |
| Follow Target                   | The `Transform` the camera should physically follow.                                                                                           | Your character, any POI, etc. |
| Self Get Player Input           | Allows this camera to retrieve input data to update itself without external intervention.                                                      |                               |
| Invert Horizontal Camera Input  | All incoming horizontal input will be inverted.                                                                                                |                               |
| Invert Vertical Camera Input    | All incoming vertical input will be inverted.                                                                                                  |                               |
| Move Lerp Speed                 | How fast the camera lerps to the assigned follow target.                                                                                       | 10                            |
| Zoom Lerp Speed                 | How fast the camera's zoom lerps in and out.                                                                                                   | 10                            |
| Min Zoom In                     | How close the camera can zoom into its origin.                                                                                                 | 1                             |
| Max Zoom Out                    | How far the camera can zoom out from its origin.                                                                                               | 30                            |
| Camera                          | The actual camera component.                                                                                                                   |                               |
| Height Transform                | The transform which controls the height aspect of the camera. Must be a child of the main transform.                                           |                               |
| Horizontal Offset Transform     | The transform which controls the horizontal offset, the "over the shoulder" aspect. Must be a child of the height transform.                   |                               |
| X Axis                          | The transform which controls X axis rotation. Must be a child of the horizontal offset transform.                                              |                               |
| Camera Transform                | The transform which has the `Camera` component on it. Must be a child of the X Axis transform.                                                 |                               |
| Look Up Max Angle               | The "maximum" look up angle in degrees. This is usually a negative number since a negative angle is required to rotate the camera upwards.     | -60                           |
| Look Down Min Angle             | The "minimum" look down angle in degrees. This is usually a positive number since a positive angle is required to rotate the camera downwards. | 90                            |
| Horizontal Rotation Lerp Speed  | How fast the horizontal rotation lerps.                                                                                                        | 1                             |
| Horizontal Rotation Sensitivity | The horizontal rotation sensitivity.                                                                                                           | 1                             |
| Vertical Rotation Lerp Speed    | How fast the vertical rotation lerps.                                                                                                          | 1                             |
| Vertical Rotation Sensitivity   | The vertical rotation sensitivity.                                                                                                             | 1                             |
| Camera Clipping Layermask       | All layers that the camera can be clipped with.                                                                                                | Default                       |
| Forward Camera Clip Radius      | The forward raycast spherical radius for detecting camera clipping.                                                                            | 0.1                           |
| Horizontal Camera Clip Radius   | The horizontal raycast spherical radius for detected camera clipping.                                                                          | 0.1                           |
| Camera Clip Point Offset        | How far away from the raycasted clip point the camera will be placed on the hit point's normal.                                                | 0.25                          |
| Camera Clipping Lerp Speed      | How fast the camera lerps to the clip point offset. The higher the value, the less likely clipped geometry will be visible.                    | 100                           |