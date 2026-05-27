using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundData")]
public class SoundData : ScriptableObject
{
    public AudioClip clip;
    public float volume = 1f;
    public float pitch = 1f;

    [Header("Variación Aleatoria")]
    public float randomPitchRange = 0.1f; 
    public float randomVolumeRange = 0.05f; 

    public bool loop = false;
}

