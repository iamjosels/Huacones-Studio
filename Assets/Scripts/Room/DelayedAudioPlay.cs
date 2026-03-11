using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DelayedAudioPlay : MonoBehaviour
{
    [Min(0f)]
    public float delaySeconds = 0.1f;

    private IEnumerator Start()
    {
        // Defer audio playback until after scene activation to avoid load hitching.
        yield return null;

        if (delaySeconds > 0f)
        {
            yield return new WaitForSeconds(delaySeconds);
        }

        AudioSource source = GetComponent<AudioSource>();
        if (source != null && source.clip != null && !source.isPlaying)
        {
            source.Play();
        }
    }
}
