using UnityEngine;

public class ParallaxUI : MonoBehaviour
{
    [Header("Velocidad de parallax")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.1f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 offset = (mousePos - center) * parallaxFactor;

        transform.localPosition = initialPosition + new Vector3(offset.x, offset.y, 0);
    }
}
