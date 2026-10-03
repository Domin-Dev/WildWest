using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldRow : MonoBehaviour
{
    [SerializeField] private Button play;
    [SerializeField] private Button edit;
    [SerializeField] private Button remove;

    [SerializeField] private TextMeshProUGUI worldNameText;
    [SerializeField] private Image headIcon;
    [SerializeField] private Image cover;
    [SerializeField] private TextMeshProUGUI saveTime;
    [SerializeField] private TextMeshProUGUI difficulty;
    [SerializeField] private TextMeshProUGUI playTime;




    private string worldName;
    public void SetWorld(HeaderSave header, PlayerSave playerSave, Material materialIcon)
    {
        Sprite background = null;
        Sprite coverSprite = null;

        worldName = header.worldName.ToString();
        worldNameText.text = worldName;


        
        remove.onClick.AddListener(() => MenuManager.instance.Confirmation(worldName));
        edit.onClick.AddListener(() => MenuManager.instance.Edit(worldName));
        play.onClick.AddListener(() => MenuManager.instance.Load(worldName));



        saveTime.text = DateTimeOffset.FromUnixTimeSeconds(header.saveTime).DateTime.ToLocalTime().ToString();

        Material mat = new Material(materialIcon);
        headIcon.material = mat;
        
        mat.SetColor("_SkinColor", MyTools.GetColorFromFloat3(playerSave.characterLook.skinColor));
        mat.SetColor("_HairColor", MyTools.GetColorFromFloat3(playerSave.characterLook.hairColor));
        mat.SetInt("_HairIndex", playerSave.characterLook.hairIndex);
        mat.SetInt("_BeardIndex", playerSave.characterLook.beardndex);
        mat.SetInt("_PaintingsIndex", playerSave.characterLook.faceDetailsIndex);

        difficulty.text = header.difficulty.ToString();

        long m = ((long)header.playTime / 60 % 60);
        long h = (long)header.playTime / 3600;
        playTime.text = $"{h}h {m}m";


        headIcon.SetMaterialDirty();
        if (playerSave.characterLook.faceDetailsIndex == 9)
        {
            coverSprite = UIAssetsManager.instance.ironBarsUI;
            background = UIAssetsManager.instance.blackFrameUI;
        }
        CheckBadges(h, ref background, coverSprite);

        if (background != null) SetBackground(background);
        if (coverSprite != null)
        {
            cover.gameObject.SetActive(true);
            cover.sprite = coverSprite;
        }
    }

    public void RowSelected()
    {
        GetComponent<Image>().sprite = UIAssetsManager.instance.selectedWoodBackgroundUI;
    }
    public void RowDeselected()
    {
        GetComponent<Image>().sprite = UIAssetsManager.instance.woodBackgroundUI;
    }

    private Color GetColor(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return new Color(0.102f, 0.478f, 0.243f, 1f);
            case Difficulty.Normal:
                return new Color(0.733f, 0.545f, 0.071f, 1f);
            case Difficulty.Hard:
                return new Color(0.537f, 0.149f, 0.243f, 1f);
        }
        return Color.white;
    }
     
    private void SetBackground(Sprite sprite)
    {
        cover.transform.parent.GetComponent<Image>().sprite = sprite;
    }

    private bool CheckBadges(long hours,ref Sprite background, Sprite cover)
    {
        if (hours >= 100)
            background = UIAssetsManager.instance.goldBackgroundUI;
        else if(hours >= 25)
            background = UIAssetsManager.instance.silverBackgroundUI;
        else if (hours >= 5)
            background = UIAssetsManager.instance.bronzeBackgroundUI;
        else
            return false;

        return true;
    }
}




