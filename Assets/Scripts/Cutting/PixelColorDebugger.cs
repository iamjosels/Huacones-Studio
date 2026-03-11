using UnityEngine;
using UnityEngine.UI;

public class PixelColorDebugger : MonoBehaviour
{
    public RawImage targetImage;
    public Texture2D targetTexture;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Click derecho
        {
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetImage.rectTransform,
                Input.mousePosition,
                null,
                out localPos))
            {
                Vector2 uv = GetUVFromLocalPoint(localPos);

                if (IsInsideTexture(uv))
                {
                    Color pixel = GetPixelFromUV(uv);
                    Debug.Log($"Pixel color: {pixel} | RGB: ({(int)(pixel.r * 255)}, {(int)(pixel.g * 255)}, {(int)(pixel.b * 255)})");
                }
                else
                {
                    Debug.Log("Click fuera de la textura.");
                }
            }
        }
    }

    Vector2 GetUVFromLocalPoint(Vector2 localPoint)
    {
        Rect rect = targetImage.rectTransform.rect;
        return new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height
        );
    }

    bool IsInsideTexture(Vector2 uv)
    {
        return uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1;
    }

    Color GetPixelFromUV(Vector2 uv)
    {
        int x = Mathf.FloorToInt(uv.x * targetTexture.width);
        int y = Mathf.FloorToInt(uv.y * targetTexture.height);
        return targetTexture.GetPixel(x, y);
    }
}
