using System.Collections;
using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum MiniType
    {
        Dalgona,
        Simon,
        Order
    }

    [Header("Camaras virtuales")]
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

    [Header("Timing")]
    public float cameraSettleDelay = 0.8f;
    public float returnToMainDelay = 0.5f;
    public float betweenMinigamesDelay = 0.4f;

    [Header("Rondas")]
    public int maxRounds = 3;

    private int currentMiniIndex;
    private int currentRound = 1;
    private bool minigameFinished;

    private bool isSequenceRunning;
    private Coroutine sequenceRoutine;

    public bool DebugIsSequenceRunning => isSequenceRunning;
    public int DebugCurrentRound => currentRound;
    public string DebugCurrentMiniName => ((MiniType)Mathf.Clamp(currentMiniIndex, 0, 2)).ToString();

    private void Start()
    {
        DeactivateAllMinigames();
        SetAllPrioritiesLow();
        camMain.Priority = 10;
    }

    public void OnStartButtonPressed()
    {
        StartFullRun();
    }

    public void StartFullRun()
    {
        StopSequenceFlow();
        DeactivateAllMinigames();
        SetAllPrioritiesLow();
        camMain.Priority = 10;

        currentMiniIndex = 0;
        currentRound = 1;
        minigameFinished = false;

        if (startButton != null)
        {
            startButton.SetActive(false);
        }

        sequenceRoutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        isSequenceRunning = true;
        currentRound = 1;

        while (currentRound <= maxRounds)
        {
            MiniType[] sequence = { MiniType.Dalgona, MiniType.Simon, MiniType.Order };

            foreach (MiniType mini in sequence)
            {
                currentMiniIndex = (int)mini;
                yield return StartCoroutine(PlayMinigame(mini));
            }

            currentRound++;
        }

        isSequenceRunning = false;
        sequenceRoutine = null;
        ShowVictoryScreen();

        if (startButton != null)
        {
            startButton.SetActive(true);
        }
    }

    private IEnumerator PlayMinigame(MiniType type)
    {
        SetAllPrioritiesLow();
        yield return new WaitForSeconds(0.2f);

        switch (type)
        {
            case MiniType.Simon:
                camLeft.Priority = 10;
                yield return new WaitForSeconds(cameraSettleDelay);

                if (minigameSimon == null)
                {
                    Debug.LogError("minigameSimon no esta asignado");
                    yield break;
                }

                minigameSimon.SetActive(true);
                SimonManager2 simon = minigameSimon.GetComponentInChildren<SimonManager2>(true);
                if (simon == null)
                {
                    Debug.LogError("No se encontro SimonManager2 en minigameSimon");
                    yield break;
                }

                simon.SetRound(currentRound);
                simon.OnGameFinished = OnMinigameFinished;
                break;

            case MiniType.Order:
                camCenter.Priority = 10;
                yield return new WaitForSeconds(cameraSettleDelay);

                if (minigameOrder == null)
                {
                    Debug.LogError("minigameOrder no esta asignado");
                    yield break;
                }

                minigameOrder.SetActive(true);
                GameNumberR orderGame = minigameOrder.GetComponentInChildren<GameNumberR>(true);
                if (orderGame == null)
                {
                    Debug.LogError("No se encontro GameNumberR en minigameOrder");
                    yield break;
                }

                orderGame.SetRound(currentRound);
                orderGame.ResetGame();
                orderGame.OnGameFinished = OnMinigameFinished;
                break;

            case MiniType.Dalgona:
                camRight.Priority = 10;
                yield return new WaitForSeconds(cameraSettleDelay);

                if (minigameDalgona == null)
                {
                    Debug.LogError("minigameDalgona no esta asignado");
                    yield break;
                }

                minigameDalgona.SetActive(true);
                DalgonaGameManager dalgona = minigameDalgona.GetComponentInChildren<DalgonaGameManager>(true);
                if (dalgona == null)
                {
                    Debug.LogError("No se encontro DalgonaGameManager en minigameDalgona");
                    yield break;
                }

                dalgona.SetRound(currentRound);
                dalgona.OnGameFinished = OnMinigameFinished;
                break;
        }

        yield return new WaitUntil(() => minigameFinished);
        minigameFinished = false;

        SetAllPrioritiesLow();
        camMain.Priority = 10;
        yield return new WaitForSeconds(returnToMainDelay);

        DeactivateMinigame(type);
        yield return new WaitForSeconds(betweenMinigamesDelay);
    }

    public void OnMinigameFinished(bool success)
    {
        if (!success)
        {
            HandleFailure();
            return;
        }

        minigameFinished = true;
    }

    public void DebugForceCurrentWin()
    {
        if (!IsAnyMinigameActive())
        {
            return;
        }

        OnMinigameFinished(true);
    }

    public void DebugForceCurrentLoss()
    {
        if (!IsAnyMinigameActive())
        {
            return;
        }

        OnMinigameFinished(false);
    }

    public void DebugPlaySingleMini(MiniType miniType, int round)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        StopSequenceFlow();

        currentRound = Mathf.Clamp(round, 1, maxRounds);
        currentMiniIndex = (int)miniType;
        minigameFinished = false;

        SetAllPrioritiesLow();
        camMain.Priority = 10;
        DeactivateAllMinigames();

        if (startButton != null)
        {
            startButton.SetActive(false);
        }

        sequenceRoutine = StartCoroutine(PlaySingleMiniRoutine(miniType));
#endif
    }

    private IEnumerator PlaySingleMiniRoutine(MiniType miniType)
    {
        isSequenceRunning = true;
        yield return StartCoroutine(PlayMinigame(miniType));
        isSequenceRunning = false;
        sequenceRoutine = null;

        if (startButton != null)
        {
            startButton.SetActive(true);
        }
    }

    private void HandleFailure()
    {
        StopSequenceFlow();
        SetAllPrioritiesLow();
        camMain.Priority = 10;
        DeactivateAllMinigames();

        if (startButton != null)
        {
            startButton.SetActive(true);
        }
    }

    private void StopSequenceFlow()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }

        isSequenceRunning = false;
        minigameFinished = false;
    }

    private bool IsAnyMinigameActive()
    {
        return (minigameSimon != null && minigameSimon.activeSelf)
            || (minigameOrder != null && minigameOrder.activeSelf)
            || (minigameDalgona != null && minigameDalgona.activeSelf);
    }

    private void DeactivateAllMinigames()
    {
        if (minigameSimon != null)
        {
            minigameSimon.SetActive(false);
        }

        if (minigameOrder != null)
        {
            minigameOrder.SetActive(false);
        }

        if (minigameDalgona != null)
        {
            minigameDalgona.SetActive(false);
        }
    }

    private void DeactivateMinigame(MiniType type)
    {
        switch (type)
        {
            case MiniType.Simon:
                if (minigameSimon != null)
                {
                    minigameSimon.SetActive(false);
                }
                break;

            case MiniType.Order:
                if (minigameOrder != null)
                {
                    minigameOrder.SetActive(false);
                }
                break;

            case MiniType.Dalgona:
                if (minigameDalgona != null)
                {
                    minigameDalgona.SetActive(false);
                }
                break;
        }
    }

    private void SetAllPrioritiesLow()
    {
        if (camMain != null) camMain.Priority = 0;
        if (camLeft != null) camLeft.Priority = 0;
        if (camCenter != null) camCenter.Priority = 0;
        if (camRight != null) camRight.Priority = 0;
    }

    private void ShowVictoryScreen()
    {
        Debug.Log("Juego completado");
    }
}
