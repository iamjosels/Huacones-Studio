using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    [Header("Prefab y referencias")]
    public GameObject notePrefab;
    public RectTransform spawnZone;
    public Transform noteParent;

    [Header("Targets visuales de generación")]
    public List<NoteSpawnTarget> spawnTargets;

    public void SpawnNote(string direction)
    {
        RectTransform target = GetTargetForDirection(direction);
        if (target == null)
        {
            Debug.LogWarning("No se encontró posición para: " + direction);
            return;
        }

        GameObject note = Instantiate(notePrefab, noteParent);
        var noteScript = note.GetComponent<NoteSymbol>();
        noteScript.noteType = direction;

        RectTransform rt = note.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(target.anchoredPosition.x, spawnZone.anchoredPosition.y);
    }

    private RectTransform GetTargetForDirection(string direction)
    {
        foreach (var target in spawnTargets)
        {
            if (target.noteType.Trim().ToLower() == direction.Trim().ToLower())
                return target.targetTransform;
        }
        return null;
    }
}