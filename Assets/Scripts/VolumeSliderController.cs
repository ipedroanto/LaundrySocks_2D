
// ====================================================================================================================================================================
// Autor: Pedro Antonio Ramos
// Email: ing.ramospedro@gmail.com
// Propósito del script: Sincronizar el componente Slider de la interfaz gráfica con el control de volumen principal del AudioManager o los AudioMixers del proyecto.
// Fecha: 09/11/2026
// ====================================================================================================================================================================
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