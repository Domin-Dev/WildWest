using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]

public partial class WaitForConfirmation : SystemBase
{

    public static event Action<CharacterLook?> connection;

    public CharacterLook? look;

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities.WithAll<AnswerPlayerVerificationRPC>().ForEach((AnswerPlayerVerificationRPC a, Entity e) =>
        {
            look = a.playerDataIsOnServer ? a.characterLook : null;
            connection?.Invoke(a.playerDataIsOnServer ? a.characterLook : null);
            this.Enabled = false;
            entityCommandBuffer.DestroyEntity(e);
        }).WithStructuralChanges().WithoutBurst().Run();



        entityCommandBuffer.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
