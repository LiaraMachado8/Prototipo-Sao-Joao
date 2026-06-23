using UnityEngine;


public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField]
    private SoundLibrary sfxLibrary;

    [SerializeField]
    private AudioSource sfx2DSource;

    private bool SoundPaused = false;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PauseSounds()
    {
        if (sfx2DSource != null && sfx2DSource.isPlaying)
            sfx2DSource.Pause();
    }

    public void ResumeSounds()
    {
        if (sfx2DSource != null && !sfx2DSource.isPlaying)
            sfx2DSource.UnPause();
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);
        }
    }

    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName));
    }

    public void ToggleSound()
    {
        if (SoundPaused)
        {
            PauseSounds();
            SoundPaused = false;
        }
        else
        {
            PauseSounds();
            SoundPaused = true;
        }
    }

}

