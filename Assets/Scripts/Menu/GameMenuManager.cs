using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] private Button resume;
    [SerializeField] private Button settinngs;
    [SerializeField] private Button hostSettings;
    [SerializeField] private Button exit;

    private void Awake()
    {
        resume.onClick.AddListener(() => { UIManager.instance.CloseOpenWindows(); });
        settinngs.onClick.AddListener(() =>
        {
            GameInfo.instance.lastLoadedScene = 11;
            UIManager.instance.LoadScene(8, true);
        });
        exit.onClick.AddListener(() => {
            SceneManager.LoadScene(0);
            if (ClientServerBootstrap.HasServerWorld)
            {
                RPCHelper.StopServer(ClientServerBootstrap.ServerWorld);
            }
        });
    }
}
