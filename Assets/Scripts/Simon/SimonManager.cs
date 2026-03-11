using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimonManager : MonoBehaviour
{
    public Button greenButton;
    public Button redButton;
    public Button yellowButton;
    public Button blueButton;

    private List<Button> pattern = new List<Button>();
    private List<Button> playerInput = new List<Button>();
    //private int currentStep = 0;
    private bool isPlayerTurn = false;

    void Start()
    {
        StartCoroutine(GeneratePattern());
    }

    IEnumerator GeneratePattern()
    {
        isPlayerTurn = false;
        playerInput.Clear();
        //currentStep = 0;

        // Agrega un nuevo botón al patrón aleatoriamente
        Button[] buttons = { greenButton, redButton, yellowButton, blueButton };
        Button nextButton = buttons[Random.Range(0, buttons.Length)];
        pattern.Add(nextButton);

        yield return new WaitForSeconds(1f);

        // Mostrar el patrón (uno por uno)
        foreach (Button btn in pattern)
        {
            StartCoroutine(FlashButton(btn));
            yield return new WaitForSeconds(1f);
        }

        isPlayerTurn = true;
    }

    IEnumerator FlashButton(Button btn)
    {
        Color originalColor = btn.image.color;
        btn.image.color = Color.white;
        yield return new WaitForSeconds(0.3f);
        btn.image.color = originalColor;
    }

    public void OnButtonClick(Button clickedButton)
    {
        if (!isPlayerTurn) return;

        playerInput.Add(clickedButton);

        if (clickedButton != pattern[playerInput.Count - 1])
        {
            Debug.Log("Incorrecto. Reiniciando juego.");
            pattern.Clear();
            StartCoroutine(GeneratePattern());
            return;
        }

        if (playerInput.Count == pattern.Count)
        {
            Debug.Log("¡Correcto! Nueva ronda.");
            StartCoroutine(GeneratePattern());
        }
    }
}
