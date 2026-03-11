using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Nombres de escenas")]
    public string escenaJuego = "EscenaSeleccion";
    public string escenaTutorial = "Tutorial"; // aún puedes usarlo en el ScriptableObject

    [Header("Paneles del menú")]
    public GameObject panelExtras;

    [Header("Datos de cutscene")]
    public CutsceneData menuToTutorialCutscene;

    public void Jugar()
    {
        SceneManager.LoadScene(escenaJuego);
    }

    public void MostrarExtras()
    {
        panelExtras.SetActive(true);
    }

    public void MostrarTutorial()
    {
        CutsceneLoader.cutsceneToLoad = menuToTutorialCutscene;
        SceneManager.LoadScene("CutsceneViewer");
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Salir (solo funciona en build)");
    }

    public void CerrarPanel(GameObject panel)
    {
        panel.SetActive(false);
    }
}
