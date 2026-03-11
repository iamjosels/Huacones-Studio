using UnityEngine;
using UnityEngine.UI;

public class CandyCuttingManager : MonoBehaviour
{
    public RawImage candyImage;
    public Texture2D candyTexture;
    [Header("Target color to follow (from border of figure)")]
    public Color targetColor = new Color32(33, 60, 132, 255);

    public float colorTolerance = 0.2f;
    public int maxMistakes = 10;

    private int currentMistakes = 0;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                candyImage.rectTransform,
                Input.mousePosition,
                null,
                out localPos
            );

            Vector2 uv = GetUVFromLocalPoint(localPos);

            if (IsInsideTexture(uv))
            {
                Color pixelColor = GetPixelFromUV(uv);

                if (!IsSimilar(pixelColor, targetColor, colorTolerance))
                {
                    currentMistakes++;
                    Debug.Log("Error! Total mistakes: " + currentMistakes);

                    if (currentMistakes >= maxMistakes)
                    {
                        Debug.Log("GAME OVER: Cookie broken.");
                    }
                }
                else
                {
                    Debug.Log("Good trace.");
                }
            }
        }
    }

    Vector2 GetUVFromLocalPoint(Vector2 localPos)
    {
        Rect rect = candyImage.rectTransform.rect;
        return new Vector2(
            Mathf.Clamp01((localPos.x - rect.x) / rect.width),
            Mathf.Clamp01((localPos.y - rect.y) / rect.height)
        );
    }

    bool IsInsideTexture(Vector2 uv)
    {
        return uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1;
    }

    Color GetPixelFromUV(Vector2 uv)
    {
        int x = Mathf.FloorToInt(uv.x * candyTexture.width);
        int y = Mathf.FloorToInt(uv.y * candyTexture.height);
        return candyTexture.GetPixel(x, y);
    }

    bool IsSimilar(Color a, Color b, float tolerance)
    {
        return Vector3.Distance(new Vector3(a.r, a.g, a.b), new Vector3(b.r, b.g, b.b)) < tolerance;
    }
}