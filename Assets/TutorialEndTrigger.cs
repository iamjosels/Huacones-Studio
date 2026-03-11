using UnityEngine;

public class TutorialEndTrigger : MonoBehaviour
{
    public CutsceneData tutorialToRitmoCutscene;

    public void EndTutorial()
    {
        CutsceneLoader.cutsceneToLoad = tutorialToRitmoCutscene;
        SceneTransitionManager.EnsureInstance().LoadSceneSafe("CutsceneViewer");
    }
}
