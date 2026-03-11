using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

public class BeatManager : MonoBehaviour
{
    public AudioSource musicSource;
    public TextAsset noteTimelineJSON; // referencia al archivo JSON
    public NoteManager noteManager;    // referencia al spawner
    public float spawnAnticipation = 2f; // segundos antes del impacto

    private NoteTimeline timeline;
    private float songStartTime;
    private int nextNoteIndex = 0;

    void Start()
    {
        timeline = JsonUtility.FromJson<NoteTimeline>(noteTimelineJSON.text);
        songStartTime = Time.time;

        musicSource.Play(); // empieza música
    }

    void Update()
    {
        float songTime = Time.time - songStartTime;

        while (nextNoteIndex < timeline.notes.Count &&
               timeline.notes[nextNoteIndex].time - spawnAnticipation <= songTime)
        {
            var note = timeline.notes[nextNoteIndex];
            noteManager.SpawnNote(note.type);
            nextNoteIndex++;
        }
    }
}


