using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class SettingsTab : MonoBehaviour
{
    [SerializeField] public Button startButton;
    [SerializeField] public GameObject startSelectedObject;
    protected bool changed;

    public abstract void ResetToDefault();
    public abstract void SaveSettings();
}

