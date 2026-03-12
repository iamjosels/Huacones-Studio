using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DalgonaGameManager : MonoBehaviour
{
    private enum MatchState
    {
        Disabled,
        Intro,
        Playing,
        Won,
        Lost
    }

    [Header("Core")]
    public RawImage galletaImage;
    public Color colorFigura = new Color32(33, 60, 132, 255);
    [Range(0.01f, 0.45f)] public float tolerancia = 0.08f;
    [Tooltip("Texturas por ronda (1=facil, 2=media, 3=dificil).")]
    public List<Texture2D> figurasPorRonda = new();

    [Header("Status UI (optional)")]
    public bool useStatusText = false;
    public TextMeshProUGUI statusText;
    public bool autoFindStatusText = true;

    [Header("Round Balance")]
    public float baseTimeLimit = 36f;
    public float timeLimitReductionPerRound = 1.2f;
    public float minimumTimeLimit = 24f;
    [Range(0.3f, 0.95f)] public float baseRequiredCoverage = 0.32f;
    public float coverageIncreasePerRound = 0.02f;
    [Range(0.3f, 0.98f)] public float maximumRequiredCoverage = 0.42f;
    public float baseIntegrity = 100f;
    public float integrityReductionPerRound = 14f;
    public float minimumIntegrity = 45f;
    public float introLockDuration = 0.6f;
    public float finishDelay = 0.35f;

    [Header("Completion Validation")]
    [Range(0.3f, 1f)] public float strictManualRequiredCoverage = 0.9f;
    [Range(0.5f, 1f)] public float strictManualCheckpointRatio = 0.98f;
    public bool requireManualCheckpoints = true;
    public int manualCheckpointHitRadius = 6;

    [Header("Mistake Feedback")]
    public bool useMistakeCounter = true;
    public bool useMistakesForOffPathDamage = true;
    public int baseAllowedMistakes = 4;
    public int allowedMistakeReductionPerRound = 1;
    public int minimumAllowedMistakes = 2;
    public float mistakeCooldown = 0.22f;
    public float damagePerMistake = 13f;

    [Header("Needle and Damage")]
    public float offPathDamagePerSecond = 6f;
    public float pressureDamagePerSecond = 8f;
    [Range(0f, 1f)] public float pressureThresholdToDamage = 0.9f;
    public float pressureBuildPerSecond = 0.3f;
    public float pressureReleasePerSecond = 0.95f;
    public float pressureBuildOnPathMultiplier = 0.33f;
    public float pressureBuildOffPathMultiplier = 0.7f;
    public float stationaryPressureBonus = 0.08f;
    public float stationarySpeedThreshold = 3f;
    public float minCursorStepPixels = 0.9f;

    [Header("Path Detection")]
    public float minAlphaForPath = 0.1f;
    public int detectionRadius = 4;
    public int assistRadiusBonus = 5;
    public int validBrushRadius = 5;
    public int invalidBrushRadius = 1;
    public int cutAssistFillRadius = 6;
    public int cutAssistMaxPixelsPerHit = 220;
    [Range(0f, 1f)] public float cutAssistFillStrength = 0.35f;
    public int minComponentPixels = 20;
    public bool ignoreEdgeConnectedPath = true;
    public bool fallbackToDarkLineDetection = true;
    [Range(0f, 1f)] public float darkLineMaxLuminance = 0.56f;
    [Range(0f, 1f)] public float darkLineMinSaturation = 0.12f;
    public int minimumTargetPixelsToAcceptColorMask = 120;

    [Header("Manual Cut Path")]
    public bool useManualCutPath = true;
    public bool usePerRoundManualPathRoots = true;
    public Transform manualPathRoundsRoot;
    public Transform manualPathPointsRoot;
    public bool manualPathClosed = true;
    public float manualPathWidthUi = 52f;
    public float manualPathSampleSpacingUi = 7f;
    public bool autoCreateManualPathPoints = true;
    [Range(4, 64)] public int defaultManualPointCount = 16;
    [Range(0.1f, 0.9f)] public float defaultManualRadiusNormalized = 0.36f;

    [Header("Visual Feedback")]
    public Color neutralTint = Color.white;
    public Color warningTint = new Color(1f, 0.9f, 0.86f, 1f);
    public Color highPressureTint = new Color(1f, 0.74f, 0.62f, 1f);
    public Color successTint = new Color(0.75f, 1f, 0.8f, 1f);
    public Color failTint = new Color(1f, 0.56f, 0.56f, 1f);
    public Color tracedPathColor = new Color(0.97f, 0.98f, 1f, 0.4f);
    public Color crackPathColor = new Color(0.86f, 0.5f, 0.36f, 0.5f);
    [Range(0f, 1f)] public float tracedPaintStrength = 0.42f;
    [Range(0f, 1f)] public float crackPaintStrength = 0.2f;
    public float flashDuration = 0.14f;
    public float shakeDuration = 0.12f;
    public float shakeStrength = 4.5f;
    public float damageFlashCooldown = 0.11f;

    [Header("Scissor Cursor")]
    public bool showScissorCursor = true;
    public Vector2 scissorCursorSize = new Vector2(34f, 34f);
    public Vector2 scissorBladeSize = new Vector2(22f, 3f);
    public Color scissorBladeColor = new Color(0.92f, 0.95f, 1f, 0.95f);
    public Color scissorHandleColor = new Color(1f, 0.74f, 0.45f, 0.95f);
    public float scissorOpenAngle = 18f;
    public float scissorClosedAngle = 6f;
    public float scissorPulseSpeed = 16f;
    public float scissorSmooth = 22f;
    public bool rotateScissorWithMovement = false;
    public float scissorIdleAngle = -16f;
    public float scissorMovementThreshold = 8f;

    [Header("Runtime HUD")]
    public bool createRuntimeHud = false;
    public RectTransform hudRoot;
    public Vector2 hudAnchoredPosition = new Vector2(0f, -185f);
    public Vector2 hudSize = new Vector2(340f, 78f);
    public Color hudBackgroundColor = new Color(0f, 0f, 0f, 0.35f);
    public Color progressBarColor = new Color(0.35f, 0.88f, 1f, 0.95f);
    public Color integrityBarColor = new Color(0.91f, 0.84f, 0.43f, 0.95f);
    public Color pressureBarColor = new Color(1f, 0.45f, 0.34f, 0.95f);
    public Vector2 barSize = new Vector2(292f, 14f);
    public float barSpacing = 21f;
    public Color hudTextColor = new Color(0.08f, 0.08f, 0.08f, 0.96f);
    public int hudTextSize = 17;
    public Vector2 hudTopTextOffset = new Vector2(0f, 35f);
    public Vector2 hudBottomTextOffset = new Vector2(0f, -35f);

    [Header("Performance")]
    public float textureApplyInterval = 0.02f;

    public System.Action<bool> OnGameFinished;

    private MatchState state = MatchState.Disabled;
    private int ronda = 1;

    private float timeLimit;
    private float timeLeft;
    private float requiredCoverage;
    private float maxIntegrity;
    private float integrity;
    private float pressure;

    private bool pointerHeld;
    private bool hasLastPixel;
    private Vector2Int lastPixel;

    private float nextTextureApplyTime;
    private bool textureDirty;
    private float nextDamageFlashTime;
    private int crackStage;
    private float introEndTime;

    private RectTransform cookieRect;
    private Vector2 cookieBasePosition;

    private Texture2D sourceTexture;
    private Texture2D runtimeTexture;
    private Texture2D transientReadableCopy;
    private Color[] sourcePixels;
    private Color[] runtimePixels;
    private bool[] targetMask;
    private bool[] visitedMask;
    private int textureWidth;
    private int textureHeight;
    private int targetPixelCount;
    private int visitedPixelCount;
    private bool manualMaskActive;
    private Vector2Int[] manualCheckpointPixels;
    private bool[] manualCheckpointVisited;
    private int manualCheckpointVisitedCount;

    private Coroutine tintRoutine;
    private Coroutine shakeRoutine;
    private bool tintOverrideActive;

    private Image progressFill;
    private Image integrityFill;
    private Image pressureFill;
    private TextMeshProUGUI hudTopText;
    private TextMeshProUGUI hudBottomText;

    private int allowedMistakes;
    private int mistakesMade;
    private float nextMistakeTime;

    private RectTransform scissorCursorRoot;
    private RectTransform scissorBladeTop;
    private RectTransform scissorBladeBottom;
    private RectTransform scissorHandle;
    private Vector2 scissorLastScreenPosition;
    private bool hasScissorLastScreenPosition;
    private float scissorCurrentAngle = 24f;
    private float scissorTargetAngle = 24f;

    private void Awake()
    {
        if (statusText == null && autoFindStatusText)
        {
            statusText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (galletaImage != null)
        {
            cookieRect = galletaImage.rectTransform;
            cookieBasePosition = cookieRect.anchoredPosition;
        }
    }

    private void OnValidate()
    {
        manualPathWidthUi = Mathf.Max(1f, manualPathWidthUi);
        manualPathSampleSpacingUi = Mathf.Max(0.5f, manualPathSampleSpacingUi);
        defaultManualPointCount = Mathf.Clamp(defaultManualPointCount, 4, 64);
        defaultManualRadiusNormalized = Mathf.Clamp(defaultManualRadiusNormalized, 0.1f, 0.9f);
        strictManualRequiredCoverage = Mathf.Clamp01(strictManualRequiredCoverage);
        strictManualCheckpointRatio = Mathf.Clamp01(strictManualCheckpointRatio);
        manualCheckpointHitRadius = Mathf.Max(1, manualCheckpointHitRadius);
        baseAllowedMistakes = Mathf.Max(1, baseAllowedMistakes);
        minimumAllowedMistakes = Mathf.Max(1, minimumAllowedMistakes);
        allowedMistakeReductionPerRound = Mathf.Max(0, allowedMistakeReductionPerRound);
        mistakeCooldown = Mathf.Max(0.01f, mistakeCooldown);
        hudTextSize = Mathf.Clamp(hudTextSize, 10, 44);
    }

    private void OnEnable()
    {
        StartRound();
    }

    private void OnDisable()
    {
        StopFeedbackCoroutines();
        state = MatchState.Disabled;
        pointerHeld = false;
        hasLastPixel = false;
        tintOverrideActive = false;

        if (cookieRect != null)
        {
            cookieRect.anchoredPosition = cookieBasePosition;
        }

        if (galletaImage != null)
        {
            galletaImage.color = neutralTint;
            if (sourceTexture != null)
            {
                galletaImage.texture = sourceTexture;
            }
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(useStatusText);
        }

        if (hudRoot != null)
        {
            hudRoot.gameObject.SetActive(false);
        }

        SetScissorCursorVisible(false);
        hasScissorLastScreenPosition = false;

        CleanupRuntimeTexture();
    }

    private void OnDestroy()
    {
        CleanupRuntimeTexture();
    }

    public void SetRound(int r)
    {
        ronda = Mathf.Max(1, r);

        if (isActiveAndEnabled)
        {
            StartRound();
        }
    }

    public int GetAuthoringRoundCount()
    {
        return GetManualPathRoundCount();
    }

    public Texture2D GetTextureForRound(int round)
    {
        if (figurasPorRonda != null && figurasPorRonda.Count > 0)
        {
            int index = Mathf.Clamp(round - 1, 0, figurasPorRonda.Count - 1);
            return figurasPorRonda[index];
        }

        return galletaImage != null ? galletaImage.texture as Texture2D : null;
    }

    public void PreviewRoundTextureInEditor(int round)
    {
        if (galletaImage == null || Application.isPlaying)
        {
            return;
        }

        Texture2D preview = GetTextureForRound(Mathf.Max(1, round));
        if (preview != null)
        {
            galletaImage.texture = preview;
        }
    }

    public bool TryEditorWorldToTexturePixel(Vector3 worldPosition, int round, out Vector2Int pixel)
    {
        pixel = default;

        if (galletaImage == null)
        {
            return false;
        }

        Texture2D texture = GetTextureForRound(round);
        if (texture == null)
        {
            return false;
        }

        RectTransform rectTransform = galletaImage.rectTransform;
        Rect rect = rectTransform.rect;
        if (Mathf.Abs(rect.width) < 0.0001f || Mathf.Abs(rect.height) < 0.0001f)
        {
            return false;
        }

        Vector2 local = rectTransform.InverseTransformPoint(worldPosition);
        float u = (local.x - rect.x) / rect.width;
        float v = (local.y - rect.y) / rect.height;
        if (u < 0f || u > 1f || v < 0f || v > 1f)
        {
            return false;
        }

        int x = Mathf.Clamp(Mathf.RoundToInt(u * (texture.width - 1)), 0, texture.width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(v * (texture.height - 1)), 0, texture.height - 1);
        pixel = new Vector2Int(x, y);
        return true;
    }

    public bool TryEditorTexturePixelToWorld(Vector2Int pixel, int round, out Vector3 worldPosition)
    {
        worldPosition = default;

        if (galletaImage == null)
        {
            return false;
        }

        Texture2D texture = GetTextureForRound(round);
        if (texture == null || texture.width <= 1 || texture.height <= 1)
        {
            return false;
        }

        RectTransform rectTransform = galletaImage.rectTransform;
        Rect rect = rectTransform.rect;
        if (Mathf.Abs(rect.width) < 0.0001f || Mathf.Abs(rect.height) < 0.0001f)
        {
            return false;
        }

        int px = Mathf.Clamp(pixel.x, 0, texture.width - 1);
        int py = Mathf.Clamp(pixel.y, 0, texture.height - 1);

        float u = px / (float)(texture.width - 1);
        float v = py / (float)(texture.height - 1);
        float localX = rect.x + (u * rect.width);
        float localY = rect.y + (v * rect.height);

        worldPosition = rectTransform.TransformPoint(new Vector3(localX, localY, 0f));
        return true;
    }

    public Transform GetOrCreateManualPathRootForRound(int round, bool rebuildPoints)
    {
        int previousRound = ronda;
        ronda = Mathf.Max(1, round);

        EnsureManualPathSetup(rebuildPoints);
        Transform root = ResolveManualPathPointsRootForCurrentRound();

        ronda = previousRound;
        return root;
    }

    public Transform GetManualPathRootForRound(int round)
    {
        int previousRound = ronda;
        ronda = Mathf.Max(1, round);
        Transform root = ResolveManualPathPointsRootForCurrentRound();
        ronda = previousRound;
        return root;
    }

    [ContextMenu("Manual Path/Recreate Placeholder Points")]
    private void RecreateManualPathPlaceholderPoints()
    {
        EnsureManualPathSetup(true);
    }

    private void Update()
    {
        if (state == MatchState.Disabled || state == MatchState.Won || state == MatchState.Lost)
        {
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Return))
        {
            CompleteMatch(true, "Debug win");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            CompleteMatch(false, "Debug fail");
            return;
        }
#endif

        ReadPointerState(out bool pointerDown, out bool pointerActive, out bool pointerUp, out Vector2 pointerPos);
        UpdateScissorCursor(pointerPos, pointerActive);

        if (state == MatchState.Intro)
        {
            if (Time.time >= introEndTime)
            {
                state = MatchState.Playing;
                UpdateStatus(BuildPlayingStatusMessage());
            }

            UpdateHud();
            ApplyRuntimeTextureIfNeeded();
            return;
        }

        if (state != MatchState.Playing)
        {
            return;
        }

        float dt = Time.deltaTime;
        timeLeft -= dt;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            CompleteMatch(false, "Tiempo agotado");
            return;
        }

        if (pointerDown)
        {
            pointerHeld = true;
            hasLastPixel = false;
        }

        if (pointerUp)
        {
            pointerHeld = false;
            hasLastPixel = false;
        }

        bool touchedPath = false;
        bool touchedOffPath = false;
        float pointerSpeed = 0f;
        bool pointerInsideCookie = false;

        if (pointerActive)
        {
            touchedPath = TraceFromPointer(pointerPos, dt, out touchedOffPath, out pointerSpeed, out pointerInsideCookie);
        }
        else
        {
            pointerHeld = false;
            hasLastPixel = false;
        }

        UpdatePressure(dt, pointerHeld && pointerInsideCookie, touchedPath, touchedOffPath, pointerSpeed);

        if (touchedOffPath)
        {
            if (useMistakeCounter)
            {
                RegisterMistake();
            }

            if (!useMistakeCounter || !useMistakesForOffPathDamage)
            {
                ApplyDamage(offPathDamagePerSecond * dt);
            }
        }

        if (HasReachedWinCondition())
        {
            CompleteMatch(true, "Exito");
            return;
        }

        UpdateCrackStageFeedback();
        UpdateHud();
        RefreshCookieTint();
        ApplyRuntimeTextureIfNeeded();
    }

    private void EnsureManualPathSetup(bool forceRebuild)
    {
        if (!useManualCutPath || !autoCreateManualPathPoints)
        {
            return;
        }

        if (usePerRoundManualPathRoots)
        {
            if (!EnsureManualPathRoundsRootExists())
            {
                return;
            }

            if (forceRebuild)
            {
                ClearManualPathChildren(manualPathRoundsRoot);
            }

            EnsureRoundManualPathRoots(GetManualPathRoundCount());
            manualPathPointsRoot = ResolveManualPathPointsRootForCurrentRound();
            return;
        }

        if (!EnsureManualPathRootExists())
        {
            return;
        }

        if (forceRebuild)
        {
            ClearManualPathChildren(manualPathPointsRoot);
        }

        if (manualPathPointsRoot == null)
        {
            return;
        }

        if (manualPathPointsRoot.childCount == 0)
        {
            CreateDefaultManualPathPoints(manualPathPointsRoot);
        }
    }

    private int GetManualPathRoundCount()
    {
        int fromTextures = figurasPorRonda != null ? figurasPorRonda.Count : 0;
        return Mathf.Max(3, fromTextures);
    }

    private bool TryResolveManualPathParent(out Transform parent, out bool useRectTransforms)
    {
        parent = transform;
        useRectTransforms = false;

        if (galletaImage != null)
        {
            parent = galletaImage.rectTransform;
            useRectTransforms = true;
        }

        return parent != null;
    }

    private void ConfigurePathRootTransform(Transform root)
    {
        if (root is RectTransform rectRoot)
        {
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);
            rectRoot.pivot = new Vector2(0.5f, 0.5f);
            rectRoot.sizeDelta = Vector2.zero;
            rectRoot.anchoredPosition = Vector2.zero;
        }
    }

    private bool EnsureManualPathRoundsRootExists()
    {
        if (manualPathRoundsRoot != null)
        {
            return true;
        }

        if (!TryResolveManualPathParent(out Transform parent, out bool useRectTransforms))
        {
            return false;
        }

        GameObject roundsRootObject = new GameObject("ManualCutPathsByRound", useRectTransforms ? typeof(RectTransform) : typeof(Transform));
        manualPathRoundsRoot = roundsRootObject.transform;
        manualPathRoundsRoot.SetParent(parent, false);
        ConfigurePathRootTransform(manualPathRoundsRoot);

        if (manualPathPointsRoot != null && manualPathPointsRoot != manualPathRoundsRoot)
        {
            manualPathPointsRoot.SetParent(manualPathRoundsRoot, false);
            manualPathPointsRoot.name = "Round_1";
        }

        return true;
    }

    private void EnsureRoundManualPathRoots(int roundCount)
    {
        if (manualPathRoundsRoot == null)
        {
            return;
        }

        int clampedRoundCount = Mathf.Max(1, roundCount);
        for (int round = 1; round <= clampedRoundCount; round++)
        {
            Transform roundRoot = FindManualPathRoundRoot(round);
            if (roundRoot == null)
            {
                roundRoot = CreateManualRoundRoot(round);
            }

            if (roundRoot != null && roundRoot.childCount == 0)
            {
                CreateDefaultManualPathPoints(roundRoot);
            }
        }

        if (manualPathPointsRoot == null)
        {
            manualPathPointsRoot = FindManualPathRoundRoot(1);
        }
    }

    private Transform CreateManualRoundRoot(int round)
    {
        if (manualPathRoundsRoot == null)
        {
            return null;
        }

        bool useRectTransforms = manualPathRoundsRoot is RectTransform;
        GameObject roundObject = new GameObject($"Round_{round}", useRectTransforms ? typeof(RectTransform) : typeof(Transform));
        Transform roundRoot = roundObject.transform;
        roundRoot.SetParent(manualPathRoundsRoot, false);
        ConfigurePathRootTransform(roundRoot);
        return roundRoot;
    }

    private Transform FindManualPathRoundRoot(int round)
    {
        if (manualPathRoundsRoot == null)
        {
            return null;
        }

        string expectedName = $"Round_{round}";
        Transform byName = manualPathRoundsRoot.Find(expectedName);
        if (byName != null)
        {
            return byName;
        }

        int index = round - 1;
        if (index >= 0 && index < manualPathRoundsRoot.childCount)
        {
            return manualPathRoundsRoot.GetChild(index);
        }

        return null;
    }

    private Transform ResolveManualPathPointsRootForCurrentRound()
    {
        if (usePerRoundManualPathRoots && manualPathRoundsRoot != null)
        {
            Transform byRound = FindManualPathRoundRoot(ronda);
            if (byRound != null)
            {
                return byRound;
            }

            if (manualPathRoundsRoot.childCount > 0)
            {
                int index = Mathf.Clamp(ronda - 1, 0, manualPathRoundsRoot.childCount - 1);
                return manualPathRoundsRoot.GetChild(index);
            }
        }

        return manualPathPointsRoot;
    }

    private bool EnsureManualPathRootExists()
    {
        if (manualPathPointsRoot != null)
        {
            return true;
        }

        if (!TryResolveManualPathParent(out Transform parent, out bool useRectTransforms))
        {
            return false;
        }

        GameObject rootObject = new GameObject("ManualCutPathRoot", useRectTransforms ? typeof(RectTransform) : typeof(Transform));
        manualPathPointsRoot = rootObject.transform;
        manualPathPointsRoot.SetParent(parent, false);
        ConfigurePathRootTransform(manualPathPointsRoot);

        return true;
    }

    private void ClearManualPathChildren(Transform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            if (child == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void CreateDefaultManualPathPoints(Transform pointsRoot)
    {
        if (pointsRoot == null)
        {
            return;
        }

        int count = Mathf.Clamp(defaultManualPointCount, 4, 64);
        float radiusFactor = Mathf.Clamp(defaultManualRadiusNormalized, 0.1f, 0.9f);

        bool useRectTransforms = pointsRoot is RectTransform;
        float radiusX = 120f;
        float radiusY = 120f;

        if (galletaImage != null)
        {
            Rect cookieRect = galletaImage.rectTransform.rect;
            radiusX = Mathf.Max(20f, Mathf.Abs(cookieRect.width) * radiusFactor);
            radiusY = Mathf.Max(20f, Mathf.Abs(cookieRect.height) * radiusFactor);
        }

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)count;
            float angle = t * Mathf.PI * 2f;
            Vector2 point = new Vector2(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY);
            string pointName = $"P{(i + 1):00}";

            if (useRectTransforms)
            {
                GameObject pointObject = new GameObject(pointName, typeof(RectTransform));
                RectTransform pointRect = pointObject.GetComponent<RectTransform>();
                pointRect.SetParent(pointsRoot, false);
                pointRect.anchorMin = new Vector2(0.5f, 0.5f);
                pointRect.anchorMax = new Vector2(0.5f, 0.5f);
                pointRect.pivot = new Vector2(0.5f, 0.5f);
                pointRect.sizeDelta = new Vector2(12f, 12f);
                pointRect.anchoredPosition = point;
            }
            else
            {
                GameObject pointObject = new GameObject(pointName, typeof(Transform));
                Transform pointTransform = pointObject.transform;
                pointTransform.SetParent(pointsRoot, false);
                pointTransform.localPosition = new Vector3(point.x, point.y, 0f);
            }
        }
    }

    private void StartRound()
    {
        StopFeedbackCoroutines();
        pointerHeld = false;
        hasLastPixel = false;
        textureDirty = false;
        tintOverrideActive = false;
        nextDamageFlashTime = 0f;
        crackStage = 0;

        if (galletaImage == null)
        {
            Debug.LogError("DalgonaGameManager: galletaImage no esta asignada.");
            CompleteMatch(false, "Config invalida");
            return;
        }

        cookieRect = galletaImage.rectTransform;
        cookieBasePosition = cookieRect.anchoredPosition;

        sourceTexture = ResolveTextureForCurrentRound();
        if (sourceTexture == null)
        {
            Debug.LogError("DalgonaGameManager: no hay textura valida para esta ronda.");
            CompleteMatch(false, "Sin textura");
            return;
        }

        if (!BuildRuntimeTexture(sourceTexture))
        {
            Debug.LogError("DalgonaGameManager: no se pudo preparar textura para recorte.");
            CompleteMatch(false, "Error de textura");
            return;
        }

        EnsureManualPathSetup(false);
        BuildTargetMask();
        if (targetPixelCount <= 0)
        {
            Debug.LogError("DalgonaGameManager: no se detecto trazado objetivo en la textura.");
            CompleteMatch(false, "Mascara invalida");
            return;
        }

        timeLimit = Mathf.Max(minimumTimeLimit, baseTimeLimit - (ronda - 1) * timeLimitReductionPerRound);
        requiredCoverage = Mathf.Clamp(baseRequiredCoverage + (ronda - 1) * coverageIncreasePerRound, 0.3f, maximumRequiredCoverage);
        if (manualMaskActive)
        {
            requiredCoverage = Mathf.Max(requiredCoverage, strictManualRequiredCoverage);
        }
        maxIntegrity = Mathf.Max(minimumIntegrity, baseIntegrity - (ronda - 1) * integrityReductionPerRound);
        allowedMistakes = Mathf.Max(minimumAllowedMistakes, baseAllowedMistakes - ((ronda - 1) * allowedMistakeReductionPerRound));
        mistakesMade = 0;
        nextMistakeTime = 0f;

        timeLeft = timeLimit;
        integrity = maxIntegrity;
        pressure = 0f;

        if (ShouldUseRuntimeHud())
        {
            EnsureHudBuilt();
            if (hudRoot != null)
            {
                hudRoot.gameObject.SetActive(true);
            }

            UpdateHud(true);
        }
        else if (hudRoot != null)
        {
            hudRoot.gameObject.SetActive(false);
        }

        EnsureScissorCursor();
        SetScissorCursorVisible(showScissorCursor);
        hasScissorLastScreenPosition = false;

        if (statusText != null)
        {
            statusText.gameObject.SetActive(useStatusText);
        }

        UpdateStatus(BuildIntroStatusMessage());
        RefreshCookieTint();

        state = MatchState.Intro;
        introEndTime = Time.time + Mathf.Max(0.05f, introLockDuration);
    }

    private Texture2D ResolveTextureForCurrentRound()
    {
        return GetTextureForRound(ronda);
    }

    private bool BuildRuntimeTexture(Texture2D texture)
    {
        CleanupRuntimeTexture();
        transientReadableCopy = null;

        Texture2D workingSource = texture;
        Color[] source = null;

        try
        {
            source = workingSource.GetPixels();
        }
        catch
        {
            workingSource = CopyReadableTexture(texture);
            transientReadableCopy = workingSource;
            if (workingSource != null)
            {
                source = workingSource.GetPixels();
            }
        }

        if (source == null || source.Length == 0)
        {
            return false;
        }

        textureWidth = workingSource.width;
        textureHeight = workingSource.height;
        sourcePixels = source;
        runtimePixels = new Color[sourcePixels.Length];
        System.Array.Copy(sourcePixels, runtimePixels, sourcePixels.Length);

        runtimeTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        runtimeTexture.filterMode = texture.filterMode;
        runtimeTexture.wrapMode = TextureWrapMode.Clamp;
        runtimeTexture.SetPixels(runtimePixels);
        runtimeTexture.Apply(false);

        galletaImage.texture = runtimeTexture;
        nextTextureApplyTime = Time.unscaledTime + textureApplyInterval;
        textureDirty = false;
        return true;
    }

    private Texture2D CopyReadableTexture(Texture2D source)
    {
        if (source == null)
        {
            return null;
        }

        RenderTexture tmp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;
        Graphics.Blit(source, tmp);
        RenderTexture.active = tmp;

        Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        copy.Apply(false);

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        return copy;
    }

    private void CleanupRuntimeTexture()
    {
        if (runtimeTexture != null)
        {
            Destroy(runtimeTexture);
            runtimeTexture = null;
        }

        if (transientReadableCopy != null)
        {
            Destroy(transientReadableCopy);
            transientReadableCopy = null;
        }

        sourcePixels = null;
        runtimePixels = null;
        targetMask = null;
        visitedMask = null;
        textureWidth = 0;
        textureHeight = 0;
        targetPixelCount = 0;
        visitedPixelCount = 0;
        manualMaskActive = false;
        manualCheckpointPixels = null;
        manualCheckpointVisited = null;
        manualCheckpointVisitedCount = 0;
        allowedMistakes = 0;
        mistakesMade = 0;
        nextMistakeTime = 0f;
    }

    private void BuildTargetMask()
    {
        targetMask = new bool[sourcePixels.Length];
        visitedMask = new bool[sourcePixels.Length];
        manualMaskActive = false;
        ClearManualCheckpointProgress();

        if (TryBuildManualTargetMask(out bool[] manualMask, out int manualCount))
        {
            targetMask = manualMask;
            targetPixelCount = manualCount;
            visitedPixelCount = 0;
            manualMaskActive = true;
            return;
        }

        bool[] colorMask = BuildColorMask(out int colorMaskCount);
        bool useFallback = fallbackToDarkLineDetection && colorMaskCount < minimumTargetPixelsToAcceptColorMask;
        bool[] rawMask = useFallback ? BuildDarkLineMask() : colorMask;
        bool[] filteredMask = BuildFilteredMask(rawMask, out int filteredCount);

        if (filteredCount <= 0)
        {
            filteredMask = rawMask;
            filteredCount = CountMaskTrue(filteredMask);
        }

        targetMask = filteredMask;
        targetPixelCount = filteredCount;
        visitedPixelCount = 0;
        manualMaskActive = false;
        ClearManualCheckpointProgress();
    }

    private bool TryBuildManualTargetMask(out bool[] mask, out int count)
    {
        mask = null;
        count = 0;

        Transform pointsRoot = ResolveManualPathPointsRootForCurrentRound();
        if (!useManualCutPath || pointsRoot == null || galletaImage == null)
        {
            return false;
        }

        List<Vector2Int> points = new List<Vector2Int>(16);
        foreach (Transform child in pointsRoot)
        {
            if (!child.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (TryWorldToTexturePixel(child.position, out Vector2Int pixel))
            {
                points.Add(pixel);
            }
        }

        if (points.Count < 2)
        {
            ClearManualCheckpointProgress();
            return false;
        }

        mask = new bool[sourcePixels.Length];
        float pxPerUi = GetPixelsPerUiAverage();
        int radius = Mathf.Max(1, Mathf.RoundToInt((manualPathWidthUi * pxPerUi) * 0.5f));
        float sampleStepPx = Mathf.Max(1f, manualPathSampleSpacingUi * pxPerUi);

        for (int i = 0; i < points.Count - 1; i++)
        {
            RasterizeManualSegment(mask, points[i], points[i + 1], radius, sampleStepPx);
        }

        if (manualPathClosed)
        {
            RasterizeManualSegment(mask, points[points.Count - 1], points[0], radius, sampleStepPx);
        }

        SetupManualCheckpointProgress(points);
        count = CountMaskTrue(mask);
        return count > 0;
    }

    private void RasterizeManualSegment(bool[] mask, Vector2Int from, Vector2Int to, int radius, float stepPx)
    {
        float distance = Vector2.Distance(from, to);
        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / Mathf.Max(0.5f, stepPx)));

        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0f : i / (float)steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));
            PaintMaskDisk(mask, x, y, radius);
        }
    }

    private void PaintMaskDisk(bool[] mask, int centerX, int centerY, int radius)
    {
        int r = Mathf.Max(1, radius);
        int sqr = r * r;

        for (int oy = -r; oy <= r; oy++)
        {
            int y = centerY + oy;
            if (y < 0 || y >= textureHeight)
            {
                continue;
            }

            for (int ox = -r; ox <= r; ox++)
            {
                if (ox * ox + oy * oy > sqr)
                {
                    continue;
                }

                int x = centerX + ox;
                if (x < 0 || x >= textureWidth)
                {
                    continue;
                }

                int index = y * textureWidth + x;
                mask[index] = true;
            }
        }
    }

    private bool[] BuildColorMask(out int count)
    {
        bool[] mask = new bool[sourcePixels.Length];
        count = 0;

        Color target = colorFigura;
        float toleranceSqr = tolerancia * tolerancia;

        for (int i = 0; i < sourcePixels.Length; i++)
        {
            Color px = sourcePixels[i];
            if (px.a < minAlphaForPath)
            {
                continue;
            }

            float dr = px.r - target.r;
            float dg = px.g - target.g;
            float db = px.b - target.b;
            float distSqr = dr * dr + dg * dg + db * db;

            if (distSqr <= toleranceSqr)
            {
                mask[i] = true;
                count++;
            }
        }

        return mask;
    }

    private bool[] BuildDarkLineMask()
    {
        bool[] mask = new bool[sourcePixels.Length];

        for (int i = 0; i < sourcePixels.Length; i++)
        {
            Color px = sourcePixels[i];
            if (px.a < minAlphaForPath)
            {
                continue;
            }

            float maxC = Mathf.Max(px.r, Mathf.Max(px.g, px.b));
            float minC = Mathf.Min(px.r, Mathf.Min(px.g, px.b));
            float saturation = maxC - minC;
            float luminance = (0.2126f * px.r) + (0.7152f * px.g) + (0.0722f * px.b);

            if (luminance <= darkLineMaxLuminance && saturation >= darkLineMinSaturation)
            {
                mask[i] = true;
            }
        }

        return mask;
    }

    private bool[] BuildFilteredMask(bool[] rawMask, out int count)
    {
        bool[] filtered = new bool[rawMask.Length];
        bool[] visited = new bool[rawMask.Length];
        Queue<int> queue = new Queue<int>(512);
        List<int> component = new List<int>(512);
        bool keptAny = false;

        for (int i = 0; i < rawMask.Length; i++)
        {
            if (!rawMask[i] || visited[i])
            {
                continue;
            }

            queue.Clear();
            component.Clear();
            visited[i] = true;
            queue.Enqueue(i);

            bool touchesEdge = false;

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                component.Add(index);

                int x = index % textureWidth;
                int y = index / textureWidth;

                if (x == 0 || y == 0 || x == textureWidth - 1 || y == textureHeight - 1)
                {
                    touchesEdge = true;
                }

                for (int oy = -1; oy <= 1; oy++)
                {
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        if (ox == 0 && oy == 0)
                        {
                            continue;
                        }

                        int nx = x + ox;
                        int ny = y + oy;
                        if (nx < 0 || ny < 0 || nx >= textureWidth || ny >= textureHeight)
                        {
                            continue;
                        }

                        int nIndex = ny * textureWidth + nx;
                        if (visited[nIndex] || !rawMask[nIndex])
                        {
                            continue;
                        }

                        visited[nIndex] = true;
                        queue.Enqueue(nIndex);
                    }
                }
            }

            bool keep = component.Count >= Mathf.Max(1, minComponentPixels);
            if (keep && ignoreEdgeConnectedPath && touchesEdge)
            {
                keep = false;
            }

            if (!keep)
            {
                continue;
            }

            keptAny = true;
            for (int c = 0; c < component.Count; c++)
            {
                filtered[component[c]] = true;
            }
        }

        if (!keptAny)
        {
            System.Array.Copy(rawMask, filtered, rawMask.Length);
        }

        count = CountMaskTrue(filtered);
        return filtered;
    }

    private int CountMaskTrue(bool[] mask)
    {
        int count = 0;
        for (int i = 0; i < mask.Length; i++)
        {
            if (mask[i])
            {
                count++;
            }
        }

        return count;
    }

    private void ClearManualCheckpointProgress()
    {
        manualCheckpointPixels = null;
        manualCheckpointVisited = null;
        manualCheckpointVisitedCount = 0;
    }

    private void SetupManualCheckpointProgress(List<Vector2Int> points)
    {
        if (!requireManualCheckpoints || points == null || points.Count == 0)
        {
            ClearManualCheckpointProgress();
            return;
        }

        manualCheckpointPixels = points.ToArray();
        manualCheckpointVisited = new bool[manualCheckpointPixels.Length];
        manualCheckpointVisitedCount = 0;
    }

    private void MarkManualCheckpointHit(int x, int y)
    {
        if (!manualMaskActive || !requireManualCheckpoints || manualCheckpointPixels == null || manualCheckpointVisited == null)
        {
            return;
        }

        int radius = Mathf.Max(1, manualCheckpointHitRadius + GetEffectiveValidBrushRadius());
        int sqr = radius * radius;

        for (int i = 0; i < manualCheckpointPixels.Length; i++)
        {
            if (manualCheckpointVisited[i])
            {
                continue;
            }

            Vector2Int p = manualCheckpointPixels[i];
            int dx = p.x - x;
            int dy = p.y - y;
            if ((dx * dx) + (dy * dy) > sqr)
            {
                continue;
            }

            manualCheckpointVisited[i] = true;
            manualCheckpointVisitedCount++;
        }
    }

    private float GetManualCheckpointRatio()
    {
        if (manualCheckpointPixels == null || manualCheckpointPixels.Length == 0)
        {
            return 1f;
        }

        return Mathf.Clamp01(manualCheckpointVisitedCount / (float)manualCheckpointPixels.Length);
    }

    private void ReadPointerState(out bool down, out bool held, out bool up, out Vector2 screenPosition)
    {
        down = false;
        held = false;
        up = false;
        screenPosition = Input.mousePosition;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            screenPosition = touch.position;
            down = touch.phase == TouchPhase.Began;
            held = touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            up = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            return;
        }

        down = Input.GetMouseButtonDown(0);
        held = Input.GetMouseButton(0);
        up = Input.GetMouseButtonUp(0);
    }

    private bool TraceFromPointer(
        Vector2 screenPosition,
        float dt,
        out bool touchedOffPath,
        out float pointerSpeed,
        out bool pointerInsideCookie)
    {
        touchedOffPath = false;
        pointerSpeed = 0f;
        pointerInsideCookie = false;

        if (!TryGetTexturePixel(screenPosition, out Vector2Int currentPixel))
        {
            hasLastPixel = false;
            return false;
        }

        pointerInsideCookie = true;

        if (!hasLastPixel)
        {
            lastPixel = currentPixel;
            hasLastPixel = true;

            bool startsOnPath = IsNearTarget(currentPixel.x, currentPixel.y, GetEffectiveDetectionRadius());
            if (startsOnPath)
            {
                PaintValid(currentPixel.x, currentPixel.y);
            }
            else
            {
                PaintInvalid(currentPixel.x, currentPixel.y);
                touchedOffPath = true;
            }

            return startsOnPath;
        }

        float distance = Vector2.Distance(lastPixel, currentPixel);
        pointerSpeed = distance / Mathf.Max(0.0001f, dt);
        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / Mathf.Max(0.1f, minCursorStepPixels)));

        bool touchedPath = false;

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            int px = Mathf.RoundToInt(Mathf.Lerp(lastPixel.x, currentPixel.x, t));
            int py = Mathf.RoundToInt(Mathf.Lerp(lastPixel.y, currentPixel.y, t));

            bool onPath = IsNearTarget(px, py, GetEffectiveDetectionRadius());
            if (onPath)
            {
                touchedPath = true;
                PaintValid(px, py);
            }
            else
            {
                touchedOffPath = true;
                PaintInvalid(px, py);
            }
        }

        lastPixel = currentPixel;
        return touchedPath;
    }

    private bool TryGetTexturePixel(Vector2 screenPosition, out Vector2Int pixel)
    {
        pixel = default;

        if (galletaImage == null || textureWidth <= 0 || textureHeight <= 0)
        {
            return false;
        }

        Camera eventCamera = galletaImage.canvas != null && galletaImage.canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? galletaImage.canvas.worldCamera
            : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(galletaImage.rectTransform, screenPosition, eventCamera, out Vector2 localPos))
        {
            return false;
        }

        return TryLocalToTexturePixel(localPos, out pixel);
    }

    private bool TryWorldToTexturePixel(Vector3 worldPosition, out Vector2Int pixel)
    {
        pixel = default;

        if (galletaImage == null || textureWidth <= 0 || textureHeight <= 0)
        {
            return false;
        }

        Vector2 localPos = galletaImage.rectTransform.InverseTransformPoint(worldPosition);
        return TryLocalToTexturePixel(localPos, out pixel);
    }

    private bool TryLocalToTexturePixel(Vector2 localPos, out Vector2Int pixel)
    {
        pixel = default;

        if (galletaImage == null || textureWidth <= 0 || textureHeight <= 0)
        {
            return false;
        }

        Rect rect = galletaImage.rectTransform.rect;
        if (Mathf.Abs(rect.width) < 0.0001f || Mathf.Abs(rect.height) < 0.0001f)
        {
            return false;
        }

        float u = (localPos.x - rect.x) / rect.width;
        float v = (localPos.y - rect.y) / rect.height;

        if (u < 0f || u > 1f || v < 0f || v > 1f)
        {
            return false;
        }

        int x = Mathf.Clamp(Mathf.RoundToInt(u * (textureWidth - 1)), 0, textureWidth - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(v * (textureHeight - 1)), 0, textureHeight - 1);
        pixel = new Vector2Int(x, y);
        return true;
    }

    private float GetPixelsPerUiAverage()
    {
        if (galletaImage == null || textureWidth <= 0 || textureHeight <= 0)
        {
            return 1f;
        }

        Rect rect = galletaImage.rectTransform.rect;
        float widthUi = Mathf.Max(1f, Mathf.Abs(rect.width));
        float heightUi = Mathf.Max(1f, Mathf.Abs(rect.height));
        float pxPerUiX = textureWidth / widthUi;
        float pxPerUiY = textureHeight / heightUi;
        return Mathf.Max(0.01f, (pxPerUiX + pxPerUiY) * 0.5f);
    }

    private int GetEffectiveDetectionRadius()
    {
        return Mathf.Max(0, detectionRadius + assistRadiusBonus);
    }

    private int GetEffectiveValidBrushRadius()
    {
        return Mathf.Max(0, validBrushRadius + Mathf.CeilToInt(assistRadiusBonus * 0.5f));
    }

    private int GetEffectiveCutAssistRadius()
    {
        return Mathf.Max(1, cutAssistFillRadius + Mathf.CeilToInt(assistRadiusBonus * 0.5f));
    }

    private bool IsNearTarget(int x, int y, int radius)
    {
        if (targetMask == null || targetMask.Length == 0)
        {
            return false;
        }

        int r = Mathf.Max(0, radius);
        int sqr = r * r;

        for (int oy = -r; oy <= r; oy++)
        {
            int py = y + oy;
            if (py < 0 || py >= textureHeight)
            {
                continue;
            }

            for (int ox = -r; ox <= r; ox++)
            {
                if (ox * ox + oy * oy > sqr)
                {
                    continue;
                }

                int px = x + ox;
                if (px < 0 || px >= textureWidth)
                {
                    continue;
                }

                int index = py * textureWidth + px;
                if (targetMask[index])
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void PaintValid(int centerX, int centerY)
    {
        PaintBrush(centerX, centerY, GetEffectiveValidBrushRadius(), tracedPathColor, tracedPaintStrength, true, true, false);
        ApplyCutAssistArea(centerX, centerY);
        MarkManualCheckpointHit(centerX, centerY);
    }

    private void PaintInvalid(int centerX, int centerY)
    {
        PaintBrush(centerX, centerY, invalidBrushRadius, crackPathColor, crackPaintStrength, false, false, true);
    }

    private void PaintBrush(
        int centerX,
        int centerY,
        int radius,
        Color paintColor,
        float paintStrength,
        bool markVisited,
        bool onlyTarget,
        bool skipTarget)
    {
        if (runtimePixels == null || runtimePixels.Length == 0)
        {
            return;
        }

        int r = Mathf.Max(0, radius);
        int sqr = r * r;

        for (int oy = -r; oy <= r; oy++)
        {
            int py = centerY + oy;
            if (py < 0 || py >= textureHeight)
            {
                continue;
            }

            for (int ox = -r; ox <= r; ox++)
            {
                if (ox * ox + oy * oy > sqr)
                {
                    continue;
                }

                int px = centerX + ox;
                if (px < 0 || px >= textureWidth)
                {
                    continue;
                }

                int index = py * textureWidth + px;
                bool isTarget = targetMask[index];

                if (onlyTarget && !isTarget)
                {
                    continue;
                }

                if (skipTarget && isTarget)
                {
                    continue;
                }

                if (markVisited && isTarget && !visitedMask[index])
                {
                    visitedMask[index] = true;
                    visitedPixelCount++;
                }

                runtimePixels[index] = Color.Lerp(runtimePixels[index], paintColor, paintStrength);
                textureDirty = true;
            }
        }
    }

    private void ApplyCutAssistArea(int centerX, int centerY)
    {
        if (runtimePixels == null || runtimePixels.Length == 0 || targetMask == null || visitedMask == null)
        {
            return;
        }

        int radius = GetEffectiveCutAssistRadius();
        int sqr = radius * radius;
        int maxPixels = Mathf.Max(0, cutAssistMaxPixelsPerHit);
        int filled = 0;

        for (int oy = -radius; oy <= radius; oy++)
        {
            int py = centerY + oy;
            if (py < 0 || py >= textureHeight)
            {
                continue;
            }

            for (int ox = -radius; ox <= radius; ox++)
            {
                if (ox * ox + oy * oy > sqr)
                {
                    continue;
                }

                int px = centerX + ox;
                if (px < 0 || px >= textureWidth)
                {
                    continue;
                }

                int index = py * textureWidth + px;
                if (!targetMask[index])
                {
                    continue;
                }

                if (!visitedMask[index])
                {
                    visitedMask[index] = true;
                    visitedPixelCount++;
                }

                runtimePixels[index] = Color.Lerp(runtimePixels[index], tracedPathColor, cutAssistFillStrength);
                textureDirty = true;
                filled++;

                if (maxPixels > 0 && filled >= maxPixels)
                {
                    return;
                }
            }
        }
    }

    private void UpdatePressure(float dt, bool isHolding, bool touchedPath, bool touchedOffPath, float speed)
    {
        if (!isHolding)
        {
            pressure = Mathf.MoveTowards(pressure, 0f, pressureReleasePerSecond * dt);
            return;
        }

        float buildRate = pressureBuildPerSecond;
        if (touchedPath)
        {
            buildRate *= pressureBuildOnPathMultiplier;
        }

        if (touchedOffPath)
        {
            buildRate *= pressureBuildOffPathMultiplier;
        }

        if (speed < stationarySpeedThreshold)
        {
            buildRate += stationaryPressureBonus;
        }

        pressure = Mathf.Clamp01(pressure + buildRate * dt);

        if (pressure > pressureThresholdToDamage)
        {
            float over = (pressure - pressureThresholdToDamage) / Mathf.Max(0.0001f, 1f - pressureThresholdToDamage);
            ApplyDamage(pressureDamagePerSecond * over * dt);
        }
    }

    private void ApplyDamage(float damage)
    {
        if (state != MatchState.Playing || damage <= 0f)
        {
            return;
        }

        float previousIntegrity = integrity;
        integrity = Mathf.Max(0f, integrity - damage);

        if (integrity <= 0f)
        {
            integrity = 0f;
            CompleteMatch(false, "La galleta se rompio");
            return;
        }

        if (integrity < previousIntegrity && Time.time >= nextDamageFlashTime)
        {
            nextDamageFlashTime = Time.time + damageFlashCooldown;
            FlashTint(warningTint);
            StartShake(1f);
        }
    }

    private void UpdateCrackStageFeedback()
    {
        float loss = 1f - GetIntegrityRatio();
        int newStage = Mathf.Clamp(Mathf.FloorToInt(loss * 4f), 0, 4);
        if (newStage <= crackStage)
        {
            return;
        }

        crackStage = newStage;
        FlashTint(Color.Lerp(warningTint, failTint, 0.35f));
        StartShake(1.2f);
    }

    private float GetProgress()
    {
        if (targetPixelCount <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01(visitedPixelCount / (float)targetPixelCount);
    }

    private float GetIntegrityRatio()
    {
        if (maxIntegrity <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(integrity / maxIntegrity);
    }

    private bool HasReachedWinCondition()
    {
        if (GetProgress() < requiredCoverage)
        {
            return false;
        }

        if (!manualMaskActive || !requireManualCheckpoints)
        {
            return true;
        }

        return GetManualCheckpointRatio() >= strictManualCheckpointRatio;
    }

    private void RegisterMistake()
    {
        if (!useMistakeCounter || state != MatchState.Playing)
        {
            return;
        }

        if (Time.time < nextMistakeTime)
        {
            return;
        }

        nextMistakeTime = Time.time + Mathf.Max(0.01f, mistakeCooldown);
        mistakesMade = Mathf.Min(allowedMistakes, mistakesMade + 1);

        if (damagePerMistake > 0f)
        {
            ApplyDamage(damagePerMistake);
            if (state != MatchState.Playing)
            {
                return;
            }
        }

        FlashTint(warningTint);
        StartShake(1.4f);
        UpdateStatus(BuildPlayingStatusMessage());
        UpdateHud();

        if (mistakesMade >= allowedMistakes)
        {
            CompleteMatch(false, "Demasiados errores");
        }
    }

    private bool ShouldUseRuntimeHud()
    {
        return createRuntimeHud || useMistakeCounter;
    }

    private string BuildIntroStatusMessage()
    {
        if (!useMistakeCounter)
        {
            return "Preparate para cortar...";
        }

        return $"Preparate: tienes {allowedMistakes} errores maximos.";
    }

    private string BuildPlayingStatusMessage()
    {
        if (!useMistakeCounter)
        {
            return "Corta el contorno con la tijera.";
        }

        return $"Corta el contorno. Errores {mistakesMade}/{allowedMistakes}.";
    }

    private void RefreshCookieTint()
    {
        if (galletaImage == null || tintOverrideActive || state == MatchState.Won || state == MatchState.Lost)
        {
            return;
        }

        float integrityRisk = 1f - GetIntegrityRatio();
        float pressureRisk = pressure;
        float baseRisk = Mathf.Clamp01((integrityRisk * 0.72f) + (pressureRisk * 0.35f));

        Color tint = Color.Lerp(neutralTint, warningTint, baseRisk);
        float hotPressure = Mathf.InverseLerp(pressureThresholdToDamage, 1f, pressure);
        tint = Color.Lerp(tint, highPressureTint, hotPressure);

        galletaImage.color = tint;
    }

    private void FlashTint(Color tint)
    {
        if (galletaImage == null || state == MatchState.Won || state == MatchState.Lost)
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
        tintOverrideActive = true;
        galletaImage.color = tint;
        yield return new WaitForSeconds(flashDuration);
        tintOverrideActive = false;
        tintRoutine = null;
        RefreshCookieTint();
    }

    private void StartShake(float multiplier)
    {
        if (cookieRect == null || shakeDuration <= 0f || shakeStrength <= 0f)
        {
            return;
        }

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(multiplier));
    }

    private IEnumerator ShakeRoutine(float multiplier)
    {
        float elapsed = 0f;
        float baseStrength = Mathf.Max(0f, shakeStrength * multiplier);

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shakeDuration);
            float strength = Mathf.Lerp(baseStrength, 0f, t);
            cookieRect.anchoredPosition = cookieBasePosition + (Random.insideUnitCircle * strength);
            yield return null;
        }

        cookieRect.anchoredPosition = cookieBasePosition;
        shakeRoutine = null;
    }

    private void ApplyRuntimeTextureIfNeeded()
    {
        if (!textureDirty || runtimeTexture == null || runtimePixels == null)
        {
            return;
        }

        if (Time.unscaledTime < nextTextureApplyTime)
        {
            return;
        }

        runtimeTexture.SetPixels(runtimePixels);
        runtimeTexture.Apply(false);
        textureDirty = false;
        nextTextureApplyTime = Time.unscaledTime + Mathf.Max(0.005f, textureApplyInterval);
    }

    private void ForceApplyRuntimeTexture()
    {
        if (runtimeTexture == null || runtimePixels == null)
        {
            return;
        }

        runtimeTexture.SetPixels(runtimePixels);
        runtimeTexture.Apply(false);
        textureDirty = false;
    }

    private void CompleteMatch(bool success, string message)
    {
        if (state == MatchState.Won || state == MatchState.Lost)
        {
            return;
        }

        state = success ? MatchState.Won : MatchState.Lost;
        pointerHeld = false;
        hasLastPixel = false;
        SetScissorCursorVisible(false);
        UpdateStatus(message);

        if (success)
        {
            ForceApplyRuntimeTexture();
        }
        else
        {
            StartShake(1.8f);
        }

        if (tintRoutine != null)
        {
            StopCoroutine(tintRoutine);
            tintRoutine = null;
        }

        tintOverrideActive = true;
        if (galletaImage != null)
        {
            galletaImage.color = success ? successTint : failTint;
        }

        StartCoroutine(FinishAndReport(success));
    }

    private IEnumerator FinishAndReport(bool success)
    {
        yield return new WaitForSeconds(Mathf.Max(0.05f, finishDelay));
        OnGameFinished?.Invoke(success);
    }

    private void EnsureHudBuilt()
    {
        if (!ShouldUseRuntimeHud() || galletaImage == null)
        {
            return;
        }

        if (hudRoot == null)
        {
            RectTransform parent = galletaImage.rectTransform.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            GameObject root = new GameObject("DalgonaRuntimeHUD", typeof(RectTransform), typeof(Image));
            hudRoot = root.GetComponent<RectTransform>();
            hudRoot.SetParent(parent, false);
        }

        hudRoot.anchorMin = new Vector2(0.5f, 0.5f);
        hudRoot.anchorMax = new Vector2(0.5f, 0.5f);
        hudRoot.pivot = new Vector2(0.5f, 0.5f);
        hudRoot.anchoredPosition = hudAnchoredPosition;
        hudRoot.sizeDelta = hudSize;

        Image rootImage = hudRoot.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = hudBackgroundColor;
            rootImage.raycastTarget = false;
        }

        if (progressFill == null)
        {
            progressFill = CreateBarFill("ProgressBar", barSpacing, progressBarColor);
        }

        if (integrityFill == null)
        {
            integrityFill = CreateBarFill("IntegrityBar", 0f, integrityBarColor);
        }

        if (pressureFill == null)
        {
            pressureFill = CreateBarFill("PressureBar", -barSpacing, pressureBarColor);
        }

        if (hudTopText == null)
        {
            hudTopText = CreateHudLabel("TopText", hudTopTextOffset);
        }

        if (hudBottomText == null)
        {
            hudBottomText = CreateHudLabel("BottomText", hudBottomTextOffset);
        }

        ApplyHudTextStyle();
    }

    private Image CreateBarFill(string barName, float yOffset, Color fillColor)
    {
        if (hudRoot == null)
        {
            return null;
        }

        GameObject bgObject = new GameObject(barName + "_Bg", typeof(RectTransform), typeof(Image));
        RectTransform bgRect = bgObject.GetComponent<RectTransform>();
        bgRect.SetParent(hudRoot, false);
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = new Vector2(0f, yOffset);
        bgRect.sizeDelta = barSize;

        Image bgImage = bgObject.GetComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.42f);
        bgImage.raycastTarget = false;

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.SetParent(bgRect, false);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        fillImage.raycastTarget = false;
        return fillImage;
    }

    private TextMeshProUGUI CreateHudLabel(string name, Vector2 anchoredPosition)
    {
        if (hudRoot == null)
        {
            return null;
        }

        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(hudRoot, false);
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = anchoredPosition;
        textRect.sizeDelta = new Vector2(Mathf.Max(120f, hudSize.x - 18f), 24f);

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = string.Empty;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        return label;
    }

    private void ApplyHudTextStyle()
    {
        if (hudTopText != null)
        {
            hudTopText.fontSize = hudTextSize;
            hudTopText.color = hudTextColor;
        }

        if (hudBottomText != null)
        {
            hudBottomText.fontSize = hudTextSize;
            hudBottomText.color = hudTextColor;
        }
    }

    private void UpdateHud(bool force = false)
    {
        if (!ShouldUseRuntimeHud())
        {
            return;
        }

        EnsureHudBuilt();

        if (progressFill != null)
        {
            progressFill.fillAmount = GetProgress();
            progressFill.color = progressBarColor;
        }

        if (integrityFill != null)
        {
            float ratio = GetIntegrityRatio();
            integrityFill.fillAmount = ratio;
            integrityFill.color = Color.Lerp(failTint, integrityBarColor, ratio);
        }

        if (pressureFill != null)
        {
            pressureFill.fillAmount = Mathf.Clamp01(pressure);
            float danger = Mathf.InverseLerp(pressureThresholdToDamage, 1f, pressure);
            pressureFill.color = Color.Lerp(pressureBarColor, failTint, danger);
        }

        float progressPercent = GetProgress() * 100f;
        int requiredPercent = Mathf.RoundToInt(requiredCoverage * 100f);
        if (hudTopText != null)
        {
            string mistakesInfo = useMistakeCounter
                ? $"Errores: {mistakesMade}/{allowedMistakes}"
                : $"Integridad: {Mathf.RoundToInt(GetIntegrityRatio() * 100f)}%";
            hudTopText.text = $"{mistakesInfo}   Tiempo: {Mathf.CeilToInt(timeLeft)}";
            if (useMistakeCounter && allowedMistakes > 0 && mistakesMade >= allowedMistakes - 1)
            {
                hudTopText.color = failTint;
            }
            else
            {
                hudTopText.color = hudTextColor;
            }
        }

        if (hudBottomText != null)
        {
            string checkpointInfo = string.Empty;
            if (manualMaskActive && requireManualCheckpoints && manualCheckpointPixels != null && manualCheckpointPixels.Length > 0)
            {
                checkpointInfo = $" | Puntos: {manualCheckpointVisitedCount}/{manualCheckpointPixels.Length}";
            }

            hudBottomText.text = $"Progreso: {Mathf.RoundToInt(progressPercent)}%/{requiredPercent}%{checkpointInfo}";
            hudBottomText.color = hudTextColor;
        }

        if (force)
        {
            RefreshCookieTint();
        }
    }

    private void EnsureScissorCursor()
    {
        if (!showScissorCursor || galletaImage == null || scissorCursorRoot != null)
        {
            return;
        }

        Canvas canvas = galletaImage.canvas;
        if (canvas == null)
        {
            return;
        }

        RectTransform parent = canvas.transform as RectTransform;
        if (parent == null)
        {
            return;
        }

        GameObject root = new GameObject("ScissorCursor", typeof(RectTransform));
        scissorCursorRoot = root.GetComponent<RectTransform>();
        scissorCursorRoot.SetParent(parent, false);
        scissorCursorRoot.anchorMin = new Vector2(0.5f, 0.5f);
        scissorCursorRoot.anchorMax = new Vector2(0.5f, 0.5f);
        scissorCursorRoot.pivot = new Vector2(0.5f, 0.5f);
        scissorCursorRoot.sizeDelta = scissorCursorSize;
        scissorCurrentAngle = scissorIdleAngle;
        scissorTargetAngle = scissorIdleAngle;

        scissorBladeTop = CreateScissorPart("BladeTop", scissorBladeSize, scissorBladeColor);
        scissorBladeBottom = CreateScissorPart("BladeBottom", scissorBladeSize, scissorBladeColor);
        scissorHandle = CreateScissorPart("Handle", new Vector2(8f, 8f), scissorHandleColor);
    }

    private RectTransform CreateScissorPart(string partName, Vector2 size, Color color)
    {
        if (scissorCursorRoot == null)
        {
            return null;
        }

        GameObject part = new GameObject(partName, typeof(RectTransform), typeof(RawImage));
        RectTransform rect = part.GetComponent<RectTransform>();
        rect.SetParent(scissorCursorRoot, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.08f, 0.5f);
        rect.sizeDelta = size;

        RawImage image = part.GetComponent<RawImage>();
        image.texture = Texture2D.whiteTexture;
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private void UpdateScissorCursor(Vector2 screenPosition, bool isCutting)
    {
        if (!showScissorCursor)
        {
            SetScissorCursorVisible(false);
            return;
        }

        EnsureScissorCursor();
        if (scissorCursorRoot == null)
        {
            return;
        }

        SetScissorCursorVisible(true);

        Canvas canvas = galletaImage != null ? galletaImage.canvas : null;
        if (canvas == null)
        {
            return;
        }

        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
        {
            return;
        }

        Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, eventCamera, out Vector2 localPos))
        {
            float follow = 1f - Mathf.Exp(-Mathf.Max(1f, scissorSmooth) * Time.deltaTime);
            scissorCursorRoot.anchoredPosition = Vector2.Lerp(scissorCursorRoot.anchoredPosition, localPos, follow);
        }

        if (!hasScissorLastScreenPosition)
        {
            scissorLastScreenPosition = screenPosition;
            hasScissorLastScreenPosition = true;
            scissorCurrentAngle = scissorIdleAngle;
            scissorTargetAngle = scissorIdleAngle;
        }

        Vector2 delta = screenPosition - scissorLastScreenPosition;
        float moveThresholdSqr = Mathf.Max(0f, scissorMovementThreshold) * Mathf.Max(0f, scissorMovementThreshold);
        if (rotateScissorWithMovement && isCutting && delta.sqrMagnitude >= moveThresholdSqr)
        {
            scissorTargetAngle = (Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg) + 20f;
        }
        else
        {
            scissorTargetAngle = scissorIdleAngle;
        }

        float angleFollow = 1f - Mathf.Exp(-Mathf.Max(1f, scissorSmooth) * Time.deltaTime);
        scissorCurrentAngle = Mathf.LerpAngle(scissorCurrentAngle, scissorTargetAngle, angleFollow);

        scissorLastScreenPosition = screenPosition;
        hasScissorLastScreenPosition = true;

        float closeTarget = isCutting ? scissorClosedAngle : scissorOpenAngle;
        float pulse = Mathf.Sin(Time.unscaledTime * scissorPulseSpeed) * (isCutting ? 1.2f : 3f);
        float openAngle = Mathf.Max(1.5f, closeTarget + pulse);

        scissorCursorRoot.localRotation = Quaternion.Euler(0f, 0f, scissorCurrentAngle);
        if (scissorBladeTop != null)
        {
            scissorBladeTop.localRotation = Quaternion.Euler(0f, 0f, openAngle);
        }

        if (scissorBladeBottom != null)
        {
            scissorBladeBottom.localRotation = Quaternion.Euler(0f, 0f, -openAngle);
        }

        if (scissorHandle != null)
        {
            scissorHandle.anchoredPosition = new Vector2(-2f, 0f);
            scissorHandle.localRotation = Quaternion.Euler(0f, 0f, 45f);
        }
    }

    private void SetScissorCursorVisible(bool visible)
    {
        if (scissorCursorRoot == null)
        {
            return;
        }

        scissorCursorRoot.gameObject.SetActive(visible);
    }

    private void UpdateStatus(string message)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.gameObject.SetActive(useStatusText);
        if (!useStatusText)
        {
            return;
        }

        statusText.text = message;
    }

    private void StopFeedbackCoroutines()
    {
        if (tintRoutine != null)
        {
            StopCoroutine(tintRoutine);
            tintRoutine = null;
        }

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }
    }
}
