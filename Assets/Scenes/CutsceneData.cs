using UnityEngine;

[CreateAssetMenu(fileName = "NewCutsceneData", menuName = "Cutscene/Data")]
public class CutsceneData : ScriptableObject
{
    public Sprite[] images;
    public string nextSceneName;
}
