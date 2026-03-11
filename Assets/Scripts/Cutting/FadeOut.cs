using UnityEngine;

public class FadeOut : MonoBehaviour
{
    public float duration = 0.5f;
    private CanvasGroup canvasGroup;
    private float timer;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        timer = duration;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0)
        {
            canvasGroup.alpha = timer / duration;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}