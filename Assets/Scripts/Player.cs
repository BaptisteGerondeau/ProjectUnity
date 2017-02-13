using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {


	public float jumpHeight;
	public float timeToApex;

	public float moveXSpeed = 6;
	public float timeToAccelerateAir = 0.5f;
	public float timeToAccelerateGround = 0.1f;

	float gravity; 
	float jumpYSpeed;
	float velocityXSmoothing;

	Vector2 velocity;

	Controller2D controller;

	void Start () {
		controller = GetComponent<Controller2D> ();
		gravity =  -((2*jumpHeight)/Mathf.Pow(timeToApex, 2))/2;
		jumpYSpeed = Mathf.Abs(gravity) * timeToApex;

	}

	void Update () {

		if (controller.collisionInfo.above || controller.collisionInfo.below) {
			velocity.y = 0;
		}

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (Input.GetKeyDown(KeyCode.Space) && controller.collisionInfo.below) {
			velocity.y = jumpYSpeed;
		}

		float velocityXTarget = input.x * moveXSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, velocityXTarget, ref velocityXSmoothing, ((controller.collisionInfo.below)?timeToAccelerateGround:timeToAccelerateAir));
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime);

		controller.DisplayColText ();
	}

}
