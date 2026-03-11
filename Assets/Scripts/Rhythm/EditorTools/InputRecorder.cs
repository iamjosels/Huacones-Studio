using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InputRecorder : MonoBehaviour
{
    [System.Serializable]
    public class NoteData
    {
        public float time;
        public string type;
    }

    [System.Serializable]
    public class NoteTimeline
    {
        public float bpm = 100; // puedes ajustar después
        public List<NoteData> notes = new List<NoteData>();
    }

    public KeyCode[] trackedKeys = { KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.RightArrow };
    public string[] noteTypes = { "Left", "Down", "Up", "Right" };

    public AudioSource musicSource;
    private float startTime;
    private NoteTimeline timeline = new NoteTimeline();

    void Start()
    {
        startTime = Time.time;
        if (musicSource != null)
        {
            musicSource.Play();
            Debug.Log(" Música iniciada y grabación de beats en marcha.");
        }
    }

    void Update()
    {
        for (int i = 0; i < trackedKeys.Length; i++)
        {
            if (Input.GetKeyDown(trackedKeys[i]))
            {
                float time = Time.time - startTime;
                NoteData note = new NoteData
                {
                    time = Mathf.Round(time * 100f) / 100f, // redondea a 2 decimales
                    type = noteTypes[i]
                };
                timeline.notes.Add(note);
                Debug.Log($"Beat registrado → {note.type} @ {note.time}s");
            }
        }

        // Guardar al presionar Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ExportToJSON();
        }
    }

    public void ExportToJSON()
    {
        string folderPath = Application.dataPath + "/Data/Exported";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string json = JsonUtility.ToJson(timeline, true);
        string fullPath = Path.Combine(folderPath, "ritual_custom_timeline.json");

        File.WriteAllText(fullPath, json);
        Debug.Log($" Archivo exportado en: {fullPath}");
    }
}


