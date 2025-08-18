using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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



    private string worldName;
    public void SetWorld(HeaderData header, Material materialIcon)
    {
        worldName = header.worldName;
        worldNameText.text = worldName;

        saveTime.text = DateTimeOffset.FromUnixTimeSeconds(header.saveTime).DateTime.ToLocalTime().ToString();

        Material mat = new Material(materialIcon);
        headIcon.material = mat;
        
        mat.SetColor("_SkinColor", MyTools.GetColorFromFloat3(header.characterLook.skinColor));
        mat.SetColor("_HairColor", MyTools.GetColorFromFloat3(header.characterLook.hairColor));
        mat.SetInt("_HairIndex", header.characterLook.hairIndex);
        mat.SetInt("_BeardIndex", header.characterLook.beardndex);
        mat.SetInt("_PaintingsIndex", header.characterLook.faceDetailsIndex);

        difficulty.text = header.difficulty.ToString();

        if (header.characterLook.faceDetailsIndex == 9)
        {
            cover.gameObject.SetActive(true);
            cover.sprite = UIAssetsManager.instance.ironBarsUI;
            cover.transform.parent.GetComponent<Image>().sprite = UIAssetsManager.instance.blackBacgroundUI;
        }
     //   difficulty.color = GetColor(header.difficulty);

        headIcon.SetMaterialDirty();
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
}




