using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    private Slider volumeSlider;

    private void Awake()
    {
        volumeSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (volumeSlider != null)
        {
            // Sincronizar la posición del slider con el volumen guardado en el AudioManager
            if (AudioManager.Instance != null)
            {
                volumeSlider.value = AudioManager.Instance.GetVolume();
            }

            // Escuchar el evento de cambio de valor del slider
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(value);
        }
    }
}