using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DalgonaGameManager : MonoBehaviour
{
    [Header("Configuracion")]
    public RawImage galletaImage;
    public Color colorFigura = new Color32(33, 60, 132, 255);
    public float tolerancia = 0.08f;

    [Tooltip("Texturas por dificultad (ronda 1 = indice 0, etc.)")]
    public List<Texture2D> figurasPorRonda;

    [Header("UI")]
    public bool useStatusText = false;
    public TextMeshProUGUI statusText;
    public bool autoFindStatusText = true;

    [Header("Balance")]
    public int baseAttempts = 3;
    public int minAttempts = 1;
    public float toleranceReductionPerRound = 0.015f;
    public float minimumTolerance = 0.05f;
    public float retryCooldown = 0.35f;

    [Header("Feedback visual")]
    public Color neutralTint = Color.white;
    public Color warningTint = new Color(1f, 0.84f, 0.84f, 1f);
    public Color successTint = new Color(0.74f, 1f, 0.76f, 1f);
    public Color invalidClickTint = new Color(1f, 0.95f, 0.75f, 1f);
    public Color failTint = new Color(1f, 0.55f, 0.55f, 1f);
    public float flashDuration = 0.16f;
    public float riskTintStrength = 0.7f;

    public System.Action<bool> OnGameFinished;

    private bool juegoActivo;
    private bool waitingRetryFeedback;
    private int ronda = 1;
    private int attemptsLeft;
    private int maxAttemptsThisRound;
    private float baseTolerance;
    private Texture2D texturaFigura;
    private Coroutine tintRoutine;

    private void Awake()
    {
        baseTolerance = tolerancia;

        if (statusText == null && autoFindStatusText)
        {
            statusText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    public void SetRound(int r)
    {
        ronda = Mathf.Max(1, r);

        int textureIndex = Mathf.Clamp(ronda - 1, 0, Mathf.Max(0, figurasPorRonda.Count - 1));
        if (figurasPorRonda.Count > 0)
        {
            texturaFigura = figurasPorRonda[textureIndex];
            if (galletaImage != null)
            {
                galletaImage.texture = texturaFigura;
            }
        }
        else
        {
            Debug.LogWarning("No hay texturas asignadas para Dalgona.");
        }

        tolerancia = Mathf.Max(minimumTolerance, baseTolerance - (ronda - 1) * toleranceReductionPerRound);
        maxAttemptsThisRound = Mathf.Max(minAttempts, baseAttempts - (ronda - 1));
        attemptsLeft = maxAttemptsThisRound;

        if (juegoActivo)
        {
            UpdateAttemptVisual();
            UpdateStatus("Recorta con cuidado");
        }
    }

    private void OnEnable()
    {
        if (texturaFigura == null)
        {
            SetRound(ronda);
        }

        if (attemptsLeft <= 0)
        {
            maxAttemptsThisRound = Mathf.Max(minAttempts, baseAttempts - (ronda - 1));
            attemptsLeft = maxAttemptsThisRound;
        }

        juegoActivo = true;
        waitingRetryFeedback = false;

        if (statusText != null)
        {
            statusText.gameObject.SetActive(useStatusText);
            if (useStatusText)
            {
                statusText.fontSize = Mathf.Max(32f, statusText.fontSize);
                statusText.alignment = TextAlignmentOptions.Center;
            }
        }

        UpdateAttemptVisual();
        UpdateStatus("Recorta con cuidado");
    }

    private void OnDisable()
    {
        if (tintRoutine != null)
        {
            StopCoroutine(tintRoutine);
            tintRoutine = null;
        }
    }

    private void Update()
    {
        if (!juegoActivo || waitingRetryFeedback)
        {
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            bool clickedInsideTexture;
            bool success = VerificarRecorte(out clickedInsideTexture);

            if (success)
            {
                juegoActivo = false;
                UpdateStatus("Exito");
                if (tintRoutine != null)
                {
                    StopCoroutine(tintRoutine);
                }
                tintRoutine = StartCoroutine(FinishWithTint(true));
                return;
            }

            if (!clickedInsideTexture)
            {
                UpdateStatus("Clic dentro de la figura");
                FlashTint(invalidClickTint);
                return;
            }

            attemptsLeft--;
            UpdateAttemptVisual();

            if (attemptsLeft <= 0)
            {
                juegoActivo = false;
                UpdateStatus("Fallaste");
                if (tintRoutine != null)
                {
                    StopCoroutine(tintRoutine);
                }
                tintRoutine = StartCoroutine(FinishWithTint(false));
                return;
            }

            StartCoroutine(RetryCooldownRoutine());
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Return))
        {
            juegoActivo = false;
            if (tintRoutine != null)
            {
                StopCoroutine(tintRoutine);
            }
            tintRoutine = StartCoroutine(FinishWithTint(true));
        }
#endif
    }

    private IEnumerator RetryCooldownRoutine()
    {
        waitingRetryFeedback = true;
        UpdateStatus("Intenta de nuevo");
        FlashTint(warningTint);

        yield return new WaitForSeconds(retryCooldown);

        waitingRetryFeedback = false;
        if (juegoActivo)
        {
            UpdateStatus("Recorta con cuidado");
        }
    }

    private IEnumerator FinishWithTint(bool success)
    {
        Color target = success ? successTint : failTint;
        FlashTint(target);

        yield return new WaitForSeconds(flashDuration + 0.08f);
        OnGameFinished?.Invoke(success);
    }

    private bool VerificarRecorte(out bool clickedInsideTexture)
    {
        clickedInsideTexture = false;

        if (galletaImage == null || texturaFigura == null)
        {
            return false;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(galletaImage.rectTransform, Input.mousePosition, null, out Vector2 localPos))
        {
            return false;
        }

        Rect rect = galletaImage.rectTransform.rect;
        Vector2 uv = new Vector2((localPos.x - rect.x) / rect.width, (localPos.y - rect.y) / rect.height);

        if (uv.x < 0f || uv.x > 1f || uv.y < 0f || uv.y > 1f)
        {
            return false;
        }

        clickedInsideTexture = true;

        int x = Mathf.Clamp(Mathf.FloorToInt(uv.x * texturaFigura.width), 0, texturaFigura.width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(uv.y * texturaFigura.height), 0, texturaFigura.height - 1);
        Color pixel = texturaFigura.GetPixel(x, y);

        float diferencia = Vector4.Distance(pixel, colorFigura);
        return diferencia <= tolerancia;
    }

    private void FlashTint(Color tint)
    {
        if (galletaImage == null)
        {
            return;
        }

        if (tintRoutine != null)
        {
            StopCoroutine(tintRoutine);
        }

        tintRoutine = StartCoroutine(FlashTintRoutine(tint));
    }

    private IEnumerator FlashTintRoutine(Color tint)
    {
        if (galletaImage == null)
        {
            yield break;
        }

        Color baseColor = ComputeBaseTint();
        galletaImage.color = tint;

        yield return new WaitForSeconds(flashDuration);

        if (galletaImage != null)
        {
            galletaImage.color = baseColor;
        }

        tintRoutine = null;
    }

    private void UpdateAttemptVisual()
    {
        if (galletaImage == null)
        {
            return;
        }

        if (tintRoutine == null)
        {
            galletaImage.color = ComputeBaseTint();
        }
    }

    private Color ComputeBaseTint()
    {
        if (maxAttemptsThisRound <= 0)
        {
            return neutralTint;
        }

        float attemptsRatio = Mathf.Clamp01(attemptsLeft / (float)maxAttemptsThisRound);
        float risk = 1f - attemptsRatio;
        return Color.Lerp(neutralTint, warningTint, risk * riskTintStrength);
    }

    private void UpdateStatus(string message)
    {
        if (!useStatusText || statusText == null)
        {
            return;
        }

        statusText.text = message;
    }
}
