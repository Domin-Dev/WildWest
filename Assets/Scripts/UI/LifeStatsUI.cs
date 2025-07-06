using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class LifeStatsUI : MonoBehaviour {

    public static LifeStatsUI Instance;

    [SerializeField] private Transform healthBar;
    [SerializeField] private Transform foodBar;
    [SerializeField] private Transform thirstBar;


    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI thirstText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }



    public void UpdateHealth(Health health)
    {
        UpdateHealth(health.Value, health.Max);
    }
    public void UpdateFood(Hunger hunger)
    {
        UpdateFood(hunger.Value, hunger.Max);
    }
    public void UpdateThirst(Thirst thirst)
    {
        UpdateThirst(thirst.Value, thirst.Max);
    }
    public void UpdateHealth(int current, int max)
    {
        Debug.Log("llllllllllllll update  " + current + ", " + max);
        SetBar(healthBar,healthText,(float)current / (float)max);
    }
    public void UpdateFood(int current, int max)
    {
        SetBar(foodBar,foodText, (float)current / (float)max);
    }
    public void UpdateThirst(int current, int max)
    {
        SetBar(thirstBar,thirstText, (float)current / (float)max);
    }
    public void SetBar(Transform bar,TextMeshProUGUI text, float value)
    {
        Vector3 scale = bar.localScale;
        scale.x = value;
        bar.localScale = scale;
        text.text = (value * 100).ToString() + " %";
    }
}
