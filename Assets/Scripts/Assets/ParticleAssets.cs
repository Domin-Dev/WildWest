using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class ParticleAssets : MonoBehaviour
{

    private static ParticleAssets i;

    public static ParticleAssets instance 
    {
        get
        {
            if (i == null)
            {
                i = Instantiate(Resources.Load("ParticleAssets") as GameObject).GetComponent<ParticleAssets>();
            }
            return i;
        }
    }

    public GameObject smoke;
    public GameObject leaves;
    public GameObject shotSmoke;
    public GameObject shotFire;
    public GameObject gunShells;
    public GameObject water;
}
