using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NoteVisualManager", menuName = "HuaconGame/NoteVisualManager", order = 2)]
public class NoteVisualManager : ScriptableObject
{
    public List<NoteTypeData> noteVisuals;

    public Sprite GetSpriteByType(string type)
    {
        foreach (var data in noteVisuals)
        {
            if (data.noteType == type)
                return data.sprite;
        }
        return null;
    }
}
