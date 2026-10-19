using UnityEngine;

[CreateAssetMenu(fileName = "CharacterEditorConfig", menuName = "GameAsset/CharacterEditorConfig")]
public class CharacterEditorConfig : Config<CharacterEditorConfig>
{
    [SerializeField] public Color[] skinColors;
    [SerializeField] public Color[] hairColors;
    [SerializeField] public Color[] clothesColors;
    public static readonly Sprite emptySprite = Sprite.Create(null, new Rect(), Vector2.zero);

    [SerializeField] public Texture2D hairstylesTexture;
    [SerializeField] public Texture2D beardTexture;
    [SerializeField] public Texture2D faceDetailsTexture;
    [Space]
    [SerializeField] public Sprite[] underwear;
}

[System.Serializable]
public class States
{
    public Sprite[] sprites = new Sprite[4];
}
