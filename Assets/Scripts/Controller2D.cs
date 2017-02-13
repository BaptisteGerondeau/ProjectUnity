using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	public LayerMask collisionMask;

	public float skinWidth = .015f;
	public float horizontalSpaceRaySpacing = 0.5f;
	public float verticalSpaceRaySpacing = 0.5f;

	int verticalSpaceRayNumber;
	int horizontalSpaceRayNumber;

	public float maxClimbSlope = 80;
	public float maxDescendSlope = 80;

	new BoxCollider2D collider;
	RayCastOrigins raycastOrigins;
	public CollisionHelper collisionInfo;

	public Text collisionText;

	void Start() {
		collider = GetComponent<BoxCollider2D> ();
		CalculateRaySpacing ();
	}

	public void Move (Vector2 displacement) {
		UpdateRaycastOrigins ();
		collisionInfo.Reset ();

		if (displacement.y < 0) {

		}

		if (displacement.x != 0) {                                     
			HorizontalCollisions (ref displacement);
		}
		if (displacement.y != 0) {
			VerticalCollisions (ref displacement);
		}

		transform.Translate (displacement);
	}
		
	void HorizontalCollisions (ref Vector2 displacement) {
		float directionRight = Mathf.Sign (displacement.x);
		float rayLength = Mathf.Abs (displacement.x) + skinWidth;

		for (int i = 0; i < horizontalSpaceRayNumber; i++) {
			Vector2 rayOrigin = (directionRight == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;

			if (i == horizontalSpaceRayNumber-1) {
				rayOrigin = (directionRight == -1) ? raycastOrigins.topLeft : raycastOrigins.topRight;
			} else {
				rayOrigin += Vector2.up * (i * horizontalSpaceRaySpacing);
			}

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionRight, rayLength, collisionMask);
			Debug.DrawRay (rayOrigin, Vector2.right * directionRight * rayLength, Color.red);

			if (hit) {

				if (hit.distance == 0) {
					continue;
				}

				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);

				// et si i = dernier rayon ?
				if (i == 0 && slopeAngle < maxClimbSlope) {


					float distanceToSlope = 0;

					if (slopeAngle != collisionInfo.slopeAngleOld) {
						distanceToSlope = hit.distance - skinWidth;
						displacement.x -= distanceToSlope * directionRight; //On enleve l'"energie" qu'il faut au cube pour atteindre la pente
					}

					ClimbSlope(ref displacement, slopeAngle);
					displacement.x += distanceToSlope * directionRight;
				}

				if (!collisionInfo.climbingSlope || slopeAngle > maxClimbSlope) {
					displacement.x = (hit.distance - skinWidth) * directionRight;
					rayLength = hit.distance;

					if (collisionInfo.climbingSlope) {
						displacement.y = Mathf.Tan (collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (displacement.x);
					}

					collisionInfo.left = (directionRight == -1);
					collisionInfo.right = (directionRight == 1);
				}
				

			}
		}
	}

	void VerticalCollisions (ref Vector2 displacement) {
		float directionUp = Mathf.Sign (displacement.y);
		float rayLength = Mathf.Abs (displacement.y) + skinWidth;

		for (int i = 0; i < verticalSpaceRayNumber; i++) {
			Vector2 rayOrigin = (directionUp == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;

			if (i == verticalSpaceRayNumber - 1) {
				rayOrigin = (directionUp == -1)?raycastOrigins.bottomRight:raycastOrigins.topRight;
			} else {
				rayOrigin += Vector2.right * (i * verticalSpaceRaySpacing + displacement.x);
			}

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionUp, rayLength, collisionMask);
			Debug.DrawRay (rayOrigin, Vector2.up * directionUp * rayLength, Color.red);


			if (hit) {
				displacement.y = (hit.distance - skinWidth) * directionUp;
				rayLength = hit.distance;

				if (collisionInfo.climbingSlope) {
					displacement.x = displacement.y / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(displacement.x);
				}

				collisionInfo.above = (directionUp == 1);
				collisionInfo.below = (directionUp == -1);
			}
		}

		if (collisionInfo.climbingSlope) {
			
			float directionX = Mathf.Sign(displacement.x);
			rayLength = Mathf.Abs(displacement.x) + skinWidth;

			Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * displacement.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionUp,rayLength,collisionMask);

			if (hit) {
				
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				if (slopeAngle != collisionInfo.slopeAngle) {
					
					displacement.x = (hit.distance - skinWidth) * directionX;
					collisionInfo.slopeAngle = slopeAngle;
					collisionInfo.slopeNormal = hit.normal;
				}
			}
		}

	}

	void DescendSlope(ref Vector2 displacement) {
		float directionX = Mathf.Sign (displacement.x);
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);

		if (hit) {
			
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

			if (slopeAngle != 0 && slopeAngle <= maxDescendSlope) {
				if (Mathf.Sign(hit.normal.x) == directionX) {
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x)) {
						float moveDistance = Mathf.Abs(displacement.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						displacement.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (displacement.x);
						displacement.y -= descendVelocityY;

						collisionInfo.slopeAngle = slopeAngle;
						collisionInfo.descendingSlope = true;
						collisionInfo.below = true;
					}
				}
			}
		}
	}


	void ClimbSlope (ref Vector2 displacement, float slopeAngle) {
		float moveDistance = Mathf.Abs(displacement.x);
		float climbHeightY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (displacement.y <= climbHeightY) {
			displacement.y = climbHeightY;
			displacement.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (displacement.x);
			collisionInfo.below = true;
			collisionInfo.climbingSlope = true;
			collisionInfo.slopeAngle = slopeAngle;
		}

	}

	public void DisplayColText () {
		collisionText.text = "Collision Above :" + collisionInfo.above.ToString() + System.Environment.NewLine + "Collsion Below :" + collisionInfo.below.ToString() + System.Environment.NewLine;
		collisionText.text += "Collision Left :" + collisionInfo.left.ToString() + System.Environment.NewLine + "Collsion Right :" + collisionInfo.right.ToString() + System.Environment.NewLine;
		collisionText.text += "Walking up slope :" + collisionInfo.climbingSlope.ToString() + System.Environment.NewLine + "SlopeAngle :" + collisionInfo.slopeAngle.ToString() + System.Environment.NewLine;
	}

	void UpdateRaycastOrigins() {
		Bounds raycastBounds = collider.bounds;
		raycastBounds.Expand (skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2 (raycastBounds.min.x, raycastBounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (raycastBounds.max.x, raycastBounds.min.y);
		raycastOrigins.topLeft = new Vector2 (raycastBounds.min.x, raycastBounds.max.y);
		raycastOrigins.topRight = new Vector2 (raycastBounds.max.x, raycastBounds.max.y);
	}

	void CalculateRaySpacing() {
		Bounds raycastBounds = collider.bounds;
		raycastBounds.Expand (skinWidth * -2);

		verticalSpaceRaySpacing = Mathf.Clamp (verticalSpaceRaySpacing, 0.01f, raycastBounds.size.y);
		horizontalSpaceRaySpacing = Mathf.Clamp (horizontalSpaceRaySpacing, 0.01f, raycastBounds.size.x);

		verticalSpaceRayNumber = (int)(( raycastBounds.size.y - ( Mathf.Repeat(raycastBounds.size.y , verticalSpaceRaySpacing) ) ) / verticalSpaceRaySpacing ) +1;
		horizontalSpaceRayNumber = (int)(( raycastBounds.size.x - ( Mathf.Repeat(raycastBounds.size.x , horizontalSpaceRaySpacing))) / horizontalSpaceRaySpacing) +1;
	}

	struct RayCastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	public struct CollisionHelper {
		public bool above, below;
		public bool right, left;
		public bool climbingSlope, descendingSlope;

		public float slopeAngle, slopeAngleOld;
		public Vector2 slopeNormal;

		public void Reset () {
			above = below = false;
			right = left = false;
			climbingSlope = descendingSlope =  false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}

	}
}
