using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    public Slider progressSlider;
    public float progressValue = 0f;
    public float maxProgress = 100f;

    public float perfectIncrement = 10f;
    public float goodIncrement = 5f;
    public float failPenalty = 7f;

    void Start()
    {
        progressSlider.maxValue = maxProgress;
        progressSlider.value = progressValue;
    }

    public void AddProgress(string result)
    {
        if (result == "Perfect")
            progressValue += perfectIncrement;
        else if (result == "Good")
            progressValue += goodIncrement;
        else if (result == "Fail")
            progressValue -= failPenalty;

        progressValue = Mathf.Clamp(progressValue, 0f, maxProgress);
        progressSlider.value = progressValue;

        if (progressValue >= maxProgress)
        {
            RitualWin();
        }

        // NUEVO: Reinicio si llega a 0
        if (progressValue <= 0f)
        {
            ResetProgress();
        }
    }

    void RitualWin()
    {
        Debug.Log("✨ ¡Ritual completado exitosamente!");
        // Aquí puedes cargar la siguiente escena o mostrar una animación
    }

    public void ResetProgress()
    {
        progressValue = 0f;
        progressSlider.value = progressValue;
        Debug.Log("💀 Ritual fallido. Se reinicia el progreso.");
        // Aquí podrías añadir una animación de fallo o reiniciar el nivel completo si quieres
    }

}
