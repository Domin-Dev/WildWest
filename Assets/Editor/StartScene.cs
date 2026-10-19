
#if UNITY_EDITOR

using UnityEngine.SceneManagement;
using Unity.Entities;

public partial class StartScene : SystemBase
{
    protected override void OnCreate()
    {
        Enabled = false;
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName(Scenes.AppBoot)) return;
        SceneManager.LoadScene(Scenes.AppBoot);
    }
    protected override void OnUpdate() {}
}

#endif