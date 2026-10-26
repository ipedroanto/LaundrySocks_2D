using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Referencias de Audio")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    private void Awake()
    {
        // Patron Singleton con DontDestroyOnLoad para evitar que la música se corte
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Evita duplicar la música si volvemos a la escena principal
            return;
        }

        // Configuración inicial de la fuente de audio
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true; // Activa el loopeo automático de los 89s

            // Cargar el volumen guardado en preferencias (por defecto 100%)
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
            musicSource.volume = savedVolume;

            musicSource.Play();
        }
    }

    // Método público llamado por el Slider de la interfaz (UI)
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
}
