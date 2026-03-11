using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimonManager2 : MonoBehaviour
{
    public enum FeedbackMode
    {
        VisualOnly,
        VisualWithText
    }

    [Header("Botones de colores")]
    public Button greenButton;
    public Button redButton;
    public Button yellowButton;
    public Button blueButton;

    [Header("UI de estado")]
    public FeedbackMode feedbackMode = FeedbackMode.VisualOnly;
    public TextMeshProUGUI statusText;

    [Header("Dificultad base")]
    public float baseFlashTime = 0.33f;
    public float baseTimeBetweenFlashes = 0.85f;
    public int baseMaxTurns = 4;

    [Header("Escalado por ronda")]
    public int extraTurnsPerRound = 1;
    public float flashTimeReductionPerRound = 0.03f;
    public float betweenFlashesReductionPerRound = 0.07f;
    public float minFlashTime = 0.2f;
    public float minTimeBetweenFlashes = 0.35f;

    [Header("Feedback visual")]
    public float startDelay = 0.75f;
    public float observeBrightness = 0.52f;
    public float readyPulseScale = 1.08f;
    public float readyPulseDuration = 0.12f;
    public Color successTint = new Color(0.65f, 1f, 0.65f, 1f);
    public Color failTint = new Color(1f, 0.45f, 0.45f, 1f);

    [Header("Progreso visual")]
    public Transform progressContainer;
    public Vector2 progressAnchoredPosition = new Vector2(0f, -58f);
    public Vector2 progressDotSize = new Vector2(16f, 16f);
    public float progressSpacing = 10f;
    public Color dotPendingColor = new Color(1f, 1f, 1f, 0.28f);
    public Color dotDoneColor = new Color(1f, 1f, 1f, 0.95f);
    public Color dotPlaybackColor = new Color(0.72f, 0.92f, 1f, 0.95f);
    public float dotPopScale = 1.28f;
    public float dotPopDuration = 0.08f;

    private readonly List<Button> availableButtons = new();
    private readonly List<Button> pattern = new();
    private readonly List<Button> playerInput = new();
    private readonly List<Image> progressDots = new();
    private readonly Dictionary<Button, Color> baseButtonColors = new();
    private readonly Dictionary<Button, Vector3> baseButtonScales = new();

    private Coroutine activeRoutine;

    private float flashTime;
    private float timeBetweenFlashes;
    private int maxTurns;

    private bool isPlayerTurn;
    private bool gameActive;
    private bool isFlashing;
    private int currentTurn;
    private float currentBrightness = 1f;

    public System.Action<bool> OnGameFinished;

    public void SetRound(int gameRound)
    {
        int roundIndex = Mathf.Max(0, gameRound - 1);
        maxTurns = baseMaxTurns + roundIndex * extraTurnsPerRound;
        flashTime = Mathf.Max(minFlashTime, baseFlashTime - flashTimeReductionPerRound * roundIndex);
        timeBetweenFlashes = Mathf.Max(minTimeBetweenFlashes, baseTimeBetweenFlashes - betweenFlashesReductionPerRound * roundIndex);
    }

    private void Awake()
    {
        CacheButtons();
        EnsureProgressContainer();
    }

    private void OnEnable()
    {
        EnsureDefaults();
        ConfigureStatusText();
        BuildProgressDots(0);
        BeginSession();
    }

    private void OnDisable()
    {
        StopActiveRoutine();
        SetButtonsInteractable(false);
        ResetButtonVisuals();
    }

    private void CacheButtons()
    {
        availableButtons.Clear();
        baseButtonColors.Clear();
        baseButtonScales.Clear();

        Button[] configuredButtons = { greenButton, redButton, yellowButton, blueButton };
        foreach (Button button in configuredButtons)
        {
            if (button == null || button.image == null)
            {
                continue;
            }

            availableButtons.Add(button);
            baseButtonColors[button] = button.image.color;
            baseButtonScales[button] = button.transform.localScale;

            Button captured = button;
            captured.onClick.AddListener(() => OnButtonClick(captured));
        }
    }

    private void EnsureDefaults()
    {
        if (maxTurns <= 0)
        {
            SetRound(1);
        }
    }

    private void ConfigureStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        bool useText = feedbackMode == FeedbackMode.VisualWithText;
        statusText.gameObject.SetActive(useText);

        if (useText)
        {
            statusText.fontSize = Mathf.Max(34f, statusText.fontSize);
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.alpha = 0.92f;
        }
    }

    private void EnsureProgressContainer()
    {
        if (progressContainer != null)
        {
            return;
        }

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null && availableButtons.Count > 0)
        {
            rootCanvas = availableButtons[0].GetComponentInParent<Canvas>();
        }

        if (rootCanvas == null)
        {
            return;
        }

        GameObject container = new GameObject("SimonProgress", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        RectTransform rect = container.GetComponent<RectTransform>();
        rect.SetParent(rootCanvas.transform, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = progressAnchoredPosition;
        rect.sizeDelta = new Vector2(420f, 26f);

        HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = progressSpacing;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        progressContainer = rect;
    }

    private void BeginSession()
    {
        StopActiveRoutine();

        pattern.Clear();
        playerInput.Clear();
        currentTurn = 0;
        isPlayerTurn = false;
        gameActive = false;
        isFlashing = false;

        SetButtonsInteractable(false);
        SetButtonBrightness(observeBrightness);
        SetStatus("Memoriza el patron");

        activeRoutine = StartCoroutine(BeginAfterDelay());
    }

    private IEnumerator BeginAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        yield return StartCoroutine(NextTurn());
    }

    private IEnumerator NextTurn()
    {
        if (availableButtons.Count == 0)
        {
            yield break;
        }

        isPlayerTurn = false;
        gameActive = false;
        isFlashing = true;
        SetButtonsInteractable(false);
        SetButtonBrightness(observeBrightness);

        playerInput.Clear();
        currentTurn++;

        if (currentTurn > maxTurns)
        {
            SetStatus("Completado");
            yield return StartCoroutine(FlashAllButtons(successTint, 1));
            yield return new WaitForSeconds(0.15f);
            OnGameFinished?.Invoke(true);
            yield break;
        }

        Button nextButton = availableButtons[Random.Range(0, availableButtons.Count)];
        pattern.Add(nextButton);

        BuildProgressDots(pattern.Count);
        UpdateProgressDots(0);
        SetStatus("Observa");

        yield return new WaitForSeconds(0.65f);

        for (int i = 0; i < pattern.Count; i++)
        {
            AnimateProgressDot(i, dotPlaybackColor);
            yield return StartCoroutine(FlashButton(pattern[i], 1f, 1.2f));
            yield return new WaitForSeconds(timeBetweenFlashes);
        }

        UpdateProgressDots(0);

        isFlashing = false;
        isPlayerTurn = true;
        gameActive = true;
        playerInput.Clear();

        SetButtonBrightness(1f);
        SetButtonsInteractable(true);
        StartCoroutine(PulseReadyState());
        SetStatus("Tu turno");
    }

    private IEnumerator FlashButton(Button button, float durationMultiplier = 1f, float scaleMultiplier = 1.16f)
    {
        if (button == null || button.image == null)
        {
            yield break;
        }

        Image image = button.image;
        Vector3 originalScale = baseButtonScales[button];

        image.color = Color.white;
        button.transform.localScale = originalScale * scaleMultiplier;

        yield return new WaitForSeconds(flashTime * durationMultiplier);

        image.color = MultiplyColor(baseButtonColors[button], currentBrightness);
        button.transform.localScale = originalScale;
    }

    private IEnumerator PulseReadyState()
    {
        foreach (Button button in availableButtons)
        {
            if (button == null)
            {
                continue;
            }

            Vector3 originalScale = baseButtonScales[button];
            button.transform.localScale = originalScale * readyPulseScale;
        }

        yield return new WaitForSeconds(readyPulseDuration);

        foreach (Button button in availableButtons)
        {
            if (button == null)
            {
                continue;
            }

            button.transform.localScale = baseButtonScales[button];
        }
    }

    private void OnButtonClick(Button clickedButton)
    {
        if (!isPlayerTurn || !gameActive || isFlashing)
        {
            return;
        }

        StartCoroutine(FlashButton(clickedButton, 0.58f, 1.12f));

        playerInput.Add(clickedButton);
        int index = playerInput.Count - 1;

        if (index >= pattern.Count)
        {
            return;
        }

        if (clickedButton != pattern[index])
        {
            SetStatus("Error");
            isPlayerTurn = false;
            gameActive = false;
            SetButtonsInteractable(false);

            StopActiveRoutine();
            activeRoutine = StartCoroutine(HandleLoss());
            return;
        }

        UpdateProgressDots(playerInput.Count);
        AnimateProgressDot(playerInput.Count - 1, dotDoneColor);

        if (playerInput.Count == pattern.Count)
        {
            SetStatus("Bien");
            isPlayerTurn = false;
            gameActive = false;
            SetButtonsInteractable(false);

            StopActiveRoutine();
            activeRoutine = StartCoroutine(AdvanceTurnAfterDelay());
        }
    }

    private IEnumerator AdvanceTurnAfterDelay()
    {
        yield return StartCoroutine(FlashAllButtons(successTint, 1));
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(NextTurn());
    }

    private IEnumerator HandleLoss()
    {
        yield return StartCoroutine(FlashAllButtons(failTint, 2));
        yield return new WaitForSeconds(0.2f);
        OnGameFinished?.Invoke(false);
    }

    private IEnumerator FlashAllButtons(Color tintColor, int loops)
    {
        for (int loop = 0; loop < loops; loop++)
        {
            foreach (Button button in availableButtons)
            {
                if (button == null || button.image == null)
                {
                    continue;
                }

                Color blended = Color.Lerp(baseButtonColors[button], tintColor, 0.75f);
                button.image.color = blended;
            }

            yield return new WaitForSeconds(0.12f);

            foreach (Button button in availableButtons)
            {
                if (button == null || button.image == null)
                {
                    continue;
                }

                button.image.color = MultiplyColor(baseButtonColors[button], currentBrightness);
            }

            yield return new WaitForSeconds(0.08f);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in availableButtons)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }

    private void SetButtonBrightness(float brightness)
    {
        currentBrightness = Mathf.Clamp(brightness, 0.15f, 1f);

        foreach (Button button in availableButtons)
        {
            if (button == null || button.image == null)
            {
                continue;
            }

            button.image.color = MultiplyColor(baseButtonColors[button], currentBrightness);
        }
    }

    private void ResetButtonVisuals()
    {
        currentBrightness = 1f;

        foreach (Button button in availableButtons)
        {
            if (button == null || button.image == null)
            {
                continue;
            }

            button.image.color = baseButtonColors[button];
            button.transform.localScale = baseButtonScales[button];
        }
    }

    private Color MultiplyColor(Color original, float multiplier)
    {
        return new Color(original.r * multiplier, original.g * multiplier, original.b * multiplier, original.a);
    }

    private void BuildProgressDots(int total)
    {
        if (progressContainer == null)
        {
            return;
        }

        while (progressDots.Count < total)
        {
            GameObject dotObject = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            RectTransform rect = dotObject.GetComponent<RectTransform>();
            rect.SetParent(progressContainer, false);
            rect.sizeDelta = progressDotSize;

            Image dot = dotObject.GetComponent<Image>();
            dot.color = dotPendingColor;
            dot.rectTransform.localScale = Vector3.one;
            progressDots.Add(dot);
        }

        for (int i = 0; i < progressDots.Count; i++)
        {
            bool active = i < total;
            progressDots[i].gameObject.SetActive(active);
        }

        UpdateProgressDots(0);
    }

    private void UpdateProgressDots(int completed)
    {
        for (int i = 0; i < progressDots.Count; i++)
        {
            if (!progressDots[i].gameObject.activeSelf)
            {
                continue;
            }

            progressDots[i].color = i < completed ? dotDoneColor : dotPendingColor;
        }
    }

    private void AnimateProgressDot(int index, Color targetColor)
    {
        if (index < 0 || index >= progressDots.Count)
        {
            return;
        }

        Image dot = progressDots[index];
        if (dot == null || !dot.gameObject.activeSelf)
        {
            return;
        }

        StartCoroutine(PopDotRoutine(dot, targetColor));
    }

    private IEnumerator PopDotRoutine(Image dot, Color targetColor)
    {
        RectTransform rect = dot.rectTransform;
        Vector3 baseScale = Vector3.one;

        dot.color = targetColor;
        rect.localScale = baseScale * dotPopScale;
        yield return new WaitForSeconds(dotPopDuration);

        rect.localScale = baseScale;
    }

    private void SetStatus(string message)
    {
        if (feedbackMode != FeedbackMode.VisualWithText || statusText == null)
        {
            return;
        }

        statusText.text = message;
    }

    private void StopActiveRoutine()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Return) && isPlayerTurn && !isFlashing)
        {
            isPlayerTurn = false;
            gameActive = false;
            SetButtonsInteractable(false);

            StopActiveRoutine();
            activeRoutine = StartCoroutine(AdvanceTurnAfterDelay());
        }
#endif
    }
}
