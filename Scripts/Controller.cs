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
		selfMove.Move(input, false, selfMove.LocalAcceleration);
	}
	public void GetMovement(InputAction.CallbackContext context)
	{
		x = context.ReadValue<Vector2>().x;
		input = context.ReadValue<Vector2>();
	}
	bool jumpEnd = false;
	public void GetJump(InputAction.CallbackContext context)
	{
		if (!context.performed && context.started)
		{
			jumpEnd = false;
			StartCoroutine(JumpForce(Time.time));
		}
		if (!context.performed && !context.started)
		{
			jumpEnd = true;
		}

	}
	IEnumerator JumpForce(double timest)
	{
		System.Func<bool> ret = ()=>Time.time - timest > 0.1d||jumpEnd;
		yield return new WaitUntil(ret);
		var f = (float)(Time.time - timest) * 10;
		if (f < 0.7) f = 0.7f;
		if (f > 1) f = 1;
		selfMove.Jump(selfMove.JumpForce*f);
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
			selfMove.Attack(selfMove.First, "1");
		}
	}
	public void GetDefend(InputAction.CallbackContext context)
	{
		if (context.performed && !context.started)
		{
			selfMove.Attack(selfMove.Second, "2", true);
			print("suction");
		}
		if (context.canceled)
		{
			selfMove.Attack(selfMove.Second, "2", false);
			print("am cumming");
		}
	}
	public void Die()
	{
		SceneM.ReloadActiveScene();

	}
	public void GetShoot(InputAction.CallbackContext context)
	{
		if (context.performed && !context.started)
		{
			selfMove.Attack(selfMove.Third, "3");
		}
	}
}
