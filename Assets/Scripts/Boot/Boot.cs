using UnityEngine;
using UnityEngine.SceneManagement;

public class Boot : MonoBehaviour
{
    public void Awake()
    {
        SceneManager.LoadScene("GameScene");
        SceneManager.LoadScene("ChatScene",LoadSceneMode.Additive);
    }
}
