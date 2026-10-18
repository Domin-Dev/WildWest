using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter( WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class WaitForConfirmation : SystemBase
{
    public static event Action<CharacterLook?> connection;
    public CharacterLook? look;

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        bool receivedAnswer = false;
        foreach (var (answer, entity) in SystemAPI.Query<RefRO<AnswerPlayerVerificationRPC>>().WithEntityAccess())
        {
            var rpc = answer.ValueRO;
            look = rpc.playerDataIsOnServer ? rpc.characterLook : null;
            ecb.DestroyEntity(entity);
            receivedAnswer = true;
            break;
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
        if (receivedAnswer)
        {
            connection?.Invoke(look);
            Enabled = false;
        }
    }
}
