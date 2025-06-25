using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameState { MainMenu, Playing, Paused, EndGame }

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject panelMainMenu;
    public GameObject panelEndGame;
    public GameObject hud;              // tu panel HUD (huda)

    [Header("HUD Elements")]
    public TMP_Text ammoText;              // texto que muestras en la esquina superior
    // no necesitamos aquí el reloadingText porque ese irá en world-space

    public GameState State { get; private set; }

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ShowMainMenu();
    }

    void Update()
    {
        if (State == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        State = GameState.MainMenu;
        Time.timeScale = 0f;
        panelMainMenu.SetActive(true);
        panelEndGame.SetActive(false);
        hud.SetActive(false);
    }

    public void StartGame()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        panelMainMenu.SetActive(false);
        panelEndGame.SetActive(false);
        hud.SetActive(true);
    }

    public void ShowEndGame()
    {
        State = GameState.EndGame;
        Time.timeScale = 0f;
        panelMainMenu.SetActive(false);
        panelEndGame.SetActive(true);
        hud.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Restaurar estado y eliminar el viejo UIManager si persiste
        Instance = null;
        Destroy(gameObject); // elimina este singleton

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // ----------------------------------------------------------------
    // Los dos métodos que necesitamos para el contador de balas
    // ----------------------------------------------------------------

    /// <summary> Muestra/oculta el texto de balas </summary>
    public void ShowAmmoUI(bool show)
    {
        if (ammoText != null)
            ammoText.gameObject.SetActive(show);
    }

    /// <summary> Actualiza el texto con el recuento actual/max </summary>
    public void UpdateAmmoCount(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = $"{current} / {max}";
    }
}
