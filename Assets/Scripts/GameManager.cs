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

    private Vector3 initialCheckpointPos = new Vector3(0f, 0f, 28.6f);
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

        player.transform.position = halfCheckpointPos;

        if (shouldLoadSavedData && SaveSystem.HasSave())
        {
            SaveData data = SaveSystem.LoadGame();
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
        }


        StartCoroutine(ForcePositionUpdate());
    }

    private System.Collections.IEnumerator ForcePositionUpdate()
    {
        yield return new WaitForFixedUpdate();
        if (shouldSpawnAtHalfCheckpoint || (!shouldShowMenuOnStart && shouldLoadSavedData))
        {
            player.transform.position = halfCheckpointPos;
        }
    }

    private void SpawnAtInitialCheckpoint()
    {
        player.transform.position = initialCheckpointPos;
        player.RedKey = false;
        UpdateRedKeyObjectVisibility(false);
    }

    private void UpdateRedKeyObjectVisibility(bool hasRedKey)
    {
        if (redKeyObject != null)
        {
            redKeyObject.SetActive(!hasRedKey);
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

        shouldShowMenuOnStart = false;
        shouldSpawnAtHalfCheckpoint = false;
        shouldLoadSavedData = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void RestartGame()
    {
        Debug.Log("GAME RESTARTED - Ricaricando scena completa");

        if (SaveSystem.HasSave())
        {
            SaveSystem.DeleteSave();
        }

        shouldSpawnAtHalfCheckpoint = false;
        shouldLoadSavedData = false;
        shouldShowMenuOnStart = false;

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
            shouldSpawnAtHalfCheckpoint = true;
            shouldLoadSavedData = true;
            shouldShowMenuOnStart = false;


            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            ShowMenu();
        }
    }

    public void OnPlayerWin()
    {


        if (SaveSystem.HasSave())
        {
            SaveSystem.DeleteSave();
        }
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
            shouldSpawnAtHalfCheckpoint = true;
            shouldLoadSavedData = true;
            shouldShowMenuOnStart = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void DeleteSave()
    {
        SaveSystem.DeleteSave();
        ShowMenu();
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