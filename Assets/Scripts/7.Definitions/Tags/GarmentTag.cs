using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;




[CreateAssetMenu(fileName = "NewGarmentTag", menuName = "GameAsset/Tags/GarmentTag")]
public class GarmentTag : TagBase
{
    [Header("Garment Tag info")]

    [SerializeField] private BodyPart _bodyPart;

    [SerializeField] private string _texturePropertyName;
    [SerializeField] private string _colorPropertyName;

    [SerializeField] private List<(int value,string propertyName)> intProperties;

    public BodyPart bodyPart => _bodyPart;
    public string texturePropertyName => _texturePropertyName;
    public string colorPropertyName => _colorPropertyName;
}


public enum BodyPart
{
    head,
    body,
    mainHand,
    sideHand,
}
