using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterSpriteController : MonoBehaviour
{
    //[SerializeField] public SpriteRenderer head;
    //[SerializeField] public SpriteRenderer eyes;
    //[SerializeField] public SpriteRenderer mouth;
    //[SerializeField] public SpriteRenderer hair;

    //[SerializeField] public SpriteRenderer handL;
    //[SerializeField] public SpriteRenderer handR;

    //[SerializeField] public SpriteRenderer body;
    //[SerializeField] public SpriteRenderer underwear;
    //[Space]
    //[SerializeField] public SpriteRenderer[] clothes;
    //[Space]

    //[SerializeField] int[] clothesID = new int[8];

    //private CharacterEditorSettings characterEditorSettings;

    //private Vector2 lastMoveDir = Vector2.zero;
    //private Vector2 lastSightDir = Vector2.zero;

    //public int hairstyleIndex = 1;

    //private void Awake()
    //{
    //    characterEditorSettings = Resources.Load<CharacterEditorSettings>("CharacterParts/CharacterEditorSettings");
    //    ClearArray();
    //}

    //public Vector2 GetThrowDir(float multiplier)
    //{
    //    return (Vector2)transform.position + lastSightDir * multiplier;
    //}

    //private void ClearArray()
    //{
    //    for (int i = 0; i < clothesID.Length; i++)
    //    {
    //        clothesID[i] = -1;
    //    }
    //}
    //public void UpdateSprite(Vector2 moveDir, Vector2 sightDir)
    //{
    //    if (lastMoveDir != moveDir)
    //    {
    //        lastMoveDir = moveDir;
    //        ChangeBodySprites(moveDir);
    //    }

    //    if (lastSightDir != sightDir)
    //    {
    //        lastSightDir = sightDir;
    //        ChangeHeadSprites(sightDir);
    //    }
    //}

    //public void RefreshSprites()
    //{
    //    ChangeBodySprites(lastMoveDir);
    //    ChangeHeadSprites(lastSightDir);
    //}

    //public void SetClothes(int garmentType,int itemID)
    //{
    //    clothesID[garmentType] = itemID;
    //    RefreshSprites();
    //}

    //public void RemoveClothes(int garmentType)
    //{
    //    clothesID[garmentType] = -1;
    //    clothes[garmentType].sprite = CharacterEditorSettings.emptySprite;
    //}

    //private void ChangeBodySprites(Vector2 dir)
    //{
    //    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    //    angle += 180;

    //    if (angle >= 20 && angle <= 160) SetSpriteBody(0);
    //    else if (angle > 160 && angle < 200) SetSpriteBody(2);
    //    else if (angle >= 200 && angle <= 340) SetSpriteBody(1);
    //    else SetSpriteBody(3);

    //    //if (dir.x > 0) SetSpriteBody(2);
    //    //else if (dir.x < 0) SetSpriteBody(3);

    //    //if (dir.y > 0) SetSpriteBody(1);
    //    //else if (dir.y < 0) SetSpriteBody(0);
    //}

    //protected void ChangeHeadSprites(Vector2 dir)
    //{
    //    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    //    angle += 180;

    //    if (angle >= 20 && angle <= 160)SetSpriteHead(0);
    //    else if (angle > 160 && angle < 200)SetSpriteHead(2);
    //    else if (angle >= 200 && angle <= 340)SetSpriteHead(1);
    //    else SetSpriteHead(3); 
    //}
    //// indexs: 0 - down, 1 - up, 2 - rigth, 3 - left
    //private void SetSpriteHead(int index)
    //{
    //    if (index > 1)
    //    {
    //        head.sprite = characterEditorSettings.heads[1];
    //    }
    //    else
    //    {
    //        head.sprite = characterEditorSettings.heads[0];
    //    }
    //    eyes.sprite = characterEditorSettings.eyes[index];
    //    mouth.sprite = characterEditorSettings.mouth[index];
    //    hair.sprite = characterEditorSettings.hairstyles[hairstyleIndex].sprites[index];
    //    for (int i = 0; i < 2; i++)
    //    {
    //        if (clothesID[i] >= 0)
    //        {
    //            clothes[i].sprite = ((Garment)ItemsAsset.instance.GetItem(clothesID[i])).sprites[index];
    //        }
    //    }


    //    if (index == 1)
    //    {
    //        head.GetComponent<SortingGroup>().sortingOrder = 100;
    //        body.GetComponent<SortingGroup>().sortingOrder = 20;
    //    }
    //    else
    //    {
    //        head.GetComponent<SortingGroup>().sortingOrder = 5;
    //        body.GetComponent<SortingGroup>().sortingOrder = 0;
    //    }
    //    if (lastMoveDir == Vector2.zero) SetSpriteBody(index);
    //}
    //// indexs: 0 - down, 1 - up, 2 - rigth, 3 - left
    //private void SetSpriteBody(int index)
    //{
    //    if (index > 1)
    //    {
    //        body.sprite = characterEditorSettings.bodies[1];
    //    }
    //    else
    //    {
    //        body.sprite = characterEditorSettings.bodies[0];
    //    }
    //    underwear.sprite = characterEditorSettings.underwear[index];
    //    for (int i = 2; i < 8; i++)
    //    {
    //        if (clothesID[i] >= 0)
    //        {
    //            clothes[i].sprite = ((Garment)ItemsAsset.instance.GetItem(clothesID[i])).sprites[index];
    //        }
    //    }
    //}
}
