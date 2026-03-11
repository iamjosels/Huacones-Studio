using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameNumberR : MonoBehaviour
{
    public static GameNumberR Instance { get; private set; }

    [Header("Prefabs y contenedores")]
    public Transform numerosContainer;               // Contenedor para los números
    public GameObject draggableNumberPrefab;         // Prefab de número arrastrable
    public Sprite[] numeroSprites;                   // Sprites de 1 al 10 (papel1, papel2, ...)

    [Header("Slots")]
    public GameObject slotPrefab;                    // Prefab del slot con imagen de "X" y DropSlot
    public Transform slotContainer;                  // Contenedor de slots ("SlotContainer")

    [HideInInspector] public List<DropSlot> dropSlots = new(); // Lista dinámica de slots generados

    [Header("UI")]
    public TextMeshProUGUI tiempoTexto;
    //public TextMeshProUGUI mensajeFinal;

    [Header("Tiempo límite")]
    public float tiempoLimite = 30f;
    private float tiempoRestante;
    private bool juegoActivo = true;

    [Header("Posiciones de slots (locales)")]
    public Vector2[] slotPositions = new Vector2[10];

    private int rondaActual = 1;
    public System.Action<bool> OnGameFinished;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        tiempoRestante = tiempoLimite;
        //mensajeFinal.gameObject.SetActive(false);

        GenerarSlots();
        GenerarNumerosAleatorios();
    }

    private void Update()
    {
        if (!juegoActivo) return;

        tiempoRestante -= Time.deltaTime;
        tiempoTexto.text = $"Tiempo: {Mathf.Ceil(tiempoRestante)}";

        if (tiempoRestante <= 0)
        {
            juegoActivo = false;
            FinalizarJuego(false);
        }

        // Solo durante desarrollo: saltar minijuego con Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("⏩ Saltando minijuego con Enter (modo prueba)");
            FinalizarJuego(true); // lo pasamos como ganado
        }

        #if UNITY_EDITOR
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    FinalizarJuego(true);
                }
        #endif

    }

    private void GenerarSlots()
    {
        dropSlots.Clear();

        for (int i = 0; i < 10; i++)
        {
            GameObject nuevoSlot = Instantiate(slotPrefab, slotContainer);
            RectTransform rt = nuevoSlot.GetComponent<RectTransform>();

            // 👇 Posición personalizada (local dentro de SlotContainer)
            rt.anchoredPosition = slotPositions[i];
            rt.localScale = Vector3.one;

            DropSlot drop = nuevoSlot.GetComponent<DropSlot>();
            dropSlots.Add(drop);
        }
    }


    private void GenerarNumerosAleatorios()
    {
        List<int> numeros = new();
        for (int i = 1; i <= 10; i++) numeros.Add(i);

        // Mezclar
        for (int i = 0; i < numeros.Count; i++)
        {
            int rand = Random.Range(i, numeros.Count);
            (numeros[i], numeros[rand]) = (numeros[rand], numeros[i]);
        }

        foreach (int numero in numeros)
        {
            GameObject nuevo = Instantiate(draggableNumberPrefab, numerosContainer);
            nuevo.transform.localScale = Vector3.one;

            DraggableNumber script = nuevo.GetComponent<DraggableNumber>();
            script.numero = numero;

            Image img = nuevo.GetComponent<Image>();
            img.sprite = numeroSprites[numero - 1];
            img.raycastTarget = true;
        }
    }

    public void CheckWinCondition()
    {
        for (int i = 0; i < dropSlots.Count; i++)
        {
            int? valor = dropSlots[i].GetCurrentNumber();
            if (valor == null || valor != i + 1)
                return; // Aún no están bien colocados
        }

        juegoActivo = false;
        FinalizarJuego(true);
    }

    private void FinalizarJuego(bool victoria)
    {
        //mensajeFinal.text = victoria ? "¡Ganaste!" : "¡Tiempo agotado!";
        //mensajeFinal.gameObject.SetActive(true);

        // Notificar al GameManager global
        OnGameFinished?.Invoke(victoria);
    }

    public void ResetGame()
    {
        // 🧹 Limpiar slots
        foreach (var slot in dropSlots)
        {
            if (slot.currentNumber != null)
            {
                Destroy(slot.currentNumber.gameObject);
                slot.ClearSlot();
            }
            Destroy(slot.gameObject); // eliminamos el slot visual también
        }

        dropSlots.Clear();

        // 🧹 Limpiar números
        foreach (Transform child in numerosContainer)
        {
            Destroy(child.gameObject);
        }

        // 🔁 Generar nuevamente
        //mensajeFinal.gameObject.SetActive(false);
        GenerarSlots();
        GenerarNumerosAleatorios();

        tiempoRestante = tiempoLimite;
        juegoActivo = true;
    }


    public void SetRound(int round)
    {
        rondaActual = round;

        switch (rondaActual)
        {
            case 1:
                tiempoLimite = 30f;
                break;
            case 2:
                tiempoLimite = 20f;
                break;
            case 3:
                tiempoLimite = 15f;
                break;
        }

        tiempoRestante = tiempoLimite; // reiniciar tiempo
    }

}
