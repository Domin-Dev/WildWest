using UnityEngine;
using UnityEngine.SceneManagement;
public class GameBoot : MonoBehaviour
{
    public void Awake()
    {
        SceneManager.LoadScene(Scenes.Game);
        SceneManager.LoadScene(Scenes.Chat,LoadSceneMode.Additive);
    }
}