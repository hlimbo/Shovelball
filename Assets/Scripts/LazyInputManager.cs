using UnityEngine;
using System.Collections;

public class LazyInputManager : MonoBehaviour
{
    // PRIVATE STATIC VARIABLES
    private static PlayerInput[] inputs;
    private static bool[] needUpdate;

    // INITIALIZE
    void Awake()
    {
        Initialize();
    }

    // FIXED UPDATE
    void FixedUpdate()
    {
        for (int i = 0; i < needUpdate.Length; ++i)
            needUpdate[i] = true;
    }

    // GetInput: Input request by outside script. Main entry into InputManagerLazy.
    public static PlayerInput GetInput(int index)
    {
        // initialize array if needed
        if (inputs == null)
            Initialize();

        // update if needed
        if (needUpdate[index])
        {
            UpdateInput(index);
            needUpdate[index] = false;
        }

        // return input
        return inputs[index];
    }

    // Initialize static values if not intialized.
    private static void Initialize()
    {
        inputs = new PlayerInput[2];
        inputs[0] = new PlayerInput();
        inputs[1] = new PlayerInput();

        needUpdate = new bool[4] { true, true, true, true };
    }

    // Update inputs
    private static void UpdateInput(int index)
    {
        int num = index + 1;
        inputs[index].joyStickX = Input.GetAxisRaw("JoystickX" + num);
        inputs[index].joyStickY = Input.GetAxisRaw("JoystickY" + num);
        inputs[index].jump = Input.GetButton("Jump" + num);
        inputs[index].attackUp = Input.GetButton("AttackUp" + num);
        inputs[index].attackForward = Input.GetButton("AttackForward" + num);
    }
}
