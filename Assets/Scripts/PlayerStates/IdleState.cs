
using UnityEngine;

public class IdleState : HeroState
{
    private CharacterController controller;
    public IdleState(CharacterController controller, HeroStateMachine heroStateMachine) : base(heroStateMachine)
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

        if(Input.GetMouseButton(0) && controller.handsController.canAttack)
        {    
            heroStateMachine.ChangeState(controller.attackState);
            controller.handsController.Use();
        }

        if(Input.GetKeyDown(KeyCode.R) && controller.handsController.CanReload())
        {
            heroStateMachine.ChangeState(controller.reloadingState);
        }
    }

    public override void FrameFixedUpdate()
    {
        controller.UpdateMovement();
    }

}

