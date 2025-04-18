using Unity.NetCode;

[UnityEngine.Scripting.Preserve]
public class GameBootStrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        CreateClientWorld(defaultWorldName);
        return true;
    }

}
