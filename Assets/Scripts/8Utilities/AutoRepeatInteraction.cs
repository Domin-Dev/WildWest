using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif
public class AutoRepeatInteraction : IInputInteraction
{
    public float initialDelay = 0.4f;
    public float repeatInterval = 0.10f; 

    static AutoRepeatInteraction()
    {
        InputSystem.RegisterInteraction<AutoRepeatInteraction>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Register()
    {
        InputSystem.RegisterInteraction<AutoRepeatInteraction>();
    }

    public void Process(ref InputInteractionContext context)
    {
        if (context.timerHasExpired)
        {
            context.PerformedAndStayStarted();
            context.SetTimeout(repeatInterval);
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Waiting:
                if (context.ControlIsActuated())
                {
                    context.Started();
                    context.PerformedAndStayStarted(); 
                    context.SetTimeout(initialDelay);  
                }
                break;

            case InputActionPhase.Started:
                if (!context.ControlIsActuated())
                {
                    context.Canceled();
                }
                break;
        }
    }

    public void Reset()
    {
    }
}