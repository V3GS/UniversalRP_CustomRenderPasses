using System;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Custom/FullScreen Volume Component")]
public class FullScreenVolumeComponent : VolumeComponent, IPostProcessComponent
{
    public enum EffectType { Gameboy, Grayscale, InvertColors };

    public BoolParameter volumeActive = new BoolParameter(true);
    public EnumParameter<EffectType> effect = new EnumParameter<EffectType>(EffectType.Gameboy);

    public bool IsActive() => volumeActive.value;
}
