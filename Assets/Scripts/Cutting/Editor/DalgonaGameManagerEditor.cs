#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DalgonaGameManager))]
public class DalgonaGameManagerEditor : Editor
{
    private const string PrefPrefix = "DalgonaAuthoring_";

    private int authoringRound = 1;
    private bool showAuthoringTools = true;
    private bool showSmartTraceTools = true;

    private bool drawModeEnabled;
    private bool strokeInProgress;
    private readonly List<Vector3> strokeWorldPoints = new List<Vector3>(512);
    private Vector2 lastStrokeGuiPosition;

    private int autoTraceTargetPoints = 18;
    private float autoTraceMinDrawSpacing = 6f;
    private int autoTraceSmoothingPasses = 1;
    private float autoTraceNormalizeSpacing = 5f;
    private float autoTraceCornerBias = 1.9f;

    private bool snapGeneratedPointsToContour = true;
    private int contourSearchRadiusPx = 14;
    private float contourColorTolerance = 0.24f;

    private static GUIStyle pointLabelStyle;
    private static GUIStyle titleLabelStyle;
    private static GUIStyle labelShadowStyle;

    private string PrefKey(string suffix)
    {
        return $"{PrefPrefix}{target.GetInstanceID()}_{suffix}";
    }

    private void OnEnable()
    {
        DalgonaGameManager manager = (DalgonaGameManager)target;
        int roundCount = Mathf.Max(1, manager.GetAuthoringRoundCount());

        authoringRound = Mathf.Clamp(SessionState.GetInt(PrefKey("Round"), 1), 1, roundCount);
        showAuthoringTools = SessionState.GetBool(PrefKey("ShowAuthoring"), true);
        showSmartTraceTools = SessionState.GetBool(PrefKey("ShowSmartTrace"), true);
        autoTraceTargetPoints = Mathf.Clamp(SessionState.GetInt(PrefKey("TracePoints"), 18), 6, 96);
        autoTraceMinDrawSpacing = Mathf.Clamp(SessionState.GetFloat(PrefKey("TraceSpacing"), 6f), 1f, 40f);
        autoTraceSmoothingPasses = Mathf.Clamp(SessionState.GetInt(PrefKey("TraceSmooth"), 1), 0, 4);
        autoTraceNormalizeSpacing = Mathf.Clamp(SessionState.GetFloat(PrefKey("TraceNormalize"), 5f), 1f, 30f);
        autoTraceCornerBias = Mathf.Clamp(SessionState.GetFloat(PrefKey("CornerBias"), 1.9f), 0f, 6f);
        snapGeneratedPointsToContour = SessionState.GetBool(PrefKey("SnapContour"), true);
        contourSearchRadiusPx = Mathf.Clamp(SessionState.GetInt(PrefKey("ContourRadius"), 14), 2, 36);
        contourColorTolerance = Mathf.Clamp(SessionState.GetFloat(PrefKey("ContourTolerance"), 0.24f), 0.05f, 0.8f);
    }

    private void OnDisable()
    {
        SessionState.SetInt(PrefKey("Round"), authoringRound);
        SessionState.SetBool(PrefKey("ShowAuthoring"), showAuthoringTools);
        SessionState.SetBool(PrefKey("ShowSmartTrace"), showSmartTraceTools);
        SessionState.SetInt(PrefKey("TracePoints"), autoTraceTargetPoints);
        SessionState.SetFloat(PrefKey("TraceSpacing"), autoTraceMinDrawSpacing);
        SessionState.SetInt(PrefKey("TraceSmooth"), autoTraceSmoothingPasses);
        SessionState.SetFloat(PrefKey("TraceNormalize"), autoTraceNormalizeSpacing);
        SessionState.SetFloat(PrefKey("CornerBias"), autoTraceCornerBias);
        SessionState.SetBool(PrefKey("SnapContour"), snapGeneratedPointsToContour);
        SessionState.SetInt(PrefKey("ContourRadius"), contourSearchRadiusPx);
        SessionState.SetFloat(PrefKey("ContourTolerance"), contourColorTolerance);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        DalgonaGameManager manager = (DalgonaGameManager)target;
        int roundCount = Mathf.Max(1, manager.GetAuthoringRoundCount());
        authoringRound = Mathf.Clamp(authoringRound, 1, roundCount);

        DrawRoundAuthoringSection(manager, roundCount);
        DrawSmartTraceSection(manager, roundCount);
    }

    private void DrawRoundAuthoringSection(DalgonaGameManager manager, int roundCount)
    {
        EditorGUILayout.Space(10f);
        showAuthoringTools = EditorGUILayout.Foldout(showAuthoringTools, "Round Path Authoring", true);
        if (!showAuthoringTools)
        {
            return;
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Flujo recomendado", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("1) Elige ronda");
        EditorGUILayout.LabelField("2) Preview de figura");
        EditorGUILayout.LabelField("3) Mueve puntos o usa Smart Auto Trace");

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField($"Rondas detectadas: {roundCount}");
        authoringRound = EditorGUILayout.IntSlider("Ronda activa", authoringRound, 1, roundCount);

        EditorGUILayout.BeginHorizontal();
        using (new EditorGUI.DisabledScope(authoringRound <= 1))
        {
            if (GUILayout.Button("Ronda -", GUILayout.Height(23f)))
            {
                authoringRound = Mathf.Max(1, authoringRound - 1);
            }
        }

        using (new EditorGUI.DisabledScope(authoringRound >= roundCount))
        {
            if (GUILayout.Button("Ronda +", GUILayout.Height(23f)))
            {
                authoringRound = Mathf.Min(roundCount, authoringRound + 1);
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Preview Figura En CandyImage", GUILayout.Height(25f)))
        {
            PreviewCurrentRound(manager);
        }

        if (GUILayout.Button("Crear/Obtener Root De Esta Ronda", GUILayout.Height(25f)))
        {
            Transform roundRoot = manager.GetOrCreateManualPathRootForRound(authoringRound, false);
            if (roundRoot != null)
            {
                Selection.activeTransform = roundRoot;
                EditorGUIUtility.PingObject(roundRoot.gameObject);
            }

            EditorUtility.SetDirty(manager);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Recrear Puntos Placeholder De Esta Ronda", GUILayout.Height(25f)))
        {
            Transform roundRoot = manager.GetOrCreateManualPathRootForRound(authoringRound, true);
            if (roundRoot != null)
            {
                Selection.activeTransform = roundRoot;
                EditorGUIUtility.PingObject(roundRoot.gameObject);
            }

            EditorUtility.SetDirty(manager);
            SceneView.RepaintAll();
        }

        Transform activeRoot = manager.GetManualPathRootForRound(authoringRound);
        EditorGUILayout.ObjectField("Root ronda activa", activeRoot, typeof(Transform), true);
        EditorGUILayout.HelpBox("En Scene view se dibuja la guia del trazo para esta ronda.", MessageType.Info);
        EditorGUILayout.EndVertical();
    }

    private void DrawSmartTraceSection(DalgonaGameManager manager, int roundCount)
    {
        EditorGUILayout.Space(8f);
        showSmartTraceTools = EditorGUILayout.Foldout(showSmartTraceTools, "Smart Auto Trace (IA Local)", true);
        if (!showSmartTraceTools)
        {
            return;
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Dibuja encima de la figura y el sistema", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("redistribuye puntos en zonas de curvas para mejorar jugabilidad.");

        authoringRound = EditorGUILayout.IntSlider("Ronda a trazar", authoringRound, 1, Mathf.Max(1, roundCount));
        autoTraceTargetPoints = EditorGUILayout.IntSlider("Cantidad de puntos", autoTraceTargetPoints, 8, 64);
        autoTraceMinDrawSpacing = EditorGUILayout.Slider("Espaciado al dibujar (px)", autoTraceMinDrawSpacing, 1f, 24f);
        autoTraceNormalizeSpacing = EditorGUILayout.Slider("Normalizar trazo (UI)", autoTraceNormalizeSpacing, 1f, 20f);
        autoTraceSmoothingPasses = EditorGUILayout.IntSlider("Suavizado", autoTraceSmoothingPasses, 0, 4);
        autoTraceCornerBias = EditorGUILayout.Slider("Priorizar esquinas", autoTraceCornerBias, 0f, 6f);

        EditorGUILayout.Space(4f);
        snapGeneratedPointsToContour = EditorGUILayout.ToggleLeft("Ajustar puntos al contorno de la textura", snapGeneratedPointsToContour);
        using (new EditorGUI.DisabledScope(!snapGeneratedPointsToContour))
        {
            contourSearchRadiusPx = EditorGUILayout.IntSlider("Radio busqueda contorno (px)", contourSearchRadiusPx, 2, 30);
            contourColorTolerance = EditorGUILayout.Slider("Tolerancia de color contorno", contourColorTolerance, 0.05f, 0.8f);
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.BeginHorizontal();
        if (!drawModeEnabled)
        {
            if (GUILayout.Button("Iniciar Dibujo En Scene", GUILayout.Height(30f)))
            {
                StartDrawMode(manager);
            }
        }
        else
        {
            if (GUILayout.Button("Salir De Modo Dibujo", GUILayout.Height(30f)))
            {
                StopDrawMode();
            }
        }

        using (new EditorGUI.DisabledScope(strokeWorldPoints.Count == 0))
        {
            if (GUILayout.Button("Limpiar Trazo", GUILayout.Height(30f)))
            {
                strokeWorldPoints.Clear();
                strokeInProgress = false;
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.EndHorizontal();

        using (new EditorGUI.DisabledScope(strokeWorldPoints.Count < 3))
        {
            if (GUILayout.Button("Generar Puntos Inteligentes Desde Trazo", GUILayout.Height(32f)))
            {
                GenerateSmartPointsFromStroke(manager);
            }
        }

        EditorGUILayout.HelpBox(
            "Modo dibujo: Click y arrastra en Scene sobre la figura. ESC sale del modo dibujo.\n" +
            $"Trazo actual: {strokeWorldPoints.Count} muestras.",
            MessageType.None);
        EditorGUILayout.EndVertical();
    }

    private void StartDrawMode(DalgonaGameManager manager)
    {
        drawModeEnabled = true;
        strokeInProgress = false;
        strokeWorldPoints.Clear();

        PreviewCurrentRound(manager);
        manager.GetOrCreateManualPathRootForRound(authoringRound, false);
        Selection.activeObject = manager.gameObject;
        SceneView.RepaintAll();
    }

    private void StopDrawMode()
    {
        drawModeEnabled = false;
        strokeInProgress = false;
        SceneView.RepaintAll();
    }

    private void PreviewCurrentRound(DalgonaGameManager manager)
    {
        if (manager.galletaImage != null)
        {
            Undo.RecordObject(manager.galletaImage, "Preview Dalgona Round");
        }

        manager.PreviewRoundTextureInEditor(authoringRound);
        EditorUtility.SetDirty(manager);
        if (manager.galletaImage != null)
        {
            EditorUtility.SetDirty(manager.galletaImage);
        }
    }

    private void OnSceneGUI()
    {
        DalgonaGameManager manager = (DalgonaGameManager)target;
        if (manager == null || !manager.useManualCutPath)
        {
            return;
        }

        int roundCount = Mathf.Max(1, manager.GetAuthoringRoundCount());
        authoringRound = Mathf.Clamp(authoringRound, 1, roundCount);

        DrawCurrentRoundPath(manager);
        DrawCapturedStrokeOverlay();
        HandleDrawInput(manager);
    }

    private void DrawCurrentRoundPath(DalgonaGameManager manager)
    {
        Transform root = manager.GetManualPathRootForRound(authoringRound);
        if (root == null)
        {
            return;
        }

        List<Vector3> points = GatherActivePoints(root);
        if (points.Count < 2)
        {
            return;
        }

        Color previousColor = Handles.color;
        Handles.color = new Color(0.16f, 0.86f, 1f, 0.95f);

        for (int i = 0; i < points.Count - 1; i++)
        {
            Handles.DrawAAPolyLine(3f, points[i], points[i + 1]);
        }

        if (manager.manualPathClosed)
        {
            Handles.DrawAAPolyLine(3f, points[points.Count - 1], points[0]);
        }

        GUIStyle pointStyle = GetPointLabelStyle();
        GUIStyle titleStyle = GetTitleLabelStyle();
        GUIStyle shadowStyle = GetLabelShadowStyle();

        Handles.BeginGUI();
        for (int i = 0; i < points.Count; i++)
        {
            float size = HandleUtility.GetHandleSize(points[i]) * 0.06f;
            Handles.SphereHandleCap(0, points[i], Quaternion.identity, size, EventType.Repaint);

            Vector3 worldLabelPos = points[i] + Vector3.up * (size * 1.2f);
            Vector2 guiLabelPos = HandleUtility.WorldToGUIPoint(worldLabelPos);
            DrawReadableSceneLabel(guiLabelPos, $"P{i + 1}", pointStyle, shadowStyle, true);
        }

        Vector3 titlePos = points[0] + Vector3.right * (HandleUtility.GetHandleSize(points[0]) * 0.18f);
        Vector2 guiTitlePos = HandleUtility.WorldToGUIPoint(titlePos);
        DrawReadableSceneLabel(guiTitlePos, $"Ronda {authoringRound} | Ancho UI: {manager.manualPathWidthUi:0.#}", titleStyle, shadowStyle, false);
        Handles.EndGUI();

        Handles.color = previousColor;
    }

    private void DrawCapturedStrokeOverlay()
    {
        if (strokeWorldPoints.Count <= 0)
        {
            return;
        }

        Color prev = Handles.color;
        Handles.color = new Color(1f, 0.56f, 0.12f, 0.95f);

        if (strokeWorldPoints.Count >= 2)
        {
            Handles.DrawAAPolyLine(4f, strokeWorldPoints.ToArray());
        }

        for (int i = 0; i < strokeWorldPoints.Count; i += Mathf.Max(1, strokeWorldPoints.Count / 40))
        {
            float size = HandleUtility.GetHandleSize(strokeWorldPoints[i]) * 0.03f;
            Handles.DotHandleCap(0, strokeWorldPoints[i], Quaternion.identity, size, EventType.Repaint);
        }

        if (drawModeEnabled && strokeWorldPoints.Count > 0)
        {
            Handles.BeginGUI();
            GUIStyle shadow = GetLabelShadowStyle();
            GUIStyle title = GetTitleLabelStyle();
            Vector2 pos = HandleUtility.WorldToGUIPoint(strokeWorldPoints[strokeWorldPoints.Count - 1]);
            DrawReadableSceneLabel(pos + new Vector2(12f, -12f), "TRAZO IA", title, shadow, false);
            Handles.EndGUI();
        }

        Handles.color = prev;
    }

    private void HandleDrawInput(DalgonaGameManager manager)
    {
        if (!drawModeEnabled)
        {
            return;
        }

        Event e = Event.current;
        if (e == null)
        {
            return;
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlId);

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            StopDrawMode();
            e.Use();
            return;
        }

        if (e.alt)
        {
            return;
        }

        if (e.button == 0 && e.type == EventType.MouseDown)
        {
            strokeInProgress = true;
            strokeWorldPoints.Clear();
            if (TryGetWorldPointOnCookieRect(manager, e.mousePosition, out Vector3 worldPoint))
            {
                AppendStrokePoint(worldPoint, e.mousePosition, true);
            }

            e.Use();
            return;
        }

        if (e.button == 0 && e.type == EventType.MouseDrag && strokeInProgress)
        {
            if (TryGetWorldPointOnCookieRect(manager, e.mousePosition, out Vector3 worldPoint))
            {
                AppendStrokePoint(worldPoint, e.mousePosition, false);
            }

            e.Use();
            SceneView.RepaintAll();
            return;
        }

        if (e.button == 0 && (e.type == EventType.MouseUp || e.rawType == EventType.MouseUp) && strokeInProgress)
        {
            strokeInProgress = false;
            e.Use();
            Repaint();
        }
    }

    private void AppendStrokePoint(Vector3 worldPoint, Vector2 guiMousePosition, bool force)
    {
        if (!force && strokeWorldPoints.Count > 0)
        {
            float guiDist = Vector2.Distance(guiMousePosition, lastStrokeGuiPosition);
            if (guiDist < autoTraceMinDrawSpacing)
            {
                return;
            }

            if ((strokeWorldPoints[strokeWorldPoints.Count - 1] - worldPoint).sqrMagnitude < 0.000001f)
            {
                return;
            }
        }

        strokeWorldPoints.Add(worldPoint);
        lastStrokeGuiPosition = guiMousePosition;
    }

    private bool TryGetWorldPointOnCookieRect(DalgonaGameManager manager, Vector2 guiMousePosition, out Vector3 worldPoint)
    {
        worldPoint = default;

        if (manager == null || manager.galletaImage == null)
        {
            return false;
        }

        RectTransform rectTransform = manager.galletaImage.rectTransform;
        if (rectTransform == null)
        {
            return false;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(guiMousePosition);
        Plane plane = new Plane(rectTransform.forward, rectTransform.position);
        if (!plane.Raycast(ray, out float distance))
        {
            return false;
        }

        Vector3 hit = ray.GetPoint(distance);
        Vector3 local = rectTransform.InverseTransformPoint(hit);
        Rect rect = rectTransform.rect;

        local.x = Mathf.Clamp(local.x, rect.xMin, rect.xMax);
        local.y = Mathf.Clamp(local.y, rect.yMin, rect.yMax);
        local.z = 0f;

        worldPoint = rectTransform.TransformPoint(local);
        return true;
    }

    private void GenerateSmartPointsFromStroke(DalgonaGameManager manager)
    {
        if (strokeWorldPoints.Count < 3)
        {
            return;
        }

        Transform root = manager.GetOrCreateManualPathRootForRound(authoringRound, false);
        if (root == null)
        {
            return;
        }

        List<Vector3> normalized = NormalizeStroke(strokeWorldPoints, autoTraceNormalizeSpacing, manager.manualPathClosed);
        if (normalized.Count < 3)
        {
            return;
        }

        for (int i = 0; i < autoTraceSmoothingPasses; i++)
        {
            normalized = SmoothStroke(normalized, manager.manualPathClosed);
        }

        List<Vector3> strategicPoints = SampleStrategicPoints(
            normalized,
            Mathf.Clamp(autoTraceTargetPoints, 8, 64),
            manager.manualPathClosed,
            autoTraceCornerBias);

        if (snapGeneratedPointsToContour)
        {
            strategicPoints = SnapPointsToContour(
                manager,
                strategicPoints,
                authoringRound,
                contourSearchRadiusPx,
                contourColorTolerance);
        }

        RebuildRoundPoints(root, strategicPoints);

        manager.PreviewRoundTextureInEditor(authoringRound);
        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(manager);
        Selection.activeTransform = root;
        SceneView.RepaintAll();
    }

    private static List<Vector3> NormalizeStroke(List<Vector3> source, float spacing, bool closed)
    {
        List<Vector3> points = new List<Vector3>(source.Count);
        float minDist = Mathf.Max(0.0001f, spacing);
        float minDistSqr = minDist * minDist;

        for (int i = 0; i < source.Count; i++)
        {
            Vector3 p = source[i];
            if (points.Count == 0 || (p - points[points.Count - 1]).sqrMagnitude >= minDistSqr)
            {
                points.Add(p);
            }
        }

        if (closed && points.Count >= 3)
        {
            if ((points[0] - points[points.Count - 1]).sqrMagnitude < minDistSqr)
            {
                points.RemoveAt(points.Count - 1);
            }
        }

        return points;
    }

    private static List<Vector3> SmoothStroke(List<Vector3> points, bool closed)
    {
        if (points.Count < 3)
        {
            return new List<Vector3>(points);
        }

        List<Vector3> output = new List<Vector3>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            if (!closed && (i == 0 || i == points.Count - 1))
            {
                output.Add(points[i]);
                continue;
            }

            int prev = (i - 1 + points.Count) % points.Count;
            int next = (i + 1) % points.Count;
            output.Add((points[prev] + (points[i] * 2f) + points[next]) * 0.25f);
        }

        return output;
    }

    private static List<Vector3> SampleStrategicPoints(List<Vector3> points, int targetCount, bool closed, float cornerBias)
    {
        if (points == null || points.Count == 0)
        {
            return new List<Vector3>();
        }

        if (!closed && points.Count == 1)
        {
            return new List<Vector3>(points);
        }

        targetCount = Mathf.Clamp(targetCount, closed ? 4 : 2, 96);
        List<float> curvature = ComputeCurvatureWeights(points, closed, cornerBias);

        int segmentCount = closed ? points.Count : points.Count - 1;
        if (segmentCount <= 0)
        {
            return new List<Vector3>(points);
        }

        float[] cumulative = new float[segmentCount];
        float total = 0f;
        for (int i = 0; i < segmentCount; i++)
        {
            int next = (i + 1) % points.Count;
            float len = Vector3.Distance(points[i], points[next]);
            float weight = (curvature[i] + curvature[next]) * 0.5f;
            float weightedLen = Mathf.Max(0.0001f, len * weight);
            total += weightedLen;
            cumulative[i] = total;
        }

        if (total <= 0.0001f)
        {
            return new List<Vector3>(points);
        }

        List<Vector3> sampled = new List<Vector3>(targetCount);
        int sampleCount = closed ? targetCount : Mathf.Max(2, targetCount);
        for (int s = 0; s < sampleCount; s++)
        {
            float t = closed
                ? s / (float)sampleCount
                : s / (float)(sampleCount - 1);
            float target = t * total;

            int seg = 0;
            while (seg < cumulative.Length - 1 && cumulative[seg] < target)
            {
                seg++;
            }

            float segStart = seg == 0 ? 0f : cumulative[seg - 1];
            float segLen = Mathf.Max(0.0001f, cumulative[seg] - segStart);
            float segT = Mathf.Clamp01((target - segStart) / segLen);

            int next = (seg + 1) % points.Count;
            sampled.Add(Vector3.Lerp(points[seg], points[next], segT));
        }

        return sampled;
    }

    private static List<float> ComputeCurvatureWeights(List<Vector3> points, bool closed, float cornerBias)
    {
        List<float> weights = new List<float>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            if (!closed && (i == 0 || i == points.Count - 1))
            {
                weights.Add(1f);
                continue;
            }

            int prev = (i - 1 + points.Count) % points.Count;
            int next = (i + 1) % points.Count;
            Vector3 a = (points[i] - points[prev]).normalized;
            Vector3 b = (points[next] - points[i]).normalized;
            float angle01 = Vector3.Angle(a, b) / 180f;
            float cornerScore = 1f + (Mathf.Clamp(cornerBias, 0f, 6f) * angle01 * angle01);
            weights.Add(Mathf.Max(0.5f, cornerScore));
        }

        return weights;
    }

    private static List<Vector3> SnapPointsToContour(
        DalgonaGameManager manager,
        List<Vector3> points,
        int round,
        int searchRadiusPx,
        float colorTolerance)
    {
        Texture2D texture = manager.GetTextureForRound(round);
        if (texture == null || points == null || points.Count == 0)
        {
            return points;
        }

        Color32[] pixels;
        try
        {
            pixels = texture.GetPixels32();
        }
        catch
        {
            return points;
        }

        int width = texture.width;
        int height = texture.height;
        if (width <= 0 || height <= 0 || pixels == null || pixels.Length != width * height)
        {
            return points;
        }

        Color target = manager.colorFigura;
        int radius = Mathf.Clamp(searchRadiusPx, 2, 36);
        float tolerance = Mathf.Clamp(colorTolerance, 0.01f, 1f);

        List<Vector3> snapped = new List<Vector3>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            if (!manager.TryEditorWorldToTexturePixel(points[i], round, out Vector2Int center))
            {
                snapped.Add(points[i]);
                continue;
            }

            bool found = false;
            Vector2Int best = center;
            float bestScore = float.MaxValue;

            for (int oy = -radius; oy <= radius; oy++)
            {
                int py = center.y + oy;
                if (py < 0 || py >= height)
                {
                    continue;
                }

                for (int ox = -radius; ox <= radius; ox++)
                {
                    int px = center.x + ox;
                    if (px < 0 || px >= width)
                    {
                        continue;
                    }

                    int idx = py * width + px;
                    Color32 c = pixels[idx];
                    if (c.a < 20)
                    {
                        continue;
                    }

                    float distColor = ColorDistanceRgb(c, target);
                    if (distColor > tolerance)
                    {
                        continue;
                    }

                    float distPx = Mathf.Sqrt((ox * ox) + (oy * oy)) / Mathf.Max(1f, radius);
                    float score = (distColor * 1.35f) + (distPx * 0.65f);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = new Vector2Int(px, py);
                        found = true;
                    }
                }
            }

            if (found && manager.TryEditorTexturePixelToWorld(best, round, out Vector3 world))
            {
                snapped.Add(world);
            }
            else
            {
                snapped.Add(points[i]);
            }
        }

        return snapped;
    }

    private static float ColorDistanceRgb(Color32 a, Color b)
    {
        float ar = a.r / 255f;
        float ag = a.g / 255f;
        float ab = a.b / 255f;
        float dr = ar - b.r;
        float dg = ag - b.g;
        float db = ab - b.b;
        return Mathf.Sqrt((dr * dr) + (dg * dg) + (db * db));
    }

    private static void RebuildRoundPoints(Transform root, List<Vector3> worldPoints)
    {
        if (root == null || worldPoints == null || worldPoints.Count == 0)
        {
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Smart Auto Trace Dalgona");

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
        }

        bool useRect = root is RectTransform;
        for (int i = 0; i < worldPoints.Count; i++)
        {
            string name = $"P{(i + 1):00}";
            GameObject pointObject = new GameObject(name, useRect ? typeof(RectTransform) : typeof(Transform));
            Undo.RegisterCreatedObjectUndo(pointObject, "Create Dalgona Path Point");

            if (useRect)
            {
                RectTransform pointRect = pointObject.GetComponent<RectTransform>();
                pointRect.SetParent(root, false);
                pointRect.anchorMin = new Vector2(0.5f, 0.5f);
                pointRect.anchorMax = new Vector2(0.5f, 0.5f);
                pointRect.pivot = new Vector2(0.5f, 0.5f);
                pointRect.sizeDelta = new Vector2(12f, 12f);

                Vector3 local = root.InverseTransformPoint(worldPoints[i]);
                pointRect.anchoredPosition = new Vector2(local.x, local.y);
            }
            else
            {
                Transform pointTransform = pointObject.transform;
                pointTransform.SetParent(root, false);
                pointTransform.localPosition = root.InverseTransformPoint(worldPoints[i]);
            }
        }
    }

    private static List<Vector3> GatherActivePoints(Transform root)
    {
        List<Vector3> points = new List<Vector3>(root.childCount);
        foreach (Transform child in root)
        {
            if (child != null && child.gameObject.activeInHierarchy)
            {
                points.Add(child.position);
            }
        }

        return points;
    }

    private static GUIStyle GetPointLabelStyle()
    {
        if (pointLabelStyle != null)
        {
            return pointLabelStyle;
        }

        pointLabelStyle = new GUIStyle(EditorStyles.boldLabel);
        pointLabelStyle.fontSize = 12;
        pointLabelStyle.normal.textColor = new Color(0.06f, 0.06f, 0.06f, 1f);
        pointLabelStyle.alignment = TextAnchor.MiddleCenter;
        return pointLabelStyle;
    }

    private static GUIStyle GetTitleLabelStyle()
    {
        if (titleLabelStyle != null)
        {
            return titleLabelStyle;
        }

        titleLabelStyle = new GUIStyle(EditorStyles.boldLabel);
        titleLabelStyle.fontSize = 13;
        titleLabelStyle.normal.textColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        titleLabelStyle.alignment = TextAnchor.MiddleLeft;
        return titleLabelStyle;
    }

    private static GUIStyle GetLabelShadowStyle()
    {
        if (labelShadowStyle != null)
        {
            return labelShadowStyle;
        }

        labelShadowStyle = new GUIStyle(EditorStyles.boldLabel);
        labelShadowStyle.fontSize = 12;
        labelShadowStyle.normal.textColor = new Color(1f, 1f, 1f, 0.92f);
        labelShadowStyle.alignment = TextAnchor.MiddleCenter;
        return labelShadowStyle;
    }

    private static void DrawReadableSceneLabel(Vector2 guiPosition, string text, GUIStyle labelStyle, GUIStyle shadowStyle, bool center)
    {
        if (string.IsNullOrEmpty(text) || labelStyle == null || shadowStyle == null)
        {
            return;
        }

        Vector2 labelSize = labelStyle.CalcSize(new GUIContent(text));
        float x = center ? guiPosition.x - (labelSize.x * 0.5f) : guiPosition.x;
        float y = guiPosition.y - (labelSize.y * 0.5f);
        Rect labelRect = new Rect(x, y, labelSize.x, labelSize.y);
        Rect shadowRect = new Rect(labelRect.x + 1f, labelRect.y + 1f, labelRect.width, labelRect.height);
        GUI.Label(shadowRect, text, shadowStyle);
        GUI.Label(labelRect, text, labelStyle);
    }
}
#endif
