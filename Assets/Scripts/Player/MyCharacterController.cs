using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MyCharacterController: MonoBehaviour, ILifePoints, IUsesWeapons
{

    [SerializeField] private int maxHP;
    [SerializeField] private int hp;
    [SerializeField] private Transform hpBar;
    [SerializeField] private Transform center;



    private Animator animator;
    private SortingGroup sortingGroup;
    private float speed = 1f;


    private Vector2 moveDir;
    private Vector2 sightDir;
    private Rigidbody2D rigidbody2D;

    private bool isRepulsed = false;
    public HandsController handsController { private set; get; }
    private CharacterEditorSettings settings;


    #region StateMachine
    public HeroStateMachine heroStateMachine { set; get; }
    public IdleState idleState { set; get; }
    public ActionState attackState { set; get; }
    public SideActionState sideActionState { set; get; }
    public ReloadingState reloadingState { set; get; }
    public BuildingState buildingState { set; get; }
    private void SetStateMachine()
    {
        heroStateMachine = new HeroStateMachine();
        idleState = new IdleState(this, heroStateMachine);
        attackState = new ActionState(this, heroStateMachine);
        sideActionState = new SideActionState(this, heroStateMachine);
        reloadingState = new ReloadingState(this, heroStateMachine);
        buildingState = new BuildingState(this, heroStateMachine);
    }

    private void SetUpUI()
    {
        UIManager.instance.windowOpen += WindowOpen;
    }

    private void WindowOpen(object sender, EventArgs e)
    {
        rigidbody2D.velocity = Vector2.zero;
        animator.SetBool("Idle", true);
    }

    #endregion

    public CharacterSpriteController characterSpriteController { private set; get; }

    private void Awake()
    {
        characterSpriteController = GetComponent<CharacterSpriteController>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        handsController = GetComponent<HandsController>();
        animator = GetComponent<Animator>();   
        sortingGroup = GetComponent<SortingGroup>();
        handsController.SetController(this,"Enemy");
        SetUpUI();
        SetStateMachine();
    }
    private void Start()
    {
        heroStateMachine.RunMachine(idleState);
    }
    private void Update()
    {
        //  if (!IsOwner) return;
        sightDir = MyTools.GetMouseWorldPosition();  
        if(UIManager.instance.WindowsAreClosed())heroStateMachine.currentState.FrameUpdate();
    }
    private void FixedUpdate()
    {
        if(UIManager.instance.WindowsAreClosed()) heroStateMachine.currentState.FrameFixedUpdate();
    }
   
    public void UpdateFlip()
    {
        handsController.Updateflip(sightDir);
    }
    public void UpdateCharacterSprites()
    {
      //  characterSpriteController.UpdateSprite(moveDir, (sightDir - (Vector2)center.position).normalized);
    }

    #region Movement
    public void GetMovementInput()
    {
        if(!ChatManager.instance.isChatting)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            moveDir = new Vector2(x, y).normalized;
        }
        else
        {
            moveDir = Vector2.zero;
        }
    }
    public void UpdateMovement()
    {
        if (!isRepulsed)
        {
            rigidbody2D.velocity = moveDir * speed;
            if(moveDir == new Vector2(0,0))
            {
                animator.SetBool("Idle", true);
            }
            else
            {
                GridVisualization.instance.PlayerMovement(transform.position);
                animator.SetBool("Idle", false);
            }
        }
    }
    public void SetPosition(Vector2 vector)
    {
        transform.position = vector;
        GridVisualization.instance.PlayerMovement(vector);
    }

    #endregion

    #region ILifePoints
    void ILifePoints.Hit(int damage, Vector2 dir)
    {
        StartCoroutine(HitForce(((Vector2)transform.position - dir).normalized));
        hp = Mathf.Clamp(hp - damage, 0, maxHP);
        if (hp == 0) Destroy(gameObject);
        hpBar.localScale = new Vector3((float)hp / maxHP, 1, 1);
    }
    void ILifePoints.Kill()
    {

    }
    #endregion

    #region IUsesWeapons
    void IUsesWeapons.Block()
    {
        StartCoroutine(BlockTime());
    }
    IEnumerator BlockTime()
    {
        yield return new WaitForSeconds(.005f);
        handsController.updaterAttack.Back();
    }
    void IUsesWeapons.EndAttack()
    {
        heroStateMachine.ChangeState(idleState);
    }
    public IEnumerator HitForce(Vector2 dir)
    {
        isRepulsed = true;
        rigidbody2D.AddForce(2 * dir.normalized, ForceMode2D.Impulse);
        handsController.ResetAttack(); 
        yield return new WaitForSeconds(0.1f);
        isRepulsed = false;
    }

    public void Riding(Horse horse)
    {
        horse.sortingGroup.sortingOrder = -10;
        transform.position = horse.riderPoint.position;
        horse.transform.parent = transform;
        speed = horse.speed;
    }
    #endregion
}

