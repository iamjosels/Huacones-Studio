using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameNumberR : MonoBehaviour
{
    public static GameNumberR Instance { get; private set; }

    [Header("Prefabs y contenedores")]
    public Transform numerosContainer;
    public GameObject draggableNumberPrefab;
    public Sprite[] numeroSprites;
    public GridLayoutGroup numerosLayoutGroup;

    [Header("Slots")]
    public GameObject slotPrefab;
    public Transform slotContainer;

    [HideInInspector] public List<DropSlot> dropSlots = new();

    [Header("UI")]
    public TextMeshProUGUI tiempoTexto;

    [Header("Tiempo limite")]
    public float tiempoLimite = 35f;
    private float tiempoRestante;
    private bool juegoActivo;

    [Header("Posiciones de slots (locales)")]
    public Vector2[] slotPositions = new Vector2[10];

    [Header("Layout visual de numeros")]
    public bool spreadNumbersInBottomArea = true;
    public Vector2 spreadAreaSize = new Vector2(1500f, 230f);
    public Vector2 spreadAreaAnchoredPosition = new Vector2(0f, 120f);
    public Vector2 spreadAreaPadding = new Vector2(70f, 28f);
    public float spreadXJitter = 52f;
    public float spreadYJitter = 24f;
    public float spreadRotationRange = 15f;

    [Header("Fallback en grilla (si spread esta desactivado)")]
    public int spawnColumns = 5;
    public Vector2 spawnCellSize = new Vector2(100f, 100f);
    public Vector2 spawnSpacing = new Vector2(16f, 16f);
    public Vector2 spawnAnchoredPosition = new Vector2(-560f, -210f);
    public Vector3 spawnContainerScale = Vector3.one;

    private bool initialized;
    private int rondaActual = 1;

    public System.Action<bool> OnGameFinished;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        tiempoRestante = tiempoLimite;
        ConfigureNumberSpawnLayout();
        EnsureBoardGenerated();
        juegoActivo = true;
        UpdateTimerText();
    }

    private void Update()
    {
        if (!juegoActivo)
        {
            return;
        }

        tiempoRestante -= Time.deltaTime;
        UpdateTimerText();

        if (tiempoRestante <= 0f)
        {
            FinalizarJuego(false);
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Return))
        {
            FinalizarJuego(true);
        }
#endif
    }

    private void UpdateTimerText()
    {
        if (tiempoTexto != null)
        {
            tiempoTexto.text = $"Tiempo: {Mathf.Ceil(Mathf.Max(0f, tiempoRestante))}";
        }
    }

    private void EnsureBoardGenerated()
    {
        if (initialized)
        {
            return;
        }

        if (slotContainer != null && slotContainer.childCount > 0)
        {
            dropSlots.Clear();
            DropSlot[] existingSlots = slotContainer.GetComponentsInChildren<DropSlot>(true);
            dropSlots.AddRange(existingSlots);
            initialized = true;
            return;
        }

        GenerarSlots();
        GenerarNumerosAleatorios();
        initialized = true;
    }

    private void GenerarSlots()
    {
        dropSlots.Clear();

        for (int i = 0; i < 10; i++)
        {
            GameObject nuevoSlot = Instantiate(slotPrefab, slotContainer);
            RectTransform rt = nuevoSlot.GetComponent<RectTransform>();

            rt.anchoredPosition = slotPositions[i];
            rt.localScale = Vector3.one;

            DropSlot drop = nuevoSlot.GetComponent<DropSlot>();
            dropSlots.Add(drop);
        }
    }

    private void GenerarNumerosAleatorios()
    {
        ConfigureNumberSpawnLayout();

        List<int> numeros = new();
        for (int i = 1; i <= 10; i++)
        {
            numeros.Add(i);
        }

        for (int i = 0; i < numeros.Count; i++)
        {
            int randomIndex = Random.Range(i, numeros.Count);
            (numeros[i], numeros[randomIndex]) = (numeros[randomIndex], numeros[i]);
        }

        List<Vector2> spreadPositions = spreadNumbersInBottomArea
            ? GenerateBottomSpreadPositions(numeros.Count)
            : null;

        for (int i = 0; i < numeros.Count; i++)
        {
            int numero = numeros[i];
            GameObject nuevo = Instantiate(draggableNumberPrefab, numerosContainer);
            nuevo.transform.localScale = Vector3.one;

            DraggableNumber script = nuevo.GetComponent<DraggableNumber>();
            script.numero = numero;

            RectTransform numberRect = nuevo.GetComponent<RectTransform>();
            if (numberRect != null)
            {
                if (spreadNumbersInBottomArea && spreadPositions != null && i < spreadPositions.Count)
                {
                    numberRect.anchorMin = new Vector2(0.5f, 0.5f);
                    numberRect.anchorMax = new Vector2(0.5f, 0.5f);
                    numberRect.pivot = new Vector2(0.5f, 0.5f);
                    numberRect.anchoredPosition = spreadPositions[i];
                    numberRect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-spreadRotationRange, spreadRotationRange));
                }
                else
                {
                    numberRect.localRotation = Quaternion.identity;
                }
            }

            Image image = nuevo.GetComponent<Image>();
            image.sprite = numeroSprites[numero - 1];
            image.raycastTarget = true;
        }
    }

    public void CheckWinCondition()
    {
        for (int i = 0; i < dropSlots.Count; i++)
        {
            int? valor = dropSlots[i].GetCurrentNumber();
            if (valor == null || valor != i + 1)
            {
                return;
            }
        }

        FinalizarJuego(true);
    }

    private void FinalizarJuego(bool victoria)
    {
        if (!juegoActivo)
        {
            return;
        }

        juegoActivo = false;
        OnGameFinished?.Invoke(victoria);
    }

    public void ResetGame()
    {
        ConfigureNumberSpawnLayout();

        if (slotContainer != null)
        {
            for (int i = slotContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(slotContainer.GetChild(i).gameObject);
            }
        }

        if (numerosContainer != null)
        {
            for (int i = numerosContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(numerosContainer.GetChild(i).gameObject);
            }
        }

        dropSlots.Clear();

        GenerarSlots();
        GenerarNumerosAleatorios();

        tiempoRestante = tiempoLimite;
        juegoActivo = true;
        initialized = true;
        UpdateTimerText();
    }

    public void SetRound(int round)
    {
        rondaActual = Mathf.Max(1, round);

        switch (rondaActual)
        {
            case 1:
                tiempoLimite = 35f;
                break;
            case 2:
                tiempoLimite = 28f;
                break;
            case 3:
                tiempoLimite = 22f;
                break;
            default:
                tiempoLimite = Mathf.Max(18f, 35f - (rondaActual - 1) * 6f);
                break;
        }

        tiempoRestante = tiempoLimite;
        UpdateTimerText();
    }

    private void ConfigureNumberSpawnLayout()
    {
        if (numerosContainer == null)
        {
            return;
        }

        RectTransform numbersRect = numerosContainer as RectTransform;
        if (numbersRect != null)
        {
            numbersRect.localScale = spawnContainerScale;

            if (spreadNumbersInBottomArea)
            {
                numbersRect.anchorMin = new Vector2(0.5f, 0f);
                numbersRect.anchorMax = new Vector2(0.5f, 0f);
                numbersRect.pivot = new Vector2(0.5f, 0.5f);
                numbersRect.anchoredPosition = spreadAreaAnchoredPosition;
                numbersRect.sizeDelta = new Vector2(
                    Mathf.Max(200f, spreadAreaSize.x),
                    Mathf.Max(120f, spreadAreaSize.y));
            }
            else
            {
                numbersRect.anchorMin = new Vector2(0.5f, 0.5f);
                numbersRect.anchorMax = new Vector2(0.5f, 0.5f);
                numbersRect.pivot = new Vector2(0.5f, 0.5f);
                numbersRect.anchoredPosition = spawnAnchoredPosition;
            }
        }

        GridLayoutGroup layout = numerosLayoutGroup != null
            ? numerosLayoutGroup
            : numerosContainer.GetComponent<GridLayoutGroup>();

        if (layout == null)
        {
            return;
        }

        if (spreadNumbersInBottomArea)
        {
            layout.enabled = false;
            return;
        }

        layout.enabled = true;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = Mathf.Max(1, spawnColumns);
        layout.cellSize = spawnCellSize;
        layout.spacing = spawnSpacing;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
    }

    private List<Vector2> GenerateBottomSpreadPositions(int count)
    {
        List<Vector2> positions = new(count);
        if (count <= 0)
        {
            return positions;
        }

        float width = Mathf.Max(200f, spreadAreaSize.x);
        float height = Mathf.Max(120f, spreadAreaSize.y);

        float left = -width * 0.5f + spreadAreaPadding.x;
        float right = width * 0.5f - spreadAreaPadding.x;
        float bottom = -height * 0.5f + spreadAreaPadding.y;
        float top = height * 0.5f - spreadAreaPadding.y;

        float safeXJitter = Mathf.Max(0f, spreadXJitter);
        float safeYJitter = Mathf.Max(0f, spreadYJitter);

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);
            float xBase = Mathf.Lerp(left, right, t);
            float x = Mathf.Clamp(xBase + Random.Range(-safeXJitter, safeXJitter), left, right);

            float lane = (i % 2 == 0) ? 0.32f : 0.72f;
            float yBase = Mathf.Lerp(bottom, top, lane);
            float y = Mathf.Clamp(yBase + Random.Range(-safeYJitter, safeYJitter), bottom, top);

            positions.Add(new Vector2(x, y));
        }

        for (int i = 0; i < positions.Count; i++)
        {
            int randomIndex = Random.Range(i, positions.Count);
            (positions[i], positions[randomIndex]) = (positions[randomIndex], positions[i]);
        }

        return positions;
    }
}
