using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class  UIAssetsManager : MonoBehaviour
{

    [SerializeField] public Sprite blackBacgroundUI;
    [SerializeField] public Sprite woodenFrameUI;
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

