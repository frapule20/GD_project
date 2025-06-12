using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Menu UI")]
    public GameObject menuPanel;
    public Button quitButton;
    public Button restartButton;
    public Button reloadButton;
    public Button deleteSaveButton;

    [Header("Checkpoint Objects")]
    public Transform initialCheckpoint;
    public Transform halfCheckpoint;
    public Transform finalCheckpoint;

    [Header("Player Reference")]
    public PlayerController player;

    [Header("Camera Reference")]
    public CameraController cameraController;

    [Header("RedKey Object")]
    public GameObject redKeyObject;

    private Vector3 initialCheckpointPos = new Vector3(0f, 0.05f, 1.88f);
    private Vector3 halfCheckpointPos = new Vector3(-0.082f, 1.850f, -11.204f);

    public static GameManager Instance;

    private bool gameStarted = false;
    private bool isMenuActive = false;

    public static bool shouldSpawnAtHalfCheckpoint = false;
    public static bool shouldLoadSavedData = false;
    public static bool shouldShowMenuOnStart = true;
    public static bool isFirstGameStart = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (menuPanel != null && (!shouldShowMenuOnStart || shouldSpawnAtHalfCheckpoint))
        {
            menuPanel.SetActive(false);
        }
    }

    private void Start()
    {
        SetupButtons();
        
        // Nascondi immediatamente il menu se non dovrebbe essere mostrato
        if (!shouldShowMenuOnStart || shouldSpawnAtHalfCheckpoint)
        {
            menuPanel.SetActive(false);
            isMenuActive = false;
            Time.timeScale = 1f;
        }
        
        // Attendi un frame prima di gestire lo spawn per assicurarti che tutto sia inizializzato
        StartCoroutine(HandleSceneStartCoroutine());
    }

    private System.Collections.IEnumerator HandleSceneStartCoroutine()
    {
        yield return new WaitForEndOfFrame();
        HandleSceneStart();
    }

    private void HandleSceneStart()
    {
        
        if (shouldSpawnAtHalfCheckpoint)
        {
            Debug.Log("Spawning at half checkpoint...");
            SpawnAtHalfCheckpoint();
            HideMenu(); 
            StartGameplay();
        }
        else if (!shouldShowMenuOnStart && !isFirstGameStart)
        {
            Debug.Log("Spawning at initial checkpoint...");
            SpawnAtInitialCheckpoint();
            HideMenu();
            StartGameplay();
        }
        else
        {
            Debug.Log("Showing menu...");
            menuPanel.SetActive(true);
            ShowMenu();
        }

        ResetStaticVariables();
    }

    private void SpawnAtHalfCheckpoint()
    {
        
        // Forza la posizione del player prima di tutto
        player.transform.position = halfCheckpointPos;
        
        if (shouldLoadSavedData && SaveSystem.HasSave())
        {
            SaveData data = SaveSystem.LoadGame();
            // Assicurati che la posizione sia impostata dopo il caricamento
            player.transform.position = halfCheckpointPos;
            player.RedKey = data.hasRedKey;

            UpdateRedKeyObjectVisibility(data.hasRedKey);
            
            if (cameraController != null)
            {
                cameraController.transform.position = new Vector3(1.28f, 2.14f, -7.50f);
            }

            Debug.Log($"Player spawnnato al half checkpoint con save data: {halfCheckpointPos} con RedKey: {data.hasRedKey}");
        }
        else
        {
            player.RedKey = false;
            Debug.Log($"Player spawnnato al half checkpoint senza save data: {halfCheckpointPos}");
        }
        
        Debug.Log($"Player position dopo spawn: {player.transform.position}");
        
        // Forza un ulteriore aggiornamento della posizione
        StartCoroutine(ForcePositionUpdate());
    }
    
    private System.Collections.IEnumerator ForcePositionUpdate()
    {
        yield return new WaitForFixedUpdate();
        if (shouldSpawnAtHalfCheckpoint || (!shouldShowMenuOnStart && shouldLoadSavedData))
        {
            player.transform.position = halfCheckpointPos;
            Debug.Log($"FORCE UPDATE - Player position: {player.transform.position}");
        }
    }

    private void SpawnAtInitialCheckpoint()
    {
        player.transform.position = initialCheckpointPos;
        player.RedKey = false;
        UpdateRedKeyObjectVisibility(false);
        Debug.Log($"Player spawnnato al checkpoint iniziale: {initialCheckpointPos}");
        Debug.Log($"Player position dopo spawn iniziale: {player.transform.position}");
    }

    private void UpdateRedKeyObjectVisibility(bool hasRedKey)
    {
        if (redKeyObject != null)
        {
            redKeyObject.SetActive(!hasRedKey);
            Debug.Log($"RedKey object visibility updated: {!hasRedKey} (player has RedKey: {hasRedKey})");
        }
        else
        {
            Debug.LogWarning("RedKey object non è assegnato nel GameManager!");
        }
    }
    public void OnRedKeyCollected()
    {
        UpdateRedKeyObjectVisibility(true);
    }
    
    private void StartGameplay()
    {
        gameStarted = true;

        if (cameraController != null)
        {
            cameraController.InitializeCameraForGame();
        }
    }

    private void ResetStaticVariables()
    {
        Debug.Log("Resetting static variables...");
        shouldSpawnAtHalfCheckpoint = false;
        shouldLoadSavedData = false;
        shouldShowMenuOnStart = true;
        isFirstGameStart = false;
    }

    private void SetupButtons()
    {
        quitButton.onClick.AddListener(QuitGame);

        restartButton.onClick.AddListener(() => {
            if (gameStarted)
                RestartGame();
            else
                StartGame();
        });

        reloadButton.onClick.AddListener(ReloadGame);
        deleteSaveButton.onClick.AddListener(DeleteSave);
    }

    private void StartGame()
    {
        Debug.Log("STARTING NEW GAME");
        
        if (SaveSystem.HasSave())
        {
            SaveSystem.DeleteSave();
        }

        // Imposta le variabili per iniziare un nuovo gioco
        shouldShowMenuOnStart = false;
        shouldSpawnAtHalfCheckpoint = false;
        shouldLoadSavedData = false;

        // Ricarica la scena per un avvio pulito
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void RestartGame()
    {
        Debug.Log("GAME RESTARTED - Ricaricando scena completa");

        // Elimina eventuali salvataggi
        if (SaveSystem.HasSave())
        {
            SaveSystem.DeleteSave();
        }

        // Imposta le variabili per il restart (spawn iniziale senza menu)
        shouldSpawnAtHalfCheckpoint = false;
        shouldLoadSavedData = false;
        shouldShowMenuOnStart = false;

        // Ricarica la scena completamente
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        isMenuActive = true;
        Time.timeScale = 0f;

        SetCursorState(true, CursorLockMode.None);

        bool hasSave = SaveSystem.HasSave();
        reloadButton.interactable = hasSave;
        deleteSaveButton.interactable = hasSave;
    }

    public void HideMenu()
    {
        menuPanel.SetActive(false);
        isMenuActive = false;
        Time.timeScale = 1f;
        SetCursorState(false, CursorLockMode.Locked);
    }

    public bool IsMenuActive()
    {
        return isMenuActive;
    }

    private void SetCursorState(bool visible, CursorLockMode lockMode)
    {
        Cursor.visible = visible;
        Cursor.lockState = lockMode;
    }

    public void OnPlayerDeath()
    {
        
        if (SaveSystem.HasSave())
        {
            // Ricarica dalla posizione del half checkpoint senza mostrare il menu
            shouldSpawnAtHalfCheckpoint = true;
            shouldLoadSavedData = true;
            shouldShowMenuOnStart = false;
            
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Nessun salvataggio, mostra menu
            ShowMenu();
        }
    }

    public void OnPlayerWin()
    {

        
        if (SaveSystem.HasSave())
        {
            SaveSystem.DeleteSave();
        }

        // Ricarica la scena dall'inizio e mostra il menu
        shouldSpawnAtHalfCheckpoint = false;
        shouldLoadSavedData = false;
        shouldShowMenuOnStart = true;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnHalfCheckpointReached()
    {
        Debug.Log("Half checkpoint raggiunto - Salvando gioco");
        SaveSystem.SaveGame(player.RedKey);
    }

    private void QuitGame()
    {
        Debug.Log("Game is quitting...");
        SaveSystem.DeleteSave();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void ReloadGame()
    {
        
        if (SaveSystem.HasSave())
        {
            // Imposta le variabili per il reload (spawn al half checkpoint senza menu)
            shouldSpawnAtHalfCheckpoint = true;
            shouldLoadSavedData = true;
            shouldShowMenuOnStart = false;

            // Ricarica la scena completamente
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void DeleteSave()
    {
        SaveSystem.DeleteSave();
        ShowMenu(); // Aggiorna il menu per disabilitare i bottoni
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isMenuActive)
                HideMenu();
            else
                ShowMenu();
        }
    }
}