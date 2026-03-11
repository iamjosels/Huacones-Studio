using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    [Header("Cámaras virtuales")]
    public CinemachineVirtualCamera camMain;
    public CinemachineVirtualCamera camLeft;
    public CinemachineVirtualCamera camCenter;
    public CinemachineVirtualCamera camRight;

    [Header("Referencias a minijuegos")]
    public GameObject minigameSimon;
    public GameObject minigameOrder;
    public GameObject minigameDalgona;

    [Header("UI")]
    public GameObject startButton;

    private int currentMiniIndex = 0;
    private int currentRound = 1;
    private const int maxRounds = 3;

    private enum MiniType { Dalgona, Simon, Order }
    private bool minigameFinished = false;

    private void Start()
    {
        // Asegúrate que todos los minijuegos estén desactivados al inicio
        minigameSimon.SetActive(false);
        minigameOrder.SetActive(false);
        minigameDalgona.SetActive(false);
    }

    public void OnStartButtonPressed()
    {
        startButton.SetActive(false);
        currentMiniIndex = 0;
        currentRound = 1;
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        currentRound = 1;

        while (currentRound <= maxRounds)
        {
            Debug.Log($"▶️ Iniciando ronda {currentRound}");

            MiniType[] sequence = { MiniType.Dalgona, MiniType.Simon, MiniType.Order };

            foreach (MiniType mini in sequence)
            {
                currentMiniIndex = (int)mini;
                yield return StartCoroutine(PlayMinigame(mini));
            }

            currentRound++;
        }

        Debug.Log("🎉 ¡Juego completado con éxito! 3 rondas superadas.");
        ShowVictoryScreen();
    }

    IEnumerator PlayMinigame(MiniType type)
    {
        SetAllPrioritiesLow();
        yield return new WaitForSeconds(0.5f);

        Debug.Log("🔍 Iniciando minijuego: " + type);

        switch (type)
        {
            case MiniType.Simon:
                camLeft.Priority = 10;
                yield return new WaitForSeconds(2f);

                if (minigameSimon != null)
                {
                    minigameSimon.SetActive(true);
                    SimonManager2 simon = minigameSimon.GetComponentInChildren<SimonManager2>();

                    if (simon != null)
                    {
                        simon.SetRound(currentRound);
                        simon.OnGameFinished = OnMinigameFinished;
                    }
                    else
                    {
                        Debug.LogError("❌ No se encontró el script SimonManager2 en hijos de minigameSimon");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError("❌ minigameSimon no está asignado");
                    yield break;
                }
                break;

            case MiniType.Order:
                camCenter.Priority = 10;
                yield return new WaitForSeconds(2f);

                if (minigameOrder != null)
                {
                    GameNumberR orderGame = minigameOrder.GetComponentInChildren<GameNumberR>();

                    if (orderGame != null)
                    {
                        orderGame.ResetGame();
                        orderGame.SetRound(currentRound);
                        minigameOrder.SetActive(true);
                        orderGame.OnGameFinished = OnMinigameFinished;
                    }
                    else
                    {
                        Debug.LogError("❌ No se encontró el script GameNumberR en hijos de minigameOrder");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError("❌ minigameOrder no está asignado");
                    yield break;
                }
                break;

            case MiniType.Dalgona:
                camRight.Priority = 10;
                yield return new WaitForSeconds(2f);

                if (minigameDalgona != null)
                {
                    minigameDalgona.SetActive(true);
                    DalgonaGameManager dalgona = minigameDalgona.GetComponentInChildren<DalgonaGameManager>();

                    if (dalgona != null)
                    {
                        dalgona.SetRound(currentRound);
                        dalgona.OnGameFinished = OnMinigameFinished;
                    }
                    else
                    {
                        Debug.LogError("❌ No se encontró el script DalgonaGameManager en hijos de minigameDalgona");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError("❌ minigameDalgona no está asignado");
                    yield break;
                }
                break;
        }

        yield return new WaitUntil(() => minigameFinished);
        minigameFinished = false;

        SetAllPrioritiesLow();
        camMain.Priority = 10;
        yield return new WaitForSeconds(1f);

        switch (type)
        {
            case MiniType.Simon: minigameSimon.SetActive(false); break;
            case MiniType.Order: minigameOrder.SetActive(false); break;
            case MiniType.Dalgona: minigameDalgona.SetActive(false); break;
        }

        yield return new WaitForSeconds(1f);
    }


    public void OnMinigameFinished(bool success)
    {
        if (!success)
        {
            Debug.Log("❌ Perdiste. Reiniciando juego.");

            StopAllCoroutines();
            SetAllPrioritiesLow();
            camMain.Priority = 10;

            switch ((MiniType)currentMiniIndex)
            {
                case MiniType.Simon:
                    minigameSimon.SetActive(false);
                    break;
                case MiniType.Order:
                    minigameOrder.SetActive(false);
                    break;
                case MiniType.Dalgona:
                    minigameDalgona.SetActive(false);
                    break;
            }

            startButton.SetActive(true);
        }
        else
        {
            minigameFinished = true;
        }
    }

    private void SetAllPrioritiesLow()
    {
        camMain.Priority = 0;
        camLeft.Priority = 0;
        camCenter.Priority = 0;
        camRight.Priority = 0;
    }

    private void ShowVictoryScreen()
    {
        Debug.Log("✨ ¡Ganaste el juego completo!");
        // Aquí puedes activar un Canvas con texto, botón de reiniciar, etc.
    }
}

