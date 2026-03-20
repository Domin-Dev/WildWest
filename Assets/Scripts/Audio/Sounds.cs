using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    public static Sounds instance;
    private AudioSource audioSource;

    [SerializeField] List<AudioClip> swordSounds;
    [SerializeField] List<AudioClip> hitSounds;
    [SerializeField] List<AudioClip> ShieldSounds;
    [SerializeField] AudioClip shot;
    [SerializeField] AudioClip empty;
    [SerializeField] AudioClip reload;
    [SerializeField] AudioClip roll;
    [SerializeField] AudioClip ammo;
    [SerializeField] AudioClip hammer;
    [SerializeField] AudioClip click;
    private void Awake()
    {
        if(instance == null )
        {
            instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Sword()
    {
        audioSource.PlayOneShot(swordSounds[Random.Range(0,1)]);
    }

    public void Hit()
    {
        audioSource.PlayOneShot(hitSounds[Random.Range(0, 1)]);
    }

    public void Shield() 
    {
        audioSource.PlayOneShot(ShieldSounds[Random.Range(0, 1)]);
    }

    public void PlayerSound(AudioClip shot)
    {
        audioSource.PlayOneShot(shot);
    }

    public void Shot()
    {
        audioSource.PlayOneShot(shot);
    }

    public void Empty()
    {
        audioSource.PlayOneShot(empty);
    }
    public void Roll()
    {
        audioSource.PlayOneShot(roll);
    }

    public void Reload()
    {
        audioSource.PlayOneShot(reload);
    }  
    
    public void Ammo()
    {
        audioSource.PlayOneShot(ammo);
    }

    public void Hammer()
    {
        audioSource.PlayOneShot(hammer);
    }

    public void Click()
    {
        audioSource.PlayOneShot(click);
    }
}

