using System.Collections.Generic;
using UnityEngine;


public class TextureLoader : MonoBehaviour
{
    public static Dictionary<int,Texture2D> LoadFloors(Floor[] floors)
    {
        Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>(); 
        foreach (var floor in floors)
        {
            Texture2D texture2D = Resources.Load<Texture2D>(floor.texturePath);
            textures.Add(floor.ID, texture2D);
        }
        return textures;
    }

    public static Dictionary<int, Texture2D> LoadBorderTextures(Floor[] floors, Dictionary<int,int> prioritiesUV)
    {
        Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();
        foreach (var floor in floors)
        {
            if (string.IsNullOrEmpty(floor.borderTexturePath)) continue;
            Texture2D texture2D = Resources.Load<Texture2D>(floor.borderTexturePath);

            textures.Add(floor.ID, texture2D);
            prioritiesUV.Add(floor.ID, floor.displayPriority); 
        }
        return textures;
    }

    public static void UnloadTextures(Dictionary<int, Texture2D> floors)
    {
        foreach (var floor in floors)
        {
           Resources.UnloadAsset(floor.Value);
        }
    }
}
