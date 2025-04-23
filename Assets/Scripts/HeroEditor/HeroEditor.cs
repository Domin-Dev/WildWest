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
    [Space]

    [SerializeField] private Button saveButton;

    [SerializeField] private Sprite selected;
    [SerializeField] private Sprite unselected;

    private CharacterEditorSettings characterEditorSettings;

    [SerializeField] private SpriteRenderer head;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer hand1;
    [SerializeField] private SpriteRenderer hand2;



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
             //   ChangeUnderwearColor(player, selectedColor.color);
                SelectNew(selectedColor, ref underwearColorSelected);
            });
        }

   //     SetCharacterSpriteProperties(player, 1, 1, 1, 1);
        hairSwitch.OnChangedValue += ChangeHair;
        beardSwitch.OnChangedValue += ChangeBeard;
        faceDetailsSwitch.OnChangedValue += ChangeFaceDetails;
    }

    
    private void ChangeHair(object sender, SwitchArgs e)
    {
        ChangeHair(e.newValue);
    }

    private void ChangeBeard(object sender, SwitchArgs e)
    {
        ChangeBeard(e.newValue);
    }

    private void ChangeFaceDetails(object sender, SwitchArgs e)
    {
        ChangeFaceDetails(e.newValue);
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



    public void SetCharacterSpriteProperties(int hair,int hairColor,int underwearColor,int skinColor)
    {
     //   ChangeHair(hair);
     //   ChangeHairColor(characterEditorSettings.hairColors[hairColor]);
     //   ChangeUnderwearColor(characterEditorSettings.clothesColors[underwearColor]);
     //   ChangeSkinColor(characterEditorSettings.skinColors[skinColor]);
    }
    public void ChangeHair(int value)
    {
        Debug.Log(value);
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetInt("_HairIndex", value);
        head.SetPropertyBlock(materialPropertyBlock);
    }
    public void ChangeBeard(int value)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetInt("_BeardIndex", value);
        head.SetPropertyBlock(materialPropertyBlock);
    }

    public void ChangeFaceDetails(int value)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetInt("_PaintingsIndex", value);
        head.SetPropertyBlock(materialPropertyBlock);
    }

    public void ChangeUnderwearColor(Color color)
    {
        //ChangeColor(characterSpriteController.underwear, color);
    }
    public void ChangeSkinColor(Color color)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_SkinColor", color);
        head.SetPropertyBlock(materialPropertyBlock);
        body.SetPropertyBlock(materialPropertyBlock);

        materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_Color", color);
        hand1.SetPropertyBlock(materialPropertyBlock);
        hand2.SetPropertyBlock(materialPropertyBlock);
    }

    private void ChangeHairColor(Color color)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_HairColor", color);
        head.SetPropertyBlock(materialPropertyBlock);
    }
    private void ChangeColor(SpriteRenderer spriteRenderer,Color color,float darkValue = 1f)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_Color", color);
        materialPropertyBlock.SetTexture("_MainTex", spriteRenderer.sprite.texture);
        materialPropertyBlock.SetFloat("_DarkValue", darkValue);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }



}

