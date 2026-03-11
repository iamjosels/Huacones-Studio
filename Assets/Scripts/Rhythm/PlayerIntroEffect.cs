using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerIntroEffect : MonoBehaviour
{
    public Image characterImage;
    public float fadeDuration = 1.5f;
    public Vector3 startScale = new Vector3(0.2f, 0.2f, 1f);
    public Vector3 endScale = Vector3.one;

    void Start()
    {
        characterImage.canvasRenderer.SetAlpha(0f);
        transform.localScale = startScale;
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        // Escala progresiva
        float t = 0;
        while (t < fadeDuration)
        {
            float progress = t / fadeDuration;
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            characterImage.canvasRenderer.SetAlpha(progress);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
        characterImage.canvasRenderer.SetAlpha(1f);
    }
}
