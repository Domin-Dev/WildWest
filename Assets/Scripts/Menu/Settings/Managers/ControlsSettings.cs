using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;


public class ControlsSettings : Settings
{
    InputActionAsset inputActions;
    public void SetUp(InputActionAsset inputActions, SettingsData data, bool defaultSettings)
    {
        this.inputActions = inputActions;
        base.SetUp(data, defaultSettings);
    }
    public override void SetSettings()
    {

    }

    public void SetDefaultSettings(string inputScheme)
    {
        Debug.Log("resetowaniea!!!");
        foreach(InputActionMap map in inputActions.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                action.RemoveBindingOverride(InputBinding.MaskByGroup(inputScheme));
            }
        }
    }

   
}
