# Usink

**[Download via Asset Store](http://u3d.as/DrG)**

Usink is a Unity3D editor extension plugin to flood your keyboard (and mouse) with useful opinionated keyboard shortcuts. You can use it for saving your time while building scenes in Unity.

You can see the whole settings in [Config.cs](Assets/Editor/Plugins/Usink/Config.cs). Don't like my settings? Just fork the project and change that as much as you like!

## Right-Click Context

Perhaps this is the most amazing thing you absolutely need. Right click a scene to open context menu where you can select objects even it's behind another object. [Demonstration](https://twitter.com/willnode/status/942026444221251584).

## Selection Query

Hit <kbd>Space</kbd> at SceneView to open query dialog. Use this feature to quickly select matched query similar to search bar in hierarchy. Only this time, it's more powerful with query signs:

| Query Begin With | Means |
|------------------|-------|
| ` ` | Select Objects by name in Scene |
| `+` | Select Objects Additively (Already Selected objects is unaffected) |
| `-` | Select Objects Subtractively (Inverse of additive) |
| `/` | Select Objects if it a children of already selected objects |
| `>` | Find Objects by component name (Similar with `t:` but can do partial search) |
| `#` | Find Assets to get selected |
| `@` | Find and open a Scene inside project (Shift to add additively) | 
| `!` | Launch an EditorWindow (Note that it lists ALL EditorWindow even it's hidden for your goodness sake) |

> PS: Query shortcut are inspired from Blender while signs are from VS Code :)

## Scene View Navigation

All scene navigation uses Numpad. Don't forget to turn on your <kbd>NumLk</kbd>:

+ <kbd>5</kbd> Pitch-rotate the camera downward
+ <kbd>8</kbd> Pitch-rotate the camera upward
+ <kbd>4</kbd> Yaw-rotate the camera leftward
+ <kbd>6</kbd> Yaw-rotate the camera rightward
+ <kbd>7</kbd> Roll-rotate the camera clockwise
+ <kbd>9</kbd> Roll-rotate the camera counter-clockwise
+ <kbd>3</kbd> Zoom in the camera
+ <kbd>1</kbd> Zoom out the camera
+ <kbd>2</kbd> Change perspective/orthographic
+ <kbd>0</kbd> Align the camera to main camera
+ <kbd>.</kbd> Align the camera to UI canvas

## Selection

Keyboard shortcut to handle selections:

+ <kbd>P</kbd> Select or Make parent
+ <kbd>A</kbd> Select none
+ <kbd>L</kbd> Select based on similarities (eg. name/prefab/mesh)
+ <kbd>K</kbd> More select operation (eg. by parent/child/sibling)

## Operations

Classic operations made easy:

+ <kbd>S</kbd> Remove Component (will open dialog and also shows components that partially exist in selection)
+ <kbd>D</kbd> Add GameObject (similar as hierarchy dropdown)
+ <kbd>G</kbd> Set Object Gizmo (applies to active object only)
+ <kbd>H</kbd> Toggle Object Active Status
+ <kbd>J</kbd> Toggle Object Lock Status (HideFlags `NotEditable`)
+ <kbd>,</kbd> Open Layer Mask (right from SceneView)
+ <kbd>.</kbd> Open Layout Selection (right from SceneView)
+ <kbd>F2</kbd> Rename GameObject right inside SceneView
+ <kbd>F9</kbd> Clear Developer Console

## FWIW

Built-in unity shortcuts include: <kbd>Q</kbd> <kbd>W</kbd> <kbd>E</kbd> <kbd>R</kbd> <kbd>T</kbd> <kbd>Y</kbd> <kbd>Z</kbd> <kbd>X</kbd>  <kbd>F</kbd> <kbd>V</kbd> <kbd>2</kbd>.

This means these keys are still unused: <kbd>1</kbd> <kbd>3</kbd> <kbd>4</kbd> <kbd>5</kbd> <kbd>6</kbd> <kbd>7</kbd> <kbd>8</kbd> <kbd>9</kbd> <kbd>0</kbd> <kbd>-</kbd> <kbd>=</kbd> <kbd>~</kbd> <kbd>U</kbd> <kbd>I</kbd> <kbd>O</kbd> <kbd>&#92;</kbd> <kbd>;</kbd> <kbd>'</kbd> <kbd>C</kbd> <kbd>B</kbd> <kbd>N</kbd> <kbd>M</kbd> <kbd>/</kbd>

## Ideas and Contribute

Have an idea to fill these empty shortcuts? Feel free to use Issues tab to suggest one.

## Versions

Current version: `0.6.1` (Alpha)

This means things can change or break, and you can propose new ideas before being marked as `Final`.

Developed using Unity 5.6.0

## License

[MIT](LICENSE)

