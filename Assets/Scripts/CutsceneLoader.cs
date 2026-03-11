using UnityEngine;

public static class CutsceneLoader
{
    public static CutsceneData cutsceneToLoad;

    private static CutsceneData debugFallbackCutscene;
    private static Sprite debugFallbackSprite;
    private static Texture2D debugFallbackTexture;

    public static void EnsureDebugFallback(string nextSceneName = "MainMenu")
    {
        if (cutsceneToLoad != null)
        {
            return;
        }

        if (debugFallbackCutscene == null)
        {
            debugFallbackCutscene = ScriptableObject.CreateInstance<CutsceneData>();
            debugFallbackCutscene.hideFlags = HideFlags.HideAndDontSave;

            debugFallbackTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            debugFallbackTexture.hideFlags = HideFlags.HideAndDontSave;
            Color[] pixels =
            {
                new Color(0.12f, 0.12f, 0.12f, 1f), new Color(0.12f, 0.12f, 0.12f, 1f),
                new Color(0.12f, 0.12f, 0.12f, 1f), new Color(0.12f, 0.12f, 0.12f, 1f)
            };
            debugFallbackTexture.SetPixels(pixels);
            debugFallbackTexture.Apply(false, true);

            debugFallbackSprite = Sprite.Create(
                debugFallbackTexture,
                new Rect(0f, 0f, debugFallbackTexture.width, debugFallbackTexture.height),
                new Vector2(0.5f, 0.5f));
            debugFallbackSprite.hideFlags = HideFlags.HideAndDontSave;
        }

        string fallbackNext = string.IsNullOrWhiteSpace(nextSceneName) ? "MainMenu" : nextSceneName;
        debugFallbackCutscene.images = new[] { debugFallbackSprite };
        debugFallbackCutscene.nextSceneName = fallbackNext;
        cutsceneToLoad = debugFallbackCutscene;
    }
}
