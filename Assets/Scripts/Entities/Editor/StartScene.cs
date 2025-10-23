
#if UNITY_EDITOR

using UnityEngine.SceneManagement;
using Unity.Entities;

public partial class StartScene : SystemBase
{
    protected override void OnCreate()
    {
        Enabled = false;
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0)) return;
        SceneManager.LoadScene(0);
    }
    protected override void OnUpdate()
    {
    }
}

#endif