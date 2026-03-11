using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NoteInputHandler : MonoBehaviour
{
    [Header("Configuración")]
    public string noteType; // "Left", "Right", etc.
    public float perfectThreshold;
    public float goodThreshold;

    public RectTransform impactZone;

    public PlayerAnimator playerAnimator;

    // NUEVO: asignación doble (flechas + WASD)
    private static readonly Dictionary<string, KeyCode[]> keyMappings = new Dictionary<string, KeyCode[]>
    {
        { "Left",  new KeyCode[] { KeyCode.LeftArrow, KeyCode.A } },
        { "Down",  new KeyCode[] { KeyCode.DownArrow, KeyCode.S } },
        { "Up",    new KeyCode[] { KeyCode.UpArrow, KeyCode.W } },
        { "Right", new KeyCode[] { KeyCode.RightArrow, KeyCode.D } }
    };

    void Update()
    {
        if (!keyMappings.ContainsKey(noteType)) return;

        foreach (KeyCode key in keyMappings[noteType])
        {
            if (Input.GetKeyDown(key))
            {
                TryHitNote();
                break;
            }
        }
    }

    void TryHitNote()
    {
        NoteSymbol[] allNotes = FindObjectsOfType<NoteSymbol>();

        foreach (NoteSymbol note in allNotes)
        {
            if (note == null || note.noteType != noteType) continue;

            RectTransform noteRect = note.GetComponent<RectTransform>();
            if (noteRect == null) continue;

            float distance = Mathf.Abs(noteRect.anchoredPosition.y - impactZone.anchoredPosition.y);

            if (distance <= perfectThreshold)
            {
                playerAnimator?.PlayDirection(noteType);
                Debug.Log("Perfect!");
                RitualGameManager.Instance?.RegisterHit("Perfect");
                Destroy(note.gameObject);
                return;
            }
            else if (distance <= goodThreshold)
            {
                playerAnimator?.PlayDirection(noteType);
                Debug.Log("Good!");
                RitualGameManager.Instance?.RegisterHit("Good");
                Destroy(note.gameObject);
                return;
            }
            playerAnimator?.PlayFail();
            Debug.Log("Fail!");
            RitualGameManager.Instance?.RegisterHit("Fail");
        }
    }
}
