using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialEndTrigger : MonoBehaviour
{
    public CutsceneData tutorialToRitmoCutscene;

    public void EndTutorial()
    {
        CutsceneLoader.cutsceneToLoad = tutorialToRitmoCutscene;
        SceneManager.LoadScene("CutsceneViewer");
    }
}
