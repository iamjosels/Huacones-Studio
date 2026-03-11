using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject backToMenuButton;
    public AudioSource audioSource;

    [Header("Cutscene de transición")]
    public CutsceneData tutorialToRitmoCutscene;

    [Header("Seguridad")]
    [Tooltip("Si el audio no puede reproducirse (autoplay bloqueado en WebGL), esperar este tiempo y continuar.")]
    public float fallbackSeconds = 6f;

    bool hasFinished;

    void Start()
    {
        backToMenuButton.SetActive(false);

        // Intenta reproducir el audio si hay clip
        if (audioSource != null && audioSource.clip != null)
        {
            // En WebGL el autoplay puede fallar; no pasa nada, tenemos fallback.
            audioSource.Play();
            StartCoroutine(WaitAndGo());
        }
        else
        {
            // Sin audio: usa solo fallback
            StartCoroutine(WaitFallbackAndGo());
        }
    }

    IEnumerator WaitAndGo()
    {
        float start = Time.realtimeSinceStartup;
        float maxWait = (audioSource.clip != null ? audioSource.clip.length + 0.5f : 0f);

        while (!hasFinished)
        {
            // 1) Si el audio terminó con seguridad (evita el frame 0)
            if (!audioSource.isPlaying && audioSource.time > 0.5f)
                break;

            // 2) Fallback si pasó demasiado tiempo (autoplay bloqueado en WebGL)
            if (Time.realtimeSinceStartup - start >= Mathf.Max(fallbackSeconds, maxWait))
                break;

            yield return null;
        }

        GoToCutscene();
    }

    IEnumerator WaitFallbackAndGo()
    {
        yield return new WaitForSecondsRealtime(fallbackSeconds);
        GoToCutscene();
    }

    void GoToCutscene()
    {
        if (hasFinished) return;
        hasFinished = true;

        CutsceneLoader.cutsceneToLoad = tutorialToRitmoCutscene;
        SceneTransitionManager.EnsureInstance().LoadSceneSafe("CutsceneViewer");
    }

    public void OnBackToMenuPressed()
    {
        SceneTransitionManager.EnsureInstance().LoadSceneSafe("MainMenu");
    }
}
