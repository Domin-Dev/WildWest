using TMPro;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public class GamePointer : MonoBehaviour
{ 
    [SerializeField] private TextMeshProUGUI tileInfo;
    private static GamePointer i;
    public void Awake()
    {
        if(i == null || i == this)
        {
            i = this; 
            this.transform.gameObject.SetActive(false);
        } 
        else
            Destroy(gameObject);
    }

    public static void SetPosition(float2 position)
    {
        i.transform.position = new float3(position,0);
    }

    public static void SetTileInfo(BuildingObjects? buildingObjects)
    {
        Debug.Log("new  " + buildingObjects.HasValue);
        if(buildingObjects.HasValue && ItemsAsset.instance.TryGetItem<BuildingItem>(buildingObjects.Value.id,out var item))
            i.tileInfo.text = item.GetBuildingObjectInfo(buildingObjects.Value);  
        else
            i.tileInfo.text = string.Empty;
    }

    public static void Hide()
    {
        i.transform.gameObject.SetActive(false);
    }

    public static void Show()
    {
        i.transform.gameObject.SetActive(true);
    }

    public static void Switch(bool value)
    {
        i.transform.gameObject.SetActive(value);
    }
}