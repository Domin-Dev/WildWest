using UnityEngine;

public class Scirpt : MonoBehaviour
{
    public Sprite sprite;
  void Start()
    {
        GetComponent<Renderer>().material.SetTexture("_Texture2D", sprite.texture);
    }
    void Update()
    {
        
    }
}
