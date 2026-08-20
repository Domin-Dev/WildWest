using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    public static Sounds instance;
    private AudioSource audioSource;
    private Dictionary<string,(AudioSource audioSource,Ambient ambient)> ambients = new Dictionary<string, (AudioSource,Ambient)>();

 
    [SerializeField] private SoundsConfig soundsConfig;

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



    #region Sounds
    public void Sword()
    {
        audioSource.PlayOneShot(swordSounds[UnityEngine.Random.Range(0,1)]);
    }

    public void Hit()
    {
        audioSource.PlayOneShot(hitSounds[UnityEngine.Random.Range(0, 1)]);
    }

    public void Shield() 
    {
        audioSource.PlayOneShot(ShieldSounds[UnityEngine.Random.Range(0, 1)]);
    }

    public void PlayerSound(int id)
    {
        if(soundsConfig.Sounds.Count > id && id >= 0)
        {
            audioSource.PlayOneShot(soundsConfig.Sounds[id]);
        }
    }

    public void PlayerSound(AudioClip shot)
    {
        if(shot != null)
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
    #endregion

    #region Ambients 

    public static void UpdateAmbient(string name, float value)
    {
        if(instance.ambients.TryGetValue(name,out var ambientElement))
        {
            if(ambientElement.ambient.TryGetAmbientClip(value,out AmbientClip ambientClip))
                SetAmbientClip(ambientElement.audioSource,ambientClip,value);
            else
                ambientElement.audioSource.clip = null;
        }
    }
    public static void CreateAmbient(Ambient ambient,float value = 0)
    {
        var audioSource = new GameObject($"{ambient.AmbientName}Ambient",typeof(AudioSource)).GetComponent<AudioSource>();
        audioSource.transform.SetParent(instance.transform);
        audioSource.loop = true;
        audioSource.outputAudioMixerGroup = ambient.AudioMixer; 
       
        if(ambient.TryGetAmbientClip(value,out AmbientClip ambientClip))
            SetAmbientClip(audioSource,ambientClip,value);
        
        instance.ambients.Add(ambient.AmbientName,(audioSource,ambient));
    }  
    private static void SetAmbientClip(AudioSource audioSource,AmbientClip ambientClip, float value)
    {
        audioSource.clip = ambientClip.AudioClip;
        float clampedValue = Mathf.InverseLerp(ambientClip.ActivationRangeMin,ambientClip.ActivationRangeMax,value);

        switch(ambientClip.PitchMode)
        {
            case PitchMode.Random:
                audioSource.pitch = UnityEngine.Random.Range(ambientClip.PitchMin,ambientClip.PitchMax); 
                break;
            case PitchMode.DependingOnValue:
                audioSource.pitch = math.lerp(ambientClip.PitchMin,ambientClip.PitchMax,clampedValue);
                break;
        }
        if(!audioSource.isPlaying)
            audioSource.Play();
    }  
    
    
    #endregion
} 

