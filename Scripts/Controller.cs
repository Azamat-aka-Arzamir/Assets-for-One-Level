using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class Controller : MonoBehaviour
{
    Movement selfMove;
    float x;
    float y;
    Vector2 input;
    // Start is called before the first frame update
    void Start()
    {
        selfMove = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        selfMove.Move(input);
    }
    public void GetMovement(InputAction.CallbackContext context)
	{
        x = context.ReadValue<Vector2>().x;
        input = context.ReadValue<Vector2>();
    }
    public void GetJump(InputAction.CallbackContext context)
	{
		if (context.performed && !context.started)
		{
            selfMove.Jump();
		}
	}
    public void GetDash(InputAction.CallbackContext context)
	{
        if (context.performed && !context.started)
        {
            selfMove.Dash();
        }
    }
    public void GetAttack(InputAction.CallbackContext context)
	{
        if (context.performed && !context.started)
        {
            selfMove.Attack(selfMove.First);
        }
    }
}
