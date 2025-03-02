using System;
using UnityEngine;
public class SideActionState : HeroState
{
    MyCharacterController controller;
    public SideActionState(MyCharacterController controller, HeroStateMachine heroStateMachine) : base(heroStateMachine)
    {
        this.controller = controller;
    }

    public override void EnterState() 
    {
        if (!controller.handsController.isGun)
        {
            controller.handsController.SetAttackVector(new Vector3(0, 0, 100), new Vector3(0.06f, 0, 0));
            Actions.instance.SideAction(controller.handsController.selectedItem);
        }
        else
        {
            if ((controller.handsController.selectedItem as RangedWeaponItem).HasAmmo())
            {
                controller.handsController.Shot();
                controller.handsController.SetAttackVector(new Vector3(0, 0, -60), new Vector3(-0.08f, 0, 0));
            }
            else
            {
                Sounds.instance.Empty();
                //controller.heroStateMachine.ChangeState(controller.idleState);
                controller.handsController.SetDefaultState();
            }
        }
        UIManager.instance.PrintTileInfo();
    }
    public override void ExitState() 
    {

    }
    public override void FrameUpdate() 
    {
        controller.GetMovementInput();
        controller.handsController.updaterAttack.Update();
    }
    public override void FrameFixedUpdate()
    {
        controller.UpdateMovement();
    }
}

