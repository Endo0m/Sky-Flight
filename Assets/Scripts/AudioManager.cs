using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;
    }

    public List<Sound> sounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sound GetSound(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }
        return s;
    }

    public void PlaySound(string name, AudioSource source)
    {
        Sound s = GetSound(name);
        if (s != null && source != null)
        {
            source.clip = s.clip;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.loop = s.loop;
            source.Play();
        }
    }

    public void StopSound(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
        }
    }
}