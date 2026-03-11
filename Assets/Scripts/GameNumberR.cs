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

        foreach (int numero in numeros)
        {
            GameObject nuevo = Instantiate(draggableNumberPrefab, numerosContainer);
            nuevo.transform.localScale = Vector3.one;

            DraggableNumber script = nuevo.GetComponent<DraggableNumber>();
            script.numero = numero;

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
}
