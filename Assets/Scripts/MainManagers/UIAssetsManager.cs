using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class  UIAssetsManager : MonoBehaviour
{
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

