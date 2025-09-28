using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
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
            Debug.Log("dzia");
            WindowsManager.instance.escScene = -1;
            GameInfo.instance.isInGame = false;
            if (ClientServerBootstrap.HasServerWorld)
            {
                SaveSystem.Save();
                RPCHelper.StopServer(ClientServerBootstrap.ServerWorld);
            }
            else
            {
                var queryNetworkID = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamConnection));
                var array = queryNetworkID.ToEntityArray(AllocatorManager.Temp);
                ClientServerBootstrap.ClientWorld.EntityManager.AddComponent(array[0], typeof(NetworkStreamRequestDisconnect));
                queryNetworkID.Dispose();
                array.Dispose();
            }

            SceneManager.LoadScene(0);
            WindowsManager.instance.SwitchBackground(false);
        });
    }
}
