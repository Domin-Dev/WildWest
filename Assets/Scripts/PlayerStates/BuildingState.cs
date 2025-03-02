
using System;
using UnityEngine;

public class BuildingState : HeroState
{
    private MyCharacterController controller;

    public BuildingState(MyCharacterController controller, HeroStateMachine heroStateMachine) : base(heroStateMachine)
    {
        this.controller = controller;
    }
    public override void EnterState() 
    {
        BuildingManager.instance.StartBuildingMode(controller.handsController.selectedItem.itemID);
    }
    public override void ExitState() 
    {
        BuildingManager.instance.EndBuildingMode();
    }
    public override void FrameUpdate()
    {
        controller.GetMovementInput();
        controller.handsController.Aim();
        controller.UpdateFlip();
        controller.UpdateCharacterSprites();
    }

    public override void FrameFixedUpdate()
    {
        controller.UpdateMovement();
    }

}

