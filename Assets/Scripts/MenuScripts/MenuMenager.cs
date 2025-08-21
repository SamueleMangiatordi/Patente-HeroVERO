using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject mainMenu;      // Il contenitore Menu
    public GameObject settingsMenu;  // Il contenitore SettingsMenu

    // Mostra il menu principale
    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    // Mostra il menu impostazioni
    public void ShowSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }
}
