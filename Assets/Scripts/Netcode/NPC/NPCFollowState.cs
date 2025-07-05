
using UnityEngine;

public class NPCFollowState : HeroState
{
    NPCController controller;

    public NPCFollowState(NPCController controller, HeroStateMachine heroStateMachine) : base(heroStateMachine)
    {
        this.controller = controller;
    }
    public override void EnterState() 
    { 

    }
    public override void ExitState() 
    {
        controller.StopFollow();
    }
    public override void FrameUpdate()
    {

        if (controller.isTarget)
        {
            float distance = controller.GetDistance();
            if (distance < 0.4f && controller.canAttack)
            {
                controller.attackModule.Aim();
                controller.canAttack = false;
                heroStateMachine.ChangeState(controller.attackState);

            }else
            {
                controller.attackModule.Aim();
           }//else 
           // {
            //    controller.attackModule.UpdateShield();
          //  }

        }
        else
        {
            heroStateMachine.ChangeState(controller.idleState);
        }

    }
    public override void FrameFixedUpdate()
    {
        controller.Follow();
    }

}

