using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Collections.Generic;

public class DalgonaGameManager : MonoBehaviour
{
    [Header("Configuración")]
    public RawImage galletaImage;
    public Color colorFigura = new Color32(33, 60, 132, 255);
    public float tolerancia = 0.08f;

    [Tooltip("Texturas por dificultad (ronda 1 = índice 0, etc.)")]
    public List<Texture2D> figurasPorRonda;

    [Header("UI")]
    //public TextMeshProUGUI estadoTexto;

    public System.Action<bool> OnGameFinished;

    private bool juegoActivo = false;
    private int ronda = 1;
    private Texture2D texturaFigura;

    public void SetRound(int r)
    {
        ronda = Mathf.Clamp(r, 1, figurasPorRonda.Count);

        // Asignar figura según dificultad
        if (figurasPorRonda.Count >= ronda)
        {
            texturaFigura = figurasPorRonda[ronda - 1];
            galletaImage.texture = texturaFigura;
        }
        else
        {
            Debug.LogWarning("No hay suficientes texturas asignadas para la ronda.");
        }

        // Reducir tolerancia para mayor dificultad
        tolerancia = Mathf.Max(0.01f, 0.08f - (ronda - 1) * 0.02f);
    }

    void OnEnable()
    {
        //estadoTexto.text = "Recorta la figura sin salirte...";
        juegoActivo = true;
    }

    void Update()
    {
        if (!juegoActivo) return;

        // Entrada real del jugador
        if (Input.GetMouseButtonUp(0))
        {
            if (VerificarRecorte())
            {
                //estadoTexto.text = "¡Éxito!";
                juegoActivo = false;
                OnGameFinished?.Invoke(true);
            }
            else
            {
                //estadoTexto.text = "¡Fallaste!";
                juegoActivo = false;
                OnGameFinished?.Invoke(false);
            }
        }

#if UNITY_EDITOR
        // Entrada simulada para pruebas con Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("⏩ Simulación automática: Victoria en Dalgona");
            juegoActivo = false;
            OnGameFinished?.Invoke(true);
        }
#endif
    }

    bool VerificarRecorte()
    {
        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            galletaImage.rectTransform, Input.mousePosition, null, out localPos))
            return false;

        Rect rect = galletaImage.rectTransform.rect;
        Vector2 uv = new Vector2(
            (localPos.x - rect.x) / rect.width,
            (localPos.y - rect.y) / rect.height
        );

        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return false;

        int x = Mathf.FloorToInt(uv.x * texturaFigura.width);
        int y = Mathf.FloorToInt(uv.y * texturaFigura.height);
        Color pixel = texturaFigura.GetPixel(x, y);

        float diferencia = Vector4.Distance(pixel, colorFigura);
        Debug.Log($"Dif. color: {diferencia}");

        return diferencia <= tolerancia;
    }
}
