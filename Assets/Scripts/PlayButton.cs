using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public string sceneToLoad = "MainScene";

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
