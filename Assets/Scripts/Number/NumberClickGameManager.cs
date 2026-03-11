using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberClickGameManager : MonoBehaviour
{
    [Header("Botones y UI")]
    public List<Button> numberButtons;
    public TextMeshProUGUI timerText;

    [Header("Configuración base")]
    public float baseTimeLimit = 10f;
    public float timeReductionPerRound = 1f;

    public System.Action<bool> OnGameFinished;

    private float timeRemaining;
    private int currentExpected;
    private bool gameActive = false;

    private int round = 1;
    private int maxRounds = 3;

    public void SetRound(int roundNumber)
    {
        round = Mathf.Clamp(roundNumber, 1, maxRounds);
    }

    void OnEnable()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentExpected = 1;
        gameActive = true;

        // Aplicar dificultad según la ronda
        timeRemaining = Mathf.Max(3f, baseTimeLimit - timeReductionPerRound * (round - 1));
        Debug.Log("Ronda actual: " + round);
        if (numberButtons.Count != 10)
        {
            Debug.LogError($"Se esperaban 10 botones pero hay {numberButtons.Count}");
            gameActive = false;
            return;
        }

        List<int> numbers = new();
        for (int i = 1; i <= 10; i++) numbers.Add(i);
        Shuffle(numbers);

        for (int i = 0; i < numberButtons.Count; i++)
        {
            int num = numbers[i];

            Button btn = numberButtons[i];
            if (btn == null) continue;

            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = num.ToString();

            btn.interactable = true;
            btn.onClick.RemoveAllListeners();

            int capturedNum = num;
            btn.onClick.AddListener(() => OnNumberClicked(capturedNum, btn));
        }

        UpdateTimerUI();
    }

    void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (timeRemaining <= 0f)
        {
            GameOver(false);
        }

#if UNITY_EDITOR
        // Solo en pruebas: simular victoria con ENTER
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("⏩ Simulación automática: Victoria por ENTER");
            GameOver(true);
        }
#endif
    }

    void UpdateTimerUI()
    {
        timerText.text = "Tiempo: " + Mathf.CeilToInt(timeRemaining).ToString();
    }

    void OnNumberClicked(int clickedNumber, Button btn)
    {
        if (!gameActive) return;

        if (clickedNumber == currentExpected)
        {
            btn.interactable = false;
            currentExpected++;

            if (currentExpected > 10)
            {
                GameOver(true);
            }
        }
        else
        {
            // Aquí puedes agregar animación/error
        }
    }

    void GameOver(bool won)
    {
        gameActive = false;
        timerText.text = won ? "¡Ganaste!" : "¡Perdiste!";

        foreach (var btn in numberButtons)
        {
            btn.interactable = false;
        }

        OnGameFinished?.Invoke(won);
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
