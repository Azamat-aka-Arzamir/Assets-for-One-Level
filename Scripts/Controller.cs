using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Events;

public class Controller : MonoBehaviour
{
	Movement selfMove;
	float x;
	float y;
	Vector2 input;
	bool dead;
	[SerializeField]Weapon gunToFollow;
	[SerializeField] GameObject DeathScreen;
	public UnityEvent FirstAttack = new UnityEvent();
	public UnityEvent Defend = new UnityEvent();
	public UnityEvent DefendStop = new UnityEvent();
	public UnityEvent ThirdAttack = new UnityEvent();
	public IntContextEvent lookUp = new IntContextEvent();

	public Weapon FirstGunSlot;
	public SimpleAnimHolder animHolder;
	// Start is called before the first frame update
	void Start()
	{
		selfMove = GetComponent<Movement>();
		DeathScreen.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
		InitializeActiveGun();
	}

	// Update is called once per frame
	void Update()
	{
		selfMove.Move(input, false, selfMove.LocalAcceleration);
	}
	public void GetPause(InputAction.CallbackContext context)
	{
		Debug.Break();
	}
	public void GetMovement(InputAction.CallbackContext context)
	{
		//x = context.ReadValue<Vector2>().x;
		input = context.ReadValue<Vector2>();
		if (Mathf.Abs(input.y) < 0.7f)
		{
			lookUp.Invoke(0);
		}
		else
		{
			lookUp.Invoke((int)Mathf.Sign(input.y));
		}
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
	void InitializeActiveGun()
	{
		ThirdAttack.RemoveAllListeners();
		ThirdAttack.AddListener(FirstGunSlot.Fire);
		FirstGunSlot.Shoot.RemoveAllListeners();
		FirstGunSlot.Shoot.AddListener(PlayFire);
	}
	void ChangeGuns()
	{
		if (gunToFollow == null) return;
		//animHolder.Animators.Remove(FirstGunSlot.GetComponent<CustomAnimator>());
		var v = FirstGunSlot.transform.parent;
		FirstGunSlot.transform.parent = null;
		//FirstGunSlot.GetComponent<SpriteRenderer>().sprite = FirstGunSlot.GetComponent<CustomAnimator>().AllAnims.Find(x => x.animName == "GunIdle").frames[0];
		FirstGunSlot.GetComponent<CustomAnimator>().enabled = false;
		FirstGunSlot = gunToFollow;
		//animHolder.Animators.Add(FirstGunSlot.GetComponent<CustomAnimator>());
		FirstGunSlot.transform.parent = v;
		FirstGunSlot.GetComponent<CustomAnimator>().enabled = true;
		FirstGunSlot.Initialize();
		//FirstGunSlot.GetComponent<CustomAnimator>().ChangeSide(animHolder.side);
		InitializeActiveGun();
	}

	void PlayFire()
	{
		//animHolder.PlayAnim("Fire");
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
		print("semen");
		if (context.performed && !context.started)
		{
			if (!selfMove.IsAttack)
			{
                FirstAttack.Invoke();
            }
			//selfMove.SelfAnim.SetTrigger("Attack 1");
		}
	}
	public void GetDefend(InputAction.CallbackContext context)
	{
		if (context.performed && !context.started)
		{
			//selfMove.SelfAnim.SetBool("Attack 2",true);
			Defend.Invoke();
		}
		if (context.canceled)
		{
			//selfMove.SelfAnim.SetBool("Attack 2", false);
			DefendStop.Invoke();
		}
	}
	public void Die()
	{
		dead = true;
		DeathScreen.transform.SetAsLastSibling();
		DeathScreen.GetComponent<RectTransform>().sizeDelta = DeathScreen.transform.parent.GetComponent<RectTransform>().sizeDelta;
		DeathScreen.GetComponent<UnityEngine.UI.Image>().color = Color.white;
	}
	public void GetReload(InputAction.CallbackContext context)
	{
		if (context.performed && !context.started)
		{
			if (dead)
			{
				SceneM.ReloadActiveScene();
			}
		}
	}
	public void GetShoot(InputAction.CallbackContext context)
	{
		if (context.performed && !context.started)
		{
			//selfMove.SelfAnim.SetTrigger("Attack 3");
			ChangeGuns();
			ThirdAttack.Invoke();
		}
	}
	private void OnTriggerStay2D(Collider2D collision)
	{
		if (gunToFollow != null) return;
		Weapon gun = null;
		collision.TryGetComponent(out gun);
		if (gun != null&&gun!=FirstGunSlot&&gun!=gunToFollow&&gun.weaponType== Weapon.type.gun&&gun.transform.parent==null)
		{
			gunToFollow = gun;
			StartCoroutine(IeFollowGun(gunToFollow));
		}
		else return;
	}
	IEnumerator IeFollowGun(Weapon gun)
	{
		yield return new WaitWhile(() => (gun.transform.position - transform.position).magnitude < 2);
		gunToFollow = null;
	}
}
