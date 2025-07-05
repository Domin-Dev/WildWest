using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCRepulsedState : HeroState
{
    NPCController controller;
    public NPCRepulsedState(NPCController controller,HeroStateMachine HeroStateMachine) : base(HeroStateMachine)
    {
        this.controller = controller;
    }
    public override void EnterState()
    {
        controller.SetRepulse(90f);
    }
    public override void ExitState()
    {

    }
    public override void FrameUpdate()
    {
        controller.UpdateRepulse();
    }
    public override void FrameFixedUpdate()
    {

    }
}
