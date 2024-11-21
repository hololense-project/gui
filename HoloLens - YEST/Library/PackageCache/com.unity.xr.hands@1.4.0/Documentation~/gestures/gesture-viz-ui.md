---
uid: xrhands-understand-gesture-visualizer
---

# Understand the gesture debugger

<center><img src="../images/gestures/gesture-debugger-screenshot.png" alt="Gesture debugger"/><br/><i>The gesture debugger scene visualizing a gesture</i></center>

The gesture debugging scene shows two types of information:

* For every finger, it displays the current values of the finger shapes. This display includes the target value and tolerance for one, configured gesture. You can [edit the configured gesture](xref:xrhands-customize-gesture-visualizer#set-hand-shape) so that the values for gesture you are testing or debugging are shown by the finger graphs.
* An indicator showing whether specific gestures are currently recognized. You can [replace or add to](xref:xrhands-customize-gesture-visualizer#gestures) the list of recognized gestures by editing the scene.

The gesture visualizer is a Unity scene provided as part of the Gestures Sample that you can import from the XR Hands package. Refer to [Install the gesture debugger](xref:xrhands-install-gesture-visualizer) for instructions.

## Finger data display

The finger data display shows the current value for each aspect of a finger's shape. For the configured HandShape asset, the display also shows the target value and (upper & lower) tolerance range for each aspect. You can change the configured HandShape asset by editing the visualizer scene. Refer to [Set the HandShape](xref:xrhands-customize-gesture-visualizer#set-hand-shape) for more information.

![Finger shape display](../images/gestures/finger-data-display.png)<br/>*An example of the data displayed for a finger*

The following table describes each part of the data graph:

|     | Description |
| :-- | :---------- |
| A | The names of the shape values. | 
| B | The numeric scale, which is a normalized range between zero and one. | 
| C | The finger shape data: <br/><ul><li><b>White line</b>: The current value of the shape based on the incoming joint data from the hand tracking system.</li><li><b>Marker</b>: The target value of the configured HandShape asset.</li><li><b>Blue line</b>: The (upper & lower) tolerance range around the target value.</li></ul> | 
| D | The name of the finger. |

Refer to [Finger shapes](xref:xrhands-finger-shapes) for information about the shape values. Refer to [Hand shapes](xref:xrhands-hand-shapes) for information about creating and editing HandShape assets.


## Gesture detection                                                                                                                          

The gesture detection section of the gesture visualizer provides indicators that change color when one of the configured gestures is detected. You can change the configured gestures or add to the list by editing the visualizer scene. Refer to [Edit the gesture list](xref:xrhands-customize-gesture-visualizer#gestures) for more information.

<center><img src="../images/gestures/gesture-detector-ui.png" alt="The preconfigured sample gestures"/><br/><i>The preconfigured sample gestures provided by the XR Hands sample</i></center>

> [!TIP]
> To see the target values and tolerances for a gesture's finger shapes, you can [edit the scene](xref:xrhands-customize-gesture-visualizer#set-hand-shape) so that the finger data display references the same HandShape asset as the gesture you are interested in.   
                                                                                                                       