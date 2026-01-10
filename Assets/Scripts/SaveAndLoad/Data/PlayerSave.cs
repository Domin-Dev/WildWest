
using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public struct PlayerSave
{
    public FixedString128Bytes playerName;
    public CharacterLook characterLook;
    public float2 playerPosition;

    public int health;
    public int hunger;
    public int thirst;

    public bool isAdmin;
}