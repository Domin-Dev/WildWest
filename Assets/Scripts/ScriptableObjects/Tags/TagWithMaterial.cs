using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;





[CreateAssetMenu(fileName = "TagWithMaterial", menuName = "GameAsset/Tags/TagWithMaterial")]
public class TagWithMaterial : Tag
{
    public override Type RequiredTagSelection => typeof(TagSelectionMaterial);
}