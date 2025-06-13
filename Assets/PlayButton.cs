using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public string sceneToLoad = "MainScene"; // personalizza con il nome corretto

    public void OnPlayClicked()
    {
        GameObject music = GameObject.Find("MenuMusic");
        if (music != null)
        {
            MenuMusicManager musicManager = music.GetComponent<MenuMusicManager>();
            if (musicManager != null)
            {
                musicManager.StopMusic();
            }
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
