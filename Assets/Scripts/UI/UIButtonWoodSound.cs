using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonWoodSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverClip;   // e.g., wood tap
    public AudioClip clickClip;   // e.g., wood hit
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip != null)
            audioSource.PlayOneShot(hoverClip, 0.7f);  // volumen opcional
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickClip != null)
            audioSource.PlayOneShot(clickClip, 0.9f);
    }
}
