 using System;
using System.Collections;
using UnityEngine;


public class NPCController : MonoBehaviour, ILifePoints,IUsesWeapons
{
    public bool isTarget;

    [SerializeField] private int maxHP;
    [SerializeField] private int hp;
    [SerializeField] private Transform hpBar;


    private Rigidbody2D rigidbody2D;

    public HeroStateMachine heroStateMachine {get; private set;}
    public NPCIdleState idleState { get; private set; }
    public NPCFollowState followState { get; private set; }
    public NPCAttackState attackState { get; private set; }
    public NPCRepulsedState repulsedState { get; private set; } 

    private Transform target;

    float timer;
    float setTime = 1f;
    public bool canAttack;


    public HandsController attackModule { private set; get; }

    private void Awake()
    {
        hp = maxHP;
        rigidbody2D = GetComponent<Rigidbody2D>();  
        attackModule = GetComponent<HandsController>();
        attackModule.SetController(this,"Player");
        SetStateMachine();
    }
    private void Start()
    {
       heroStateMachine.RunMachine(idleState);
    }
    private void SetStateMachine()
    {
        heroStateMachine = new HeroStateMachine();

        idleState = new NPCIdleState(this,heroStateMachine);
        followState = new NPCFollowState(this, heroStateMachine);
        attackState = new NPCAttackState(this, heroStateMachine);
        repulsedState = new NPCRepulsedState(this, heroStateMachine);
    }
    private void Update()
    {
        //CoolDownUpdate();
        //heroStateMachine.currentState.FrameUpdate();
        //if(target != null) attackModule.Updateflip(target.position);
    }
    private void FixedUpdate()
    {
        heroStateMachine.currentState.FrameFixedUpdate();
    }



    private void CoolDownUpdate()
    {
        if (!canAttack)
        {
            timer += Time.deltaTime;
            if (timer >= setTime)
            {
                canAttack = true;
                timer = 0;
            }
        }
    }
    public void SetTarget(Transform target)
    {
        if (target == null)
        {
            this.target = null;
            isTarget = false;
        }
        else
        {
            this.target = target;
            isTarget = true;
        }
    }
    public void Follow()
    {
       rigidbody2D.linearVelocity = target.position - transform.position;    
    }
    public void StopFollow()
    {
        rigidbody2D.linearVelocity = Vector3.zero;
    }
    public float GetDistance()
    {
        return Vector3.Distance(transform.position, target.position);
    }

    private Vector3 hitDir;
    public void Hit(int damage,Vector2 dir)
    {
        hp = Mathf.Clamp(hp - damage, 0, maxHP);
        if(hp == 0) Destroy(gameObject);
        hpBar.localScale = new Vector3((float)hp / maxHP, 1, 1);

        if (heroStateMachine.currentState != repulsedState)
        {
            hitDir = dir;
            heroStateMachine.ChangeState(repulsedState);
            rigidbody2D.AddForce(2 * ((Vector2)transform.position - dir).normalized, ForceMode2D.Impulse);
        }
    }
    public void Kill()
    {
        throw new System.NotImplementedException();
    }
    void IUsesWeapons.Block()
    {
        StartCoroutine(BlockTime());
    }
    IEnumerator BlockTime()
    {
        yield return new WaitForSeconds(.005f);
       // attackModule.back = true;
    }
    IEnumerator Repulse()
    {
        yield return new WaitForSeconds(.2f);
        heroStateMachine.ChangeState(idleState);
    }


    private bool isRepulse;
    private bool standUp = false;
    private float repulseRotation;
    private float lastRotation;
    public void SetRepulse(float angle)
    {
        attackModule.ResetAttack();
        if (UnityEngine.Random.Range(0,5) == 0)
        {
            isRepulse = true;
            if (hitDir.x < transform.position.x )
            {
                angle = -angle;
            }

            repulseRotation = angle;
            lastRotation = transform.rotation.z;
            standUp = false;
        }
        else
        {
            StartCoroutine(Repulse());
            isRepulse = false;            
        }
    }
    public void UpdateRepulse()
    {
        if (isRepulse)
        {
            if (!standUp)
            {
                float value = Mathf.LerpAngle(transform.localEulerAngles.z, repulseRotation, Time.deltaTime * 12f);
                transform.eulerAngles = new Vector3(0, 0, value);

                if (Math.Abs(Mathf.DeltaAngle(value, repulseRotation)) <= 2)
                {
                    standUp = true;
                }
            }
            else
            {
                float value = Mathf.LerpAngle(transform.localEulerAngles.z, lastRotation, Time.deltaTime * 7f);
                transform.eulerAngles = new Vector3(0, 0, value);

                if (Math.Abs(Mathf.DeltaAngle(value, lastRotation)) <= 2)
                {
                    transform.eulerAngles = new Vector3(0, 0, lastRotation);
                    heroStateMachine.ChangeState(idleState);
                }
            }
        }
    }
    void IUsesWeapons.EndAttack()
    {
        heroStateMachine.ChangeState(idleState);       
    }

}
