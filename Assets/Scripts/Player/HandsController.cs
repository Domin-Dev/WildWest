using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class HandsController : MonoBehaviour
{
    public class Updater
    {
        bool back;
        public Func<bool> funcToUpdate;
        public Func<bool> funcToBack;

        public Updater(Func<bool> funcToUpdate, Func<bool> funcToBack)
        {
            this.funcToUpdate = funcToUpdate;
            this.funcToBack = funcToBack;
            back = false;
        }
        public bool Update()
        {
            if (!back)
            {
                back = funcToUpdate();
            }
            else
            {
                if (funcToBack())
                {
                    back = false;
                    return true;                
                }
            }
            return false;
        }

        public void Back()
        {
            back = true; 
        }
    }
    public Updater updaterAttack { private set; get; }
    public Updater updaterReload { private set; get; }
 


    #region SerializeFields
    [SerializeField] private Transform backpack;
    [SerializeField] private Transform secondHand;
    [SerializeField] private SpriteRenderer secondHandItem;
    [SerializeField] private Transform secondHandParent;
    [SerializeField] private SpriteRenderer itemInHand;
    [SerializeField] private PolygonCollider2D hitBox;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private Transform reloadPoint;
    [SerializeField] private Transform mainHand;
    [SerializeField] private Transform hand;
    [SerializeField] private Transform fliper;
    #endregion
    private Vector3 secondHandStartPosition;
    public ItemStats selectedItem;
    private ToolType toolType;
    private Sprite ammoSprite;

    #region Events
    public event EventHandler UseItem;
    public event EventHandler<SetAmmoBarArgs> SetAmmoBar;
    public event EventHandler<UpdateAmmoBarArgs> UpdateAmmoBar;
    public event EventHandler HideAmmoBar;
    #endregion

    private Vector3 attackAngle;
    private Vector3 attackVector;
    private Vector3 lastWeaponPosition;
    private Transform attackItem;

    #region Aim
    private Vector3 targetPos;
    private bool flip;
    #endregion
    //other
    private FirstHand firstHand;
    private IUsesWeapons usesWeapons;
    public bool isGun { get; private set; } = false;
    public bool canAttack { get; private set; } = true;
    private const float setTime = 0.45f;

    private MyCharacterController characterController;
    public GameObject bullet;

    private void Awake()
    {
        secondHandStartPosition = secondHand.localPosition;
        updaterAttack = new Updater(UpdateAttack, UpdateAttackReturn);
        updaterReload = new Updater(UpdateReload, UpdateReloadReturn);
    }
    private void Start()
    {
        characterController = GetComponent<MyCharacterController>();
        SetUpEvents();
        UIManager.instance.SetUpUIPlayer(this);
        EquipmentManager.instance.SetUpEvent(this);
    }
    private void SetUpEvents()
    {
        EquipmentManager.instance.UpdateItemInHand += UpdateItemInHand;
    }
    private void UpdateItemInHand(object sender, ItemStatsArgs e)
    {
        toolType = ToolType.None;
        if (characterController.heroStateMachine.currentState is ReloadingState)
        {
            EndReloading();
        }
        else if(characterController.heroStateMachine.currentState is ActionState)
        {
            ResetAttack();
        }
        else
        {
            characterController.heroStateMachine.ChangeState(characterController.idleState);
        }

        itemInHand.transform.localPosition = Vector3.zero;
        if (e.item != null)
        {
            Item item = ItemsAsset.instance.GetItem(e.item.itemID);
            selectedItem = e.item;
            if (item as Weapon != null)
            {
                toolType = ItemsAsset.instance.GetToolType(item.ID);
                ItemIsWepon(item, e.item);
            }
            else
                ItemIsNotWepon(item);
        }
        else
            ItemIsNull();

      //  SwitchState(toolType);
    }

    public void SetDefaultState()
    {
        SwitchState(toolType);
    }

    private void SwitchState(ToolType toolType)
    {
        switch(toolType)
        {
            case ToolType.None:
                characterController.heroStateMachine.ChangeState(characterController.idleState);
                break;
        }
    }
    private void ItemIsNull()
    {
        selectedItem = null;
        itemInHand.sprite = null;
        isGun = false;
        SecondHandReset();
        HideAmmoBar(this, null);
    }
    private void ItemIsNotWepon(Item item)
    {
        itemInHand.sprite = item.icon;
        itemInHand.sortingOrder = 20;
        SecondHandReset();
        HideAmmoBar(this, null);

        if(item as BuildingItem != null)
        {
            characterController.heroStateMachine.ChangeState(characterController.buildingState);
        }
    }
    private void ItemIsWepon(Item item, ItemStats itemStats)
    {
        Weapon weapon = (Weapon)item;
        itemInHand.sprite = weapon.weaponImage;
        hitBox.points = weapon.hitBoxPoints;
        hitBox.transform.localPosition = -weapon.gripPoint1;
        itemInHand.sortingOrder = 10;
        itemInHand.transform.localPosition = -weapon.gripPoint1;
        if (weapon.gripPoint2.x != -100)
        {
            secondHand.parent = itemInHand.transform;
            secondHand.localEulerAngles = Vector3.zero;
            secondHand.localPosition = weapon.gripPoint2;          
        }
        else
        {
            SecondHandReset();
        }

        if (weapon as RangedWeapon != null)
        {
            isGun = true;
            RangedWeapon rangedWeapon = (RangedWeapon)weapon;
            float posY = Mathf.Abs(rangedWeapon.aimPoint.y - rangedWeapon.gripPoint1.y);
            SetTransformHand(posY);
         //  aimPoint.localPosition = (Vector2)itemInHand.transform.localPosition + rangedWeapon.aimPoint;
         //   reloadPoint.localPosition = (Vector2)itemInHand.transform.localPosition + rangedWeapon.reloadPoint;
            SetAmmoBar(this, new SetAmmoBarArgs(rangedWeapon.magazineCapacity, (itemStats as RangedWeaponItem).currentAmmoCount, rangedWeapon.oldtype));
            ammoSprite = ItemsAsset.instance.GetAmmoHandSprite(rangedWeapon.oldtype);
        }
        else
        {
            isGun = false;
            HideAmmoBar(this, null);
        }
    }
    private void SecondHandReset()
    {
        if (secondHand.parent != secondHandParent)
        {
            secondHand.parent = secondHandParent;
            secondHand.localPosition = secondHandStartPosition;
            secondHand.localEulerAngles = Vector3.zero;
        }
    }
    public void Use()
    {
        UseItem(this, null);
    }
    public void Shot()
    {
        (selectedItem as RangedWeaponItem).Shot();
        UpdateAmmoBar(this, new UpdateAmmoBarArgs((selectedItem as RangedWeaponItem).currentAmmoCount));

        Vector3 aimDir = (MyTools.GetMouseWorldPosition() - mainHand.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle = 180 + (180 + angle);
        Instantiate(bullet, aimPoint.position, Quaternion.identity).transform.localEulerAngles = new Vector3(0, 0, angle + UnityEngine.Random.Range(-5, 5));
        Sounds.instance.Shot();
        ShotParticle(angle);
    }

    private Vector2? target = null;
    private Vector2 startPosition;
    private Transform startParent;
    public void Reload()
    {
        startPosition = secondHand.localPosition;
        startParent = secondHand.parent;
        secondHand.eulerAngles = Vector3.zero;

        secondHand.parent = transform;
        target = backpack.transform.position;

        rotationTarget = hand.localEulerAngles - new Vector3(0,0, -60);
    }
    public void SpawnShells()
    {
        Transform shells  = Instantiate(ParticleAssets.instance.gunShells, reloadPoint.transform.position, Quaternion.identity).transform;
        int shellCount = (selectedItem as RangedWeaponItem).ToFullMagazine();
        shells.GetComponent<ParticleSystem>().textureSheetAnimation.SetSprite(0, ammoSprite);
        shells.GetComponent<ParticleSystem>().emission.SetBurst(0,new ParticleSystem.Burst(0,1,1,shellCount, 0.03f));
    }
    private bool UpdateReload()
    {    
        secondHand.position = Vector2.Lerp(secondHand.position, backpack.position, Time.deltaTime * 5f);
        secondHand.eulerAngles = Vector3.Lerp(secondHand.eulerAngles, new Vector3(0, 0, -90), Time.deltaTime * 2f);
        if (Vector2.Distance(secondHand.position, backpack.position) < 0.01f)
        {
            Sounds.instance.Ammo();
            secondHandItem.sprite = ammoSprite; 
            return true;
        }
        return false;
    }
    private bool UpdateReloadReturn()
    {     
        secondHand.position = Vector2.Lerp(secondHand.position, reloadPoint.position, Time.deltaTime * 5f);
        secondHand.eulerAngles = Vector3.Lerp(secondHand.eulerAngles,mainHand.eulerAngles, Time.deltaTime * 6f);

        if (Vector2.Distance(secondHand.position, reloadPoint.position) < 0.01f)
        {      
            (selectedItem as RangedWeaponItem).Reload(1);
            Sounds.instance.Reload();
            UpdateAmmoBar(this, new UpdateAmmoBarArgs((selectedItem as RangedWeaponItem).currentAmmoCount));
            secondHandItem.sprite = null;
            int ammoCount = EquipmentManager.instance.Reload();


            if (!(selectedItem as RangedWeaponItem).CanReload() || ammoCount <= 0)
            {
                Sounds.instance.Roll();
                EndReloading();  
            }
            return true;
        }
        return false;
    }
    public void EndReloading()
    {
        secondHandItem.sprite = null;
        secondHand.parent = startParent;
        secondHand.localPosition = startPosition;
        characterController.heroStateMachine.ChangeState(characterController.idleState);
    }

    private Vector3 rotationTarget;
    public bool UpdateRotation()
    {
        hand.localEulerAngles = new Vector3(0,0,GetEulerAnglesLerp(rotationTarget, 30));
        if (CheckAngle(rotationTarget, 1f))
        {
            return true;
        }   
        return false;
    }
    public void Aim()
    {
        float angle = GetAngle(targetPos, mainHand, 0);
        float angle1; 

        if (flip)
            angle1 = GetAngle(targetPos, secondHandParent, -100);
        else
            angle1 = GetAngle(targetPos, secondHandParent, 100);        
        
        float z;
        if (hand.localEulerAngles.z > 180)
             z = Mathf.Lerp(hand.localEulerAngles.z - 360, 0, Time.deltaTime * 10);
        else
            z = Mathf.Lerp(hand.localEulerAngles.z , 0, Time.deltaTime * 10);

        hand.localEulerAngles = new Vector3(hand.localEulerAngles.x, hand.localEulerAngles.y, z);
        mainHand.eulerAngles = Vector3.Lerp(mainHand.eulerAngles, new Vector3(0, 0, angle), Time.deltaTime * 12f);
        secondHandParent.eulerAngles = Vector3.Lerp(secondHandParent.eulerAngles, new Vector3(0, 0, angle1), Time.deltaTime * 2f);
    }
    public void SetController(IUsesWeapons usesWeapons,string enemyTag)
    {
        this.usesWeapons = usesWeapons;
        firstHand = transform.Find("MainHand").GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<FirstHand>();
        firstHand.SetController(usesWeapons,enemyTag,transform);
    }
    private float GetAngle(Vector3 mousePos, Transform aimTransform, float addValue)
    {
        Vector3 aimDir = (mousePos - aimTransform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        angle += addValue;
        if (angle < 0) angle = 180 + (180 + angle);
        if (aimTransform.eulerAngles.z - angle > 180)
        {
            angle = 360 + angle;
        }
        else if (aimTransform.eulerAngles.z - angle < -180)
        {
            angle = -(360 - angle);
        }
        return angle;
    }
    public void SetAttackVector(Vector3 Angle, Vector3 position)
    {       
        canAttack = false;
        Timer.Create(setTime, () => { canAttack = true; return true; });
        attackItem = fliper;
        
        attackAngle = hand.localEulerAngles - Angle;  
        this.firstHand.AttackSwitch(true);
        lastWeaponPosition = attackItem.localPosition;
        attackVector = position + attackItem.localPosition;
    }
    public void Updateflip(Vector3 targetPos)
    {
        this.targetPos = targetPos;
        if(flip != this.targetPos.x < mainHand.position.x)
        {
            fliper.localPosition = new Vector3(fliper.localPosition.x, -fliper.localPosition.y, 0);
            if(flip)
            {
                fliper.localEulerAngles = new Vector3(0,0, 0);
            }
            else
            {
                fliper.localEulerAngles = new Vector3(180,0,0);
            }
        }
        flip = this.targetPos.x < mainHand.position.x;
    }
    private void ShotParticle(float angle)
    {
        Instantiate(ParticleAssets.instance.shotSmoke, aimPoint.TransformPoint(aimPoint.localPosition + new Vector3(-0.05f, 0)), Quaternion.identity);
        Instantiate(ParticleAssets.instance.shotFire, aimPoint.position , Quaternion.identity).transform.localEulerAngles = new Vector3(0,0,angle);
    }
    public bool UpdateAttackReturn()
    {
        attackItem.localPosition = Vector3.Lerp(attackItem.localPosition, lastWeaponPosition, Time.deltaTime * 30f);
        if (Vector3.Distance(attackItem.localPosition, lastWeaponPosition) < 0.015)
        {
            ResetAttack();
            return true;
        }
        return false;
    }
    public bool UpdateAttack()
    {
        hand.localEulerAngles = new Vector3(0,0,GetEulerAnglesLerp(attackAngle,30));
        attackItem.localPosition = Vector3.Lerp(attackItem.localPosition, attackVector, Time.deltaTime * 12f);
        if (CheckAngle(attackAngle,1f) && Vector3.Distance(attackItem.localPosition, attackVector) < 0.1f)
        {
            return true;
        }   
        return false;
    }
    private float GetEulerAnglesLerp(Vector3 target,float speed)
    {
        Vector3 angle = hand.localEulerAngles;
        if(target.z < 0) angle = new Vector3(0,0, angle.z - 360);

        return Mathf.LerpAngle(angle.z, target.z, Time.deltaTime * speed);
    }
    private bool CheckAngle(Vector3 target, float border)
    {
        Vector3 angle = target;
        if (angle.z > 360) angle = new Vector3(target.x, target.y, target.z % 360);


        if (angle.z >= 0)
        {
            return Mathf.Abs(hand.localEulerAngles.z - angle.z) < border;
        }
        else
        {
            return Mathf.Abs(hand.localEulerAngles.z - (360 + angle.z)) < border;
        }
    
    }
    public void ResetAttack()
    {
        if(attackItem != null)
        {
            fliper.transform.localPosition = lastWeaponPosition;
            SetTransformHand(Mathf.Abs(lastWeaponPosition.y));
            attackItem = null;
            firstHand.AttackSwitch(false);
            usesWeapons.EndAttack();
        }
    }
    public void SetTransformHand(float posY)
    {
        if (flip)
        {
            fliper.localPosition = new Vector3(fliper.localPosition.x, posY, 0);
        }
        else
        {
            fliper.localPosition = new Vector3(fliper.localPosition.x, -posY, 0);
        }
    } 
    public bool CanReload()
    {
        RangedWeaponItem rangedWeapon = selectedItem as RangedWeaponItem;
        return rangedWeapon != null && rangedWeapon.CanReload() && EquipmentManager.instance.CountAmmo(rangedWeapon);
    }
}

