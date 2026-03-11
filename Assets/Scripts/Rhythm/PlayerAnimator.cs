using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerAnimator : MonoBehaviour
{
    public Image characterImage;

    [Header("Sprites animados de idle")]
    public Sprite[] idleAnimationFrames;
    public float idleFrameDuration = 0.2f;

    [Header("Sprites únicos")]
    public Sprite leftSprite;
    public Sprite downSprite;
    public Sprite upSprite;
    public Sprite rightSprite;
    public Sprite failSprite;

    [Header("Duración del gesto")]
    public float reactionDuration = 0.3f;

    private Coroutine revertCoroutine;
    private Coroutine idleCoroutine;

    void Start()
    {
        StartIdleLoop();
    }

    public void PlayDirection(string direction)
    {
        StopIdleLoop();

        Sprite newSprite = direction switch
        {
            "Left" => leftSprite,
            "Down" => downSprite,
            "Up" => upSprite,
            "Right" => rightSprite,
            _ => null
        };

        if (newSprite != null)
        {
            characterImage.sprite = newSprite;

            if (revertCoroutine != null) StopCoroutine(revertCoroutine);
            revertCoroutine = StartCoroutine(RevertToIdleAfterDelay());
        }
    }

    public void PlayFail()
    {
        StopIdleLoop();

        characterImage.sprite = failSprite;

        if (revertCoroutine != null) StopCoroutine(revertCoroutine);
        revertCoroutine = StartCoroutine(RevertToIdleAfterDelay());
    }

    private IEnumerator RevertToIdleAfterDelay()
    {
        yield return new WaitForSeconds(reactionDuration);
        StartIdleLoop();
    }

    private void StartIdleLoop()
    {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        idleCoroutine = StartCoroutine(IdleAnimationLoop());
    }

    private void StopIdleLoop()
    {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
    }

    private IEnumerator IdleAnimationLoop()
    {
        int frame = 0;
        while (true)
        {
            if (idleAnimationFrames.Length > 0)
                characterImage.sprite = idleAnimationFrames[frame];

            frame = (frame + 1) % idleAnimationFrames.Length;
            yield return new WaitForSeconds(idleFrameDuration);
        }
    }
}
