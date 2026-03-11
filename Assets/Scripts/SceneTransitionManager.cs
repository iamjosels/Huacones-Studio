using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager I { get; private set; }

    private bool isLoading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static SceneTransitionManager EnsureInstance()
    {
        if (I != null)
        {
            return I;
        }

        GameObject transitionObject = new GameObject(nameof(SceneTransitionManager));
        I = transitionObject.AddComponent<SceneTransitionManager>();
        DontDestroyOnLoad(transitionObject);
        return I;
    }

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadSceneSafe(string sceneName)
    {
        if (isLoading || string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void LoadSceneSafe(int buildIndex)
    {
        if (isLoading)
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(buildIndex));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;
        ThreadPriority previousPriority = Application.backgroundLoadingPriority;
        Application.backgroundLoadingPriority = ThreadPriority.High;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (operation == null)
        {
            Application.backgroundLoadingPriority = previousPriority;
            isLoading = false;
            yield break;
        }

        operation.priority = 100;

        while (!operation.isDone)
        {
            yield return null;
        }

        Application.backgroundLoadingPriority = previousPriority;
        isLoading = false;
    }

    private IEnumerator LoadSceneRoutine(int buildIndex)
    {
        isLoading = true;
        ThreadPriority previousPriority = Application.backgroundLoadingPriority;
        Application.backgroundLoadingPriority = ThreadPriority.High;

        AsyncOperation operation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        if (operation == null)
        {
            Application.backgroundLoadingPriority = previousPriority;
            isLoading = false;
            yield break;
        }

        operation.priority = 100;

        while (!operation.isDone)
        {
            yield return null;
        }

        Application.backgroundLoadingPriority = previousPriority;
        isLoading = false;
    }
}
