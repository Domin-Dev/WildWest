
using UnityEngine;

public class NPCIdleState : HeroState
{
    NPCController controller;
    public NPCIdleState(NPCController controller, HeroStateMachine heroStateMachine) : base(heroStateMachine)
    {
        this.controller = controller;
    }
    public override void EnterState() 
    { 
        //
    }
    public override void ExitState() 
    { 
    
    }
    public override void FrameUpdate()
    {       
        if (controller.isTarget)
        {
            heroStateMachine.ChangeState(controller.followState);
        }
    }
    public override void FrameFixedUpdate()
    {
        base.FrameFixedUpdate();
    }


}

