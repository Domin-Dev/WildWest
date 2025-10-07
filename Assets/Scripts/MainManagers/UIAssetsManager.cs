using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class FontData
{
    public LocalizedString name;
    public TMP_FontAsset font;
}


public class  UIAssetsManager : MonoBehaviour
{

    [Header("Fonts")]
    [SerializeField] public List<FontData> fonts;
    [Header("Materials")]
    [SerializeField] public Material UIHeadMaterial;
    [Header("Rewards")]
    [SerializeField] public Sprite bronzeBackgroundUI;
    [SerializeField] public Sprite silverBackgroundUI;
    [SerializeField] public Sprite goldBackgroundUI;
    [Header("Backgrounds")]
    [SerializeField] public Sprite woodBackgroundUI;
    [SerializeField] public Sprite ironBackgroundUI;
    [Header("Frames")]
    [SerializeField] public Sprite woodenFrameUI;
    [SerializeField] public Sprite blackFrameUI;
    [Header("Selected")]
    [SerializeField] public Sprite selectedWoodBackgroundUI;
    [SerializeField] public Sprite selectedIronBackgroundUI;
    [Header("Pressed")]
    [SerializeField] public Sprite goldPressedBackgroundUI;


    [Space]
    [SerializeField] public Sprite ironBarsUI;



    public static UIAssetsManager instance { private set; get; }

    public string[] GetFontNames()
    {
        List<string> names = new List<string>();
        foreach (var item in fonts)
        {
            names.Add(item.name.GetLocalizedString());
        }
        return names.ToArray();
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ChangeButtonToIron(Button button)
    {
        var states = button.spriteState;
        states.pressedSprite = goldPressedBackgroundUI;

        states.selectedSprite = selectedIronBackgroundUI;
        states.highlightedSprite = selectedIronBackgroundUI;

        button.spriteState = states;
        button.image.sprite = ironBackgroundUI;
    }
    public void ChangeButtonToWood(Button button)
    {
        var states = button.spriteState;
        states.pressedSprite = goldPressedBackgroundUI;

        states.selectedSprite = selectedWoodBackgroundUI;
        states.highlightedSprite = selectedWoodBackgroundUI;

        button.spriteState = states;
        button.image.sprite = woodBackgroundUI;
    }

}

