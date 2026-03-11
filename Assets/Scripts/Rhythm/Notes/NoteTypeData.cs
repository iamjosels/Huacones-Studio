using UnityEngine;

[CreateAssetMenu(fileName = "NoteTypeData", menuName = "HuaconGame/NoteTypeData", order = 1)]
public class NoteTypeData : ScriptableObject
{
    public string noteType; // Ej: "Left", "Down"
    public Sprite sprite;   // Sprite rúnico asociado
}