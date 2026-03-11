using UnityEngine;
using UnityEngine.UI;

public class NoteSymbol : MonoBehaviour
{
    public string noteType;
    public float speed = 250f;

    public NoteVisualManager visualManager;

    private RectTransform rectTransform;
    private Image image;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        ApplySpriteByType(); // ahora sí se ejecuta después de que noteType fue asignado
    }

    private void Update()
    {
        rectTransform.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);

        if (rectTransform.anchoredPosition.y < -445f)
        {
            Destroy(gameObject);
        }
    }

    private void ApplySpriteByType()
    {
        if (image == null || visualManager == null)
        {
            Debug.LogWarning("VisualManager o Image no asignado");
            return;
        }

        Sprite sprite = visualManager.GetSpriteByType(noteType);
        //Debug.Log("[NOTE DEBUG] Buscando sprite para tipo: '" + noteType + "'");

        if (sprite != null)
        {
            image.sprite = sprite;
        }
        else
        {
            Debug.LogWarning("No se encontró sprite para: " + noteType);
        }
    }
}


