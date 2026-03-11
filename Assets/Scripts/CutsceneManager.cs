using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public Image displayImage;
    private CutsceneData cutsceneData;
    private int currentIndex = 0;

    void Start()
    {
        cutsceneData = CutsceneLoader.cutsceneToLoad;

        if (cutsceneData == null || cutsceneData.images.Length == 0)
        {
            Debug.LogError("No cutscene data found!");
            return;
        }

        ShowImage(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            currentIndex++;
            if (currentIndex < cutsceneData.images.Length)
            {
                ShowImage(currentIndex);
            }
            else
            {
                SceneManager.LoadScene(cutsceneData.nextSceneName);
            }
        }
    }

    void ShowImage(int index)
    {
        displayImage.sprite = cutsceneData.images[index];
    }
}
