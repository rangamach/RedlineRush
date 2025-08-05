using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundSO", menuName = "Scriptable Objects/SoundSO")]
public class SoundSO : ScriptableObject
{
    public List<Sounds> Sounds;
}
[System.Serializable]
public struct Sounds
{
    public SoundTypes type;
    public AudioClip clip;
}
public enum SoundTypes
{
    Background,
    ButtonClick,
    CarStart,
    CarEngine,
    CarCrash,
    CarExplosion,
    Cheer,
}
