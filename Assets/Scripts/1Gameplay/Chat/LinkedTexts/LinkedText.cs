using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LinkedTextMeshPro))]
public class LinkedText : MonoBehaviour
{
    [SerializeField] private RectTransform linkedText;
    public void Awake()
    {
        GetComponent<LinkedTextMeshPro>().linkedText = linkedText;
    }
} 