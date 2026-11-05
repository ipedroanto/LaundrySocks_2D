using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Referencias de Audio")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Header("Efectos de Sonido (SFX)")]
    public AudioClip sfxCorrect;   // Asignar Correcto.wav
    public AudioClip sfxIncorrect; // Asignar Equivocado.wav

    [Header("SFX Especiales")]
    public AudioClip sfxWantedBonus; // Asignar sfxWantedBonus.mp3 de recompensa/fanfarria

    private float basePitch = 1.0f;

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

    public void SetMusicPitch(float pitch)
    {
        if (musicSource != null)
        {
            musicSource.pitch = Mathf.Clamp(pitch, 1.0f, 1.35f);
        }
    }

    public void ResetMusicPitch()
    {
        if (musicSource != null)
        {
            musicSource.pitch = basePitch;
        }
    }

    // NUEVO: Reproduce el efecto de sonido de acierto
    public void PlayCorrectSFX()
    {
        if (musicSource != null && sfxCorrect != null)
        {
            musicSource.PlayOneShot(sfxCorrect);
        }
    }

    // NUEVO: Reproduce el efecto de sonido de error
    public void PlayIncorrectSFX()
    {
        if (musicSource != null && sfxIncorrect != null)
        {
            musicSource.PlayOneShot(sfxIncorrect);
        }
    }

    public void PlayWantedBonusSFX()
    {
        if (musicSource != null && sfxWantedBonus != null)
        {
            musicSource.PlayOneShot(sfxWantedBonus);
        }
        else
        {
            PlayCorrectSFX(); // Respaldo si no hay audio asignado
        }
    }
}