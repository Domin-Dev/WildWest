using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class  UIAssetsManager : MonoBehaviour
{

    [Header("Fonts")]
    [SerializeField] public List<TMP_FontAsset> fonts;
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


    [Space]
    [SerializeField] public Sprite ironBarsUI;



    public static UIAssetsManager instance { private set; get; }


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
}

