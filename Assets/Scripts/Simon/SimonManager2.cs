using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimonManager2 : MonoBehaviour
{
    [Header("Botones de colores")]
    public Button greenButton;
    public Button redButton;
    public Button yellowButton;
    public Button blueButton;

    [Header("UI de estado")]
    public TextMeshProUGUI statusText;

    [Header("Parámetros base de dificultad")]
    public float baseFlashTime = 0.3f;
    public float baseTimeBetweenFlashes = 0.8f;
    public int baseMaxTurns = 5;

    private float flashTime;
    private float timeBetweenFlashes;
    private int maxTurns = 5;

    private List<Button> pattern = new();
    private List<Button> playerInput = new();
    private bool isPlayerTurn = false;
    private int currentTurn = 0;

    public System.Action<bool> OnGameFinished;

    private Button[] allButtons;
    private bool gameActive = false;
    private bool isFlashing = false;


    public void SetRound(int gameRound)
    {
        // Ajustar dificultad basada en la ronda general (del GameManager)
        maxTurns = baseMaxTurns + (gameRound - 1) * 2;  // Ronda 1 = 5 turnos, Ronda 2 = 7, etc.
        flashTime = Mathf.Max(0.15f, baseFlashTime - 0.05f * (gameRound - 1));
        timeBetweenFlashes = Mathf.Max(0.3f, baseTimeBetweenFlashes - 0.1f * (gameRound - 1));
    }

    void Awake()
    {
        allButtons = new Button[] { greenButton, redButton, yellowButton, blueButton };
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() => OnButtonClick(btn));
        }
    }

    void OnEnable()
    {
        pattern.Clear();
        playerInput.Clear();
        currentTurn = 0;
        StartGame();
    }

    public void StartGame()
    {
        pattern.Clear();
        playerInput.Clear();
        currentTurn = 0;
        StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        isPlayerTurn = false;
        gameActive = false;
        isFlashing = true; // ← nuevo

        playerInput.Clear();
        currentTurn++;

        if (currentTurn > maxTurns)
        {
            statusText.text = "¡Completado!";
            yield return new WaitForSeconds(1f);
            OnGameFinished?.Invoke(true);
            yield break;
        }

        // Añadir un nuevo botón al patrón
        Button nextButton = allButtons[Random.Range(0, allButtons.Length)];
        pattern.Add(nextButton);

        statusText.text = $"Ronda {currentTurn} - Observa el patrón...";
        yield return new WaitForSeconds(1f);

        foreach (Button btn in pattern)
        {
            yield return StartCoroutine(FlashButton(btn));
            yield return new WaitForSeconds(timeBetweenFlashes);
        }

        isFlashing = false; // ← nuevo
        isPlayerTurn = true;
        gameActive = true;
        playerInput.Clear();
        statusText.text = $"Tu turno: {pattern.Count} pasos";
    }


    IEnumerator FlashButton(Button btn)
    {
        Image img = btn.image;
        Color originalColor = img.color;
        Vector3 originalScale = btn.transform.localScale;

        // Hacerlo más brillante (opcional: subir alfa o color claro)
        img.color = new Color(1f, 1f, 1f, 1f); // blanco puro

        // Escalar ligeramente para destacar
        btn.transform.localScale = originalScale * 1.2f;

        yield return new WaitForSeconds(flashTime);

        // Restaurar
        img.color = originalColor;
        btn.transform.localScale = originalScale;
    }


    void OnButtonClick(Button clickedButton)
    {
        if (!isPlayerTurn || !gameActive) return;

        playerInput.Add(clickedButton);
        int i = playerInput.Count - 1;

        if (i >= pattern.Count)
        {
            Debug.LogWarning("Click inesperado fuera del patrón.");
            return;
        }

        if (clickedButton != pattern[i])
        {
            statusText.text = "¡Te equivocaste!";
            isPlayerTurn = false;
            gameActive = false;
            StartCoroutine(HandleLoss());
            return;
        }

        if (playerInput.Count == pattern.Count)
        {
            statusText.text = "¡Correcto!";
            isPlayerTurn = false;
            gameActive = false;
            StartCoroutine(NextTurn());
        }
    }

    IEnumerator HandleLoss()
    {
        yield return new WaitForSeconds(1f);
        OnGameFinished?.Invoke(false);
    }

    void Update()
    {
    #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Return) && isPlayerTurn && !isFlashing)
            {
                Debug.Log("⏩ Simulación automática de victoria (Enter presionado)");
                isPlayerTurn = false;
                gameActive = false;
                StartCoroutine(NextTurn());
            }
    #endif
        }


}
