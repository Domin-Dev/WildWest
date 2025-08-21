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
        resume.onClick.AddListener(() => { WindowsManager.instance.CloseOpenWindows(); });
        settinngs.onClick.AddListener(() =>
        {
            GameInfo.instance.lastLoadedScene = 11;
            WindowsManager.instance.LoadScene(8, true);
        });
        exit.onClick.AddListener(() => {
            WindowsManager.instance.SwitchBackground(false);
            WindowsManager.instance.escScene = -1;
            SaveSystem.Save();
            if (ClientServerBootstrap.HasServerWorld)
            {
                RPCHelper.StopServer(ClientServerBootstrap.ServerWorld);
            }
            SceneManager.LoadScene(0);
        });
    }
}
