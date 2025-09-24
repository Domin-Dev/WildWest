using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Horse : MonoBehaviour
{
    [SerializeField] public float speed;
    [SerializeField] public Transform riderPoint;
    public SortingGroup sortingGroup;
    private void Start()
    {
        sortingGroup = GetComponent<SortingGroup>();
    }
}

