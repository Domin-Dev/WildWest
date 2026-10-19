using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using EditorAttributes;
using System;


public class AppBoot : MonoBehaviour
{
    [Validate("Missing configurations", nameof(CheckConfigs), buildKiller: true, applyToCollection: true)]
    [SerializeField] private ConfigCore[] configs;
    private void Awake()
    {
#if UNITY_EDITOR 
        CheckConfigs();
#endif
        foreach (ConfigCore config in configs)
            if (config != null) config.SetUp();
        SceneManager.LoadScene(Scenes.Menu);
    }

#if UNITY_EDITOR
    private bool CheckConfigs()
    {
        var allTypes = TypeCache
            .GetTypesDerivedFrom<ConfigCore>()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                !type.ContainsGenericParameters)
            .ToArray();

        var configuredTypes = (configs ?? Array.Empty<ConfigCore>()).Where(config => config != null)
            .Select(config => config.GetType()).ToHashSet();

        var missingTypes = allTypes
            .Where(type => !configuredTypes.Contains(type)).ToArray();

        if (missingTypes.Length > 0)
        {
            Debug.LogError(
                $"Missing configurations:\n" +
                string.Join("\n", missingTypes.Select(type => type.FullName)),
                this);
            return true;
        }
        return false;
    }
#endif
}