using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;




public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private StatUI statsPrefab;
    [SerializeField] private Transform startsParent;
    [SerializeField] private TMP_SpriteAsset spriteAsset;

    private Dictionary<string,StatUI> stats = new Dictionary<string, StatUI>();
    private static PlayerStatsUI instance;


    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Load()
    {
        stats.Clear();
        var values = UIConfig.Instance.Properties.OrderByDescending(x => x.Value.DisplayPriority).ToList();
        foreach(var item in values)
        {
            if(item.Value.DisplayEQWindow)
                LoadStat(item.Value,item.Key);       
        }
    }


    private bool TrySetValue<T>(StatUI statUI,Property property, params T[] values) 
    {
        int spriteIndex = spriteAsset.GetSpriteIndexFromName(property.nameIcon);
        if(spriteIndex < 0)
            return false;
            
        var glyph = spriteAsset.spriteCharacterTable[spriteIndex].glyph as TMP_SpriteGlyph;
        if(glyph == null)
            return false;

        if(property.HideWhenValueIsZero)
        {
            if(values.Length > 0 && EqualityComparer<T>.Default.Equals(values[0], default))
            {
                statUI.gameObject.SetActive(false);
            }
            else
            {
                statUI.gameObject.SetActive(true);
            }
        } 

        statUI.SetUp(StringsHelper.GetPropertyString(property,Color.white,values),property.color);
        return true;
    }
    private void LoadStat(Property property,string name)
    {
        var obj = Instantiate(statsPrefab.gameObject,startsParent).GetComponent<StatUI>(); 
        if(TrySetValue(obj,property,0))
            stats.Add(name,obj);
    }
    public static void UpdateStat<T>(string name,params T[] values)
    {
        if(instance.stats.TryGetValue(name,out var statUI))
        {
            var property = UIConfig.GetProperty(name);
            instance.TrySetValue(statUI,property,values);
        }
    }

}
