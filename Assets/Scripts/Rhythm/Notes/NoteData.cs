using System.Collections.Generic;

[System.Serializable]
public class NoteData
{
    public float time;
    public string type;
}

[System.Serializable]
public class NoteTimeline
{
    public float bpm;
    public List<NoteData> notes;
}
