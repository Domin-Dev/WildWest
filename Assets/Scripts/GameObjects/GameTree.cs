using System;
using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;


public class GameTree : MonoBehaviour, ILifePoints,ITransparent
{
    SpriteRenderer renderer;
    [SerializeField] private int maxLifePoints;
    private int lifePoints;

    bool isFall;
    bool isDisappear;
    bool isHit;
    bool isFell;

    float repulseRotation;
    float lastRotation;
    bool standUp;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        lifePoints = maxLifePoints;
    }

    private void Update()
    {
        if (isFall || isHit)
        {
            UpdateHit();
       
        }
        if (isDisappear) UpdateisDisappear();
    }

    void ILifePoints.Hit(int damage, Vector2 dir)
    {

        lifePoints = Math.Clamp(lifePoints - damage,0, maxLifePoints);
        if(isFall) return;
        if(lifePoints == 0)
        {
            isFall = true;
            SetHit(90, dir);
        }
        else
        {
            SetHit(25, dir);
        }
        Instantiate(ParticleAssets.instance.leaves, transform.position + (Vector3)new Vector2(0,0.35f), Quaternion.identity);
    }
    void ILifePoints.Kill()
    {
    
    }
    void ITransparent.Hide()
    {
        if(!isDisappear)renderer.color = new Color(1, 1, 1, 0.5f);
    }
    void ITransparent.Show()
    {
        if(!isDisappear)renderer.color = new Color(1, 1, 1, 1f);
    }

    public void SetHit(float angle,Vector2 hitDir)
    {
            isHit = true;
            if (hitDir.x < transform.position.x)
            {
                angle = -angle;
            }

            repulseRotation = angle;
            lastRotation = transform.rotation.z;
            standUp = false;

            
    }
    private void UpdateHit()
    {
        if (!standUp)
        {
            float value = Mathf.LerpAngle(transform.localEulerAngles.z, repulseRotation, Time.deltaTime * 10f);
            float scaleX = Mathf.LerpAngle(transform.localScale.x, 0.9f, Time.deltaTime * 8f);
            float scaleY = Mathf.LerpAngle(transform.localScale.y, 0.9f, Time.deltaTime * 8f);
            transform.eulerAngles = new Vector3(0, 0, value);
            transform.localScale = new Vector3(scaleX,scaleY);


            if (isFall && Math.Abs(Mathf.DeltaAngle(value, repulseRotation)) <= 10)
            {
                if(!isFell)
                {
                    transform.Find("HitBox").AddComponent<DamgeBox>();
                    Instantiate(ParticleAssets.instance.smoke,new Vector3(0,-0.08f,0) + transform.localPosition + transform.up * 0.25f, Quaternion.identity);
                    Sounds.instance.Shield();
                    isFell = true;
                }
            }

            if (Math.Abs(Mathf.DeltaAngle(value, repulseRotation)) <= 2)
            {
                if (!isFall) standUp = true;
                else
                {
                    if(!isDisappear)
                    {
                        transform.Find("Collider").gameObject.SetActive(false);
                    }
                    isDisappear = true;             
                }
            }
        }
        else
        {
            float value = Mathf.LerpAngle(transform.localEulerAngles.z, lastRotation, Time.deltaTime * 15f);
            float scaleX = Mathf.LerpAngle(transform.localScale.x, 1f, Time.deltaTime * 12f);
            float scaleY = Mathf.LerpAngle(transform.localScale.y, 1f, Time.deltaTime * 12f);
            transform.eulerAngles = new Vector3(0, 0, value);
            transform.localScale = new Vector3(scaleX, scaleY);

            if (Math.Abs(Mathf.DeltaAngle(value, lastRotation)) <= 2 && transform.localScale.x > 0.97f)
            {
                transform.eulerAngles = new Vector3(0, 0, lastRotation);
                transform.localScale = new Vector3(1, 1);
                isHit = false;
            }
        }
    }

    private void UpdateisDisappear()
    {
        float value = Mathf.LerpAngle(renderer.color.a, 0, Time.deltaTime * 5f);
        renderer.color = new Color(1, 1, 1, value);
        if (value < 0.05) Destroy(gameObject);
    }
}
