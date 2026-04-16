using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;




[CreateAssetMenu(fileName = "NewAmmoTag", menuName = "GameAsset/Tags/AmmoTag")]
public class AmmoTag : Tag
{
    [Header("Ammo Tag info")]
    [SerializeField] private Sprite noAmmoIconUI;
    public Sprite NoAmmoIconUI => noAmmoIconUI;
}

