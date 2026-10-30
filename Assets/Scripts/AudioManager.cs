using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Referencias de Audio")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    private float basePitch = 1.0f; // Pitch normal de reproducción

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
            return;
        }

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
            musicSource.volume = savedVolume;

            musicSource.Play();
        }
    }

    public void SetVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume);
            PlayerPrefs.Save();
        }
    }

    public float GetVolume()
    {
        return musicSource != null ? musicSource.volume : 1.0f;
    }

    // NUEVO: Modifica el pitch dinámicamente según el tiempo restante
    public void SetMusicPitch(float pitch)
    {
        if (musicSource != null)
        {
            musicSource.pitch = Mathf.Clamp(pitch, 1.0f, 1.35f); // Limita el acelerado a un máximo de 1.35x
        }
    }

    // NUEVO: Restablece el pitch a su velocidad normal al iniciar/reiniciar un nivel
    public void ResetMusicPitch()
    {
        if (musicSource != null)
        {
            musicSource.pitch = basePitch;
        }
    }
}