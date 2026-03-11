using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public Image displayImage;
    private CutsceneData cutsceneData;
    private int currentIndex = 0;
    private AsyncOperation preloadedNextScene;
    private bool isTransitioning;
    private bool hasStartedPreload;

    void Start()
    {
        cutsceneData = CutsceneLoader.cutsceneToLoad;
        CutsceneLoader.cutsceneToLoad = null;

        if (cutsceneData == null)
        {
            CutsceneLoader.EnsureDebugFallback("MainMenu");
            cutsceneData = CutsceneLoader.cutsceneToLoad;
        }

        if (cutsceneData == null)
        {
            Debug.LogWarning("Cutscene data is missing. Returning to MainMenu.");
            SceneTransitionManager.EnsureInstance().LoadSceneSafe("MainMenu");
            return;
        }

        if (cutsceneData.images == null || cutsceneData.images.Length == 0)
        {
            Debug.LogWarning("Cutscene has no images. Loading next scene directly.");
            LoadNextScene();
            return;
        }

        ShowImage(0);

        // For short cutscenes, start preloading immediately to speed up response.
        if (!hasStartedPreload && cutsceneData.images.Length <= 2)
        {
            hasStartedPreload = true;
            StartCoroutine(PreloadNextScene());
        }
    }

    void Update()
    {
        if (cutsceneData == null || isTransitioning)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            currentIndex++;
            if (currentIndex < cutsceneData.images.Length)
            {
                ShowImage(currentIndex);
            }
            else
            {
                LoadNextScene();
            }
        }
    }

    void ShowImage(int index)
    {
        displayImage.sprite = cutsceneData.images[index];

        if (!hasStartedPreload && cutsceneData.images.Length > 1 && index == cutsceneData.images.Length - 1)
        {
            hasStartedPreload = true;
            StartCoroutine(PreloadNextScene());
        }
    }

    private IEnumerator PreloadNextScene()
    {
        string nextScene = cutsceneData.nextSceneName;
        if (string.IsNullOrWhiteSpace(nextScene))
        {
            yield break;
        }

        preloadedNextScene = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
        if (preloadedNextScene == null)
        {
            yield break;
        }

        preloadedNextScene.priority = 100;
        preloadedNextScene.allowSceneActivation = false;

        while (preloadedNextScene.progress < 0.9f)
        {
            yield return null;
        }
    }

    private void LoadNextScene()
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;

        if (preloadedNextScene != null)
        {
            preloadedNextScene.allowSceneActivation = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(cutsceneData.nextSceneName))
        {
            SceneTransitionManager.EnsureInstance().LoadSceneSafe("MainMenu");
            return;
        }

        SceneTransitionManager.EnsureInstance().LoadSceneSafe(cutsceneData.nextSceneName);
    }
}
