
using UnityEngine;

public class IdleState : HeroState
{
    private MyCharacterController controller;
    public IdleState(MyCharacterController controller, HeroStateMachine heroStateMachine) : base(heroStateMachine)
    {
        this.controller = controller;
    }
    public override void EnterState() 
    { 
    
    }
    public override void ExitState() 
    { 
    
    }
    public override void FrameUpdate()
    {
        controller.GetMovementInput();
        controller.handsController.Aim();
        controller.UpdateFlip();
        controller.UpdateCharacterSprites();

        if (controller.handsController.canAttack)
        {
            if (Input.GetMouseButton(0))
            {
                heroStateMachine.ChangeState(controller.attackState);
                controller.handsController.Use();
            }
            else if (Input.GetMouseButton(1))
            {
                heroStateMachine.ChangeState(controller.sideActionState);
                controller.handsController.Use();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && controller.handsController.CanReload())
        {
            heroStateMachine.ChangeState(controller.reloadingState);
        }
    }

    public override void FrameFixedUpdate()
    {
        controller.UpdateMovement();
    }

}

