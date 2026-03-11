using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyAnimator : MonoBehaviour
{
    public Image enemyImage;

    [Header("Sprites del enemigo (idle)")]
    public Sprite[] idleAnimationFrames;
    public float idleFrameDuration = 0.2f;

    [Header("Sprites de baile (1 por dirección)")]
    public Sprite leftSprite;
    public Sprite downSprite;
    public Sprite upSprite;
    public Sprite rightSprite;

    [Header("Parámetros de danza")]
    public float danceInterval = 1.5f; // cada cuánto tiempo baila
    public float danceDuration = 0.3f;

    private Coroutine idleRoutine;
    private Coroutine danceRoutine;

    void Start()
    {
        StartIdleLoop();
        StartCoroutine(DanceLoop());
    }

    void StartIdleLoop()
    {
        if (idleRoutine != null) StopCoroutine(idleRoutine);
        idleRoutine = StartCoroutine(IdleLoop());
    }

    void StopIdleLoop()
    {
        if (idleRoutine != null) StopCoroutine(idleRoutine);
    }

    IEnumerator IdleLoop()
    {
        int frame = 0;
        while (true)
        {
            if (idleAnimationFrames.Length > 0)
                enemyImage.sprite = idleAnimationFrames[frame];

            frame = (frame + 1) % idleAnimationFrames.Length;
            yield return new WaitForSeconds(idleFrameDuration);
        }
    }

    IEnumerator DanceLoop()
    {
        string[] directions = { "Left", "Down", "Up", "Right" };

        while (true)
        {
            yield return new WaitForSeconds(danceInterval);

            string randomDir = directions[Random.Range(0, directions.Length)];
            Sprite chosen = GetDirectionSprite(randomDir);

            StopIdleLoop(); // Detiene la animación idle
            enemyImage.sprite = chosen;

            yield return new WaitForSeconds(danceDuration);

            StartIdleLoop(); // Reanuda la animación idle
        }
    }

    Sprite GetDirectionSprite(string dir)
    {
        return dir switch
        {
            "Left" => leftSprite,
            "Down" => downSprite,
            "Up" => upSprite,
            "Right" => rightSprite,
            _ => idleAnimationFrames.Length > 0 ? idleAnimationFrames[0] : null
        };
    }
}
