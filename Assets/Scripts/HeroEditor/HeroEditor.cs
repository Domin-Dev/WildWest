using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HeroEditor: MonoBehaviour
{
 
    [SerializeField] private Material colorSwapMaterial;
    [SerializeField] private GameObject colorToSelectPrefab;
    [Space]

    [SerializeField] private Transform UIEditor;
    [SerializeField] private Transform skinColors;
    [SerializeField] private Transform hairColors;
    [SerializeField] private Transform underwearColors;
    [Space]
    [SerializeField] private Switch hairSwitch; 
    [SerializeField] private Switch beardSwitch;
    [SerializeField] private Switch faceDetailsSwitch;
    [SerializeField] private BaseSwitch DirectionSwitch;
    [Space]

    [SerializeField] private Button saveButton;

    [SerializeField] private Sprite selected;
    [SerializeField] private Sprite unselected;

    private CharacterEditorSettings characterEditorSettings;

    [SerializeField] private SpriteRenderer head;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer hand1;
    [SerializeField] private SpriteRenderer hand2;

    private static int[] dirs = {0,2,1,3};

    private Image skinColorSelected;
    private Image hairColorSelected;
    private Image underwearColorSelected;


    public static HeroEditor instance { private set; get; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        characterEditorSettings = Resources.Load<CharacterEditorSettings>("CharacterParts/CharacterEditorSettings");
    }
    private void Start()
    {
        saveButton.onClick.AddListener(() =>
        {
            UIEditor.gameObject.SetActive(false);
        });

        hairSwitch.SetUpSwitch(0, characterEditorSettings.hairstylesTexture.height / 21, "Hairstyle");
        beardSwitch.SetUpSwitch(0, characterEditorSettings.beardTexture.height / 21, "Beard");
        faceDetailsSwitch.SetUpSwitch(0, characterEditorSettings.faceDetailsTexture.height / 21, "Facial details");
        DirectionSwitch.SetUpSwitch(0,4);
        LoadColors();
    }
    private void LoadColors()
    {
        for (int i = 0; i < characterEditorSettings.skinColors.Length; i++)
        {
            Transform transform = Instantiate(colorToSelectPrefab, skinColors).transform;
            transform.GetComponent<Image>().color = characterEditorSettings.skinColors[i];
            transform.GetComponent<Button>().onClick.AddListener(() => 
            {
                Image selectedColor = transform.GetComponent<Image>();
                ChangeSkinColor(selectedColor.color);
                SelectNew(selectedColor, ref skinColorSelected);
            });
        }

        for (int i = 0; i < characterEditorSettings.hairColors.Length; i++)
        {
            Transform transform = Instantiate(colorToSelectPrefab,hairColors).transform;
            transform.GetComponent<Image>().color = characterEditorSettings.hairColors[i];
            transform.GetComponent<Button>().onClick.AddListener(() => 
            {
                Image selectedColor = transform.GetComponent<Image>();
                ChangeHairColor(selectedColor.color);
                SelectNew(selectedColor, ref hairColorSelected);
            });
        }

        for (int i = 0; i < characterEditorSettings.clothesColors.Length; i++)
        {
            Transform transform = Instantiate(colorToSelectPrefab, underwearColors).transform;
            transform.GetComponent<Image>().color = characterEditorSettings.clothesColors[i];
            transform.GetComponent<Button>().onClick.AddListener(() =>
            {
                Image selectedColor = transform.GetComponent<Image>();
                ChangeUnderwearColor(selectedColor.color);
                SelectNew(selectedColor, ref underwearColorSelected);
            });
        }

   //     SetCharacterSpriteProperties(player, 1, 1, 1, 1);
        hairSwitch.OnChangedValue += ChangeHair;
        beardSwitch.OnChangedValue += ChangeBeard;
        faceDetailsSwitch.OnChangedValue += ChangeFaceDetails;
        DirectionSwitch.OnChangedValue += ChangeDirection;
    }
    private void SelectNew(Image newSelected,ref Image currentSelected)
    {
        if(currentSelected != null)
        {
            currentSelected.sprite = unselected;
        }
        Image border = newSelected.transform.GetChild(0).GetComponent<Image>();
        currentSelected = border;
        border.sprite = selected;
    }
   
    public void SetCharacterSpriteProperties()
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        head.SetPropertyBlock(materialPropertyBlock);
        body.SetPropertyBlock(materialPropertyBlock);
        hand1.SetPropertyBlock(materialPropertyBlock);
        hand2.SetPropertyBlock(materialPropertyBlock);
    }
  
    public void ChangeHair(object sender,int value)
    {
        SetMaterialInt(head, "_HairIndex", value);
    }
    public void ChangeBeard(object sender, int value)
    {
        SetMaterialInt(head, "_BeardIndex", value);
    }
    public void ChangeFaceDetails(object sender, int value)
    {
        SetMaterialInt(head,"_PaintingsIndex", value);
    }
    public void ChangeDirection(object sender, int value)
    {
        SetMaterialInt(head, "_Direction", dirs[value]);
        SetMaterialInt(body, "_Direction", dirs[value]);
    }

    private void SetMaterialInt(SpriteRenderer spriteRenderer,string name, int newValue)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetInt(name, newValue);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    private void SetMaterialColor(SpriteRenderer spriteRenderer, string name, Color value)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor(name, value);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    public void ChangeUnderwearColor(Color color)
    {
        SetMaterialColor(body, "_UnderwearColor", color);
    }
    public void ChangeSkinColor(Color color)
    {
        SetMaterialColor(head, "_SkinColor", color);
        SetMaterialColor(body, "_SkinColor", color);
        SetMaterialColor(hand1, "_Color", color);
        SetMaterialColor(hand2, "_Color", color);
    }
    private void ChangeHairColor(Color color)
    {
        SetMaterialColor(head, "_HairColor",color);
    }

    private void SetPlayerLook()
    {
        
    }
}

