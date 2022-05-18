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
            selfMove.Jump(1);
		}
	}
    IEnumerator JumpForce(bool perf)
	{
        float a = 0;
		while (perf&&a<1)
		{
            a += 0.01f;
            yield return new WaitForFixedUpdate();
		}
        print(a);
        yield break;
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
