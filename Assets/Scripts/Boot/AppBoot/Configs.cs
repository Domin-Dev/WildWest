using UnityEngine;
using UnityEngine.SceneManagement;
using EditorAttributes;
using System;
using UnityEditor;
using System.Linq;

public class Configs : MonoBehaviour
{
    [Validate("Missing configurations", nameof(CheckConfigs), buildKiller: true, applyToCollection: true)]
    [SerializeField] private ConfigCore[] configs;
    private static Configs _configs;

    public ConfigCore[] GetConfigs()
    {
        if(_configs == null)
        {
            DontDestroyOnLoad(gameObject);
            _configs = this;
        }
        return configs;
    }

#if UNITY_EDITOR
    public bool CheckConfigs()
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