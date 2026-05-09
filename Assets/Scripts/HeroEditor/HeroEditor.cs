using System;
using System.Collections.Generic;
using System.Data.Common;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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


    private static Texture2D _emptyTexture;
    public static Texture2D EmptyTexture
    {
        get
        {
            if (_emptyTexture == null)
            {
                _emptyTexture = new Texture2D(1, 1);
                _emptyTexture.SetPixel(0, 0, new Color(0,0,0,0));
                _emptyTexture.Apply();

                _emptyTexture.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            return _emptyTexture;
        }
    }

    
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
        hairSwitch.SetUpSwitch(0, characterEditorSettings.hairstylesTexture.height / 21, "Hairstyle");
        beardSwitch.SetUpSwitch(0, characterEditorSettings.beardTexture.height / 21, "Beard");
        faceDetailsSwitch.SetUpSwitch(0, characterEditorSettings.faceDetailsTexture.height / 21, "Facial details");
        DirectionSwitch.SetUpSwitch(0, 4);
        LoadColors();
    }

    private void LoadColors()
    {
        SetCharacterSpriteProperties();
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
        hairSwitch.OnChangedValue += ChangeHair;
        beardSwitch.OnChangedValue += ChangeBeard;
        faceDetailsSwitch.OnChangedValue += ChangeFaceDetails;
        DirectionSwitch.OnChangedValue += ChangeDirection;
        saveButton.onClick.AddListener(SetPlayerLook);
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

        ChangeHair(0);
        ChangeBeard(0);
        ChangeFaceDetails(0);
        ChangeSkinColor(characterEditorSettings.skinColors[0]);
        ChangeHairColor(characterEditorSettings.hairColors[0]);
        ChangeUnderwearColor(characterEditorSettings.clothesColors[0]);
    } 
    public void ChangeHair(int value)
    {
        SetMaterialInt(head, "_HairIndex", value);
    }
    public void ChangeBeard(int value)
    {
        SetMaterialInt(head, "_BeardIndex", value);
    }
    public void ChangeFaceDetails(int value)
    {
        SetMaterialInt(head,"_PaintingsIndex", value);
    }
    public void ChangeDirection(int value)
    {
        SetMaterialInt(head, "_Direction", dirs[value]);
        SetMaterialInt(body, "_Direction", dirs[value]);
    }
    public static void SetMaterialInt(SpriteRenderer spriteRenderer,string name, int newValue)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetInt(name, newValue);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    public static void SetMaterialColor(SpriteRenderer spriteRenderer, string name, Color value)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor(name, value);     
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    public static void SetMaterialTexture2D(SpriteRenderer spriteRenderer, string name, Texture2D value)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        value = (value == null ? EmptyTexture : value);
        Debug.Log("tekstura to " + value + " " + EmptyTexture);
        materialPropertyBlock.SetTexture(name,value);     
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private int GetMaterialInt(SpriteRenderer spriteRenderer, string name)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        return materialPropertyBlock.GetInt(name);
    }
    private float3 GetMaterialFloat3(SpriteRenderer spriteRenderer, string name)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        Color color = materialPropertyBlock.GetColor(name);
        return new float3(color.r, color.g, color.b);
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
        GameInfo.LoadScene(1,1,0.5f);

        LocalPlayerLook playerLook = new LocalPlayerLook();
        playerLook.characterLook.skinColor = GetMaterialFloat3(head, "_SkinColor");
        playerLook.characterLook.underwearColor = GetMaterialFloat3(body, "_UnderwearColor");
        playerLook.characterLook.hairColor = GetMaterialFloat3(head, "_HairColor");

        playerLook.characterLook.beardndex = GetMaterialInt(head, "_BeardIndex");
        playerLook.characterLook.faceDetailsIndex = GetMaterialInt(head, "_PaintingsIndex");
        playerLook.characterLook.hairIndex = GetMaterialInt(head, "_HairIndex");

        Entity e = ClientServerBootstrap.ClientWorld.EntityManager.CreateEntity();
        ClientServerBootstrap.ClientWorld.EntityManager.AddComponentData(e, playerLook);
    }
}

