using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;



public interface IReadTag
{
    public int ID { get; }
    public string tagName { get; }
    public Sprite icon { get; }
    public LocalizedString localizedString { get; }

}




[CreateAssetMenu(fileName = "NewTag", menuName = "GameAsset/Tag")]
public class Tag : ScriptableObject, IReadTag
{
    [Header("Tag info")]

    [SerializeField] private Sprite _icon;
    [SerializeField] LocalizedString _localizedString;
    [SerializeField] private string _tagName;
    [SerializeField] int _ID = -1;

    public string tagName => _tagName; 
    public int ID => _ID; 
    public Sprite icon => _icon; 
    public LocalizedString localizedString => _localizedString;



    private void OnValidate()
    {
        if (_ID == -1) _ID = Resources.Load<IDManager>("IDManager").GetNextTagID();
    }
 
}