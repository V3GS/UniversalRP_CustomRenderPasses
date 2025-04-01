using System;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Custom/Simple Blur Volume Component")]
public class BlurVolumeComponent : VolumeComponent
{
    public BoolParameter grayscaleEffect = new BoolParameter(true);
    public ClampedFloatParameter horizontalBlur =new ClampedFloatParameter(0.015f, 0, 0.015f);
    public ClampedFloatParameter verticalBlur = new ClampedFloatParameter(0.015f, 0, 0.015f);
}
