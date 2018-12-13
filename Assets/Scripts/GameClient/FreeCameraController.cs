using Engine.Utilities;
using UnityEngine;

namespace GameClient
{
	public class FreeCameraController : MonoBehaviour
	{
		private float startDelay = 0.5f;
		public float moveSpeed = 4f;
		public float turnSpeed = 4f;

		private bool active = true;
		public bool Active { get { return active; } set { active = value; } }

		private bool CursorActive
		{
			get { return Cursor.lockState == CursorLockMode.None; }
			set
			{
				Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
				Cursor.visible = value;
			}
		}


		void Update()
		{
			if (Time.timeScale == 0) return;
			if (startDelay > 0) { startDelay -= Time.deltaTime; return; }

			if (!active) return;

			if (!NetworkHub.MessageConsoleActive)
			{
				if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
				{
					CursorActive = !CursorActive;
				}

				if (!CursorActive)
				{
					RotateCamera();
					MoveCamera();
				}
			}
		}

		const float DAMP = 0.20f;
		const float THRESHOLD = 0.10f;
		Vector3 move = new Vector3();
		void MoveCamera()
		{
			float spd = moveSpeed * 10f * Time.deltaTime;
			float fac = 1 + (Time.deltaTime / Time.fixedDeltaTime);


			float Z = Input.GetAxisRaw("move_backfront");
			float X = Input.GetAxisRaw("move_leftright");
			float Y = Input.GetAxisRaw("move_downup");
			move.z = Mathf.Lerp(move.z, Z, DAMP * fac);
			move.x = Mathf.Lerp(move.x, X, DAMP * fac);
			move.y = Mathf.Lerp(move.y, Y, DAMP * fac);
			if (Mathf.Abs(move.z) <= THRESHOLD) move.z = Z;
			if (Mathf.Abs(move.x) <= THRESHOLD) move.x = X;
			if (Mathf.Abs(move.y) <= THRESHOLD) move.y = Y;

			transform.position += transform.forward * move.z * spd;
			transform.position += transform.right * move.x * spd;
			transform.position += Vector3.up * move.y * spd;
		}

		private void RotateCamera()
		{
			const float BOUND = 80f;

			Vector3 euler = transform.eulerAngles;

			float mouseX = Input.GetAxisRaw("mouse_x");
			float mouseY = Input.GetAxisRaw("mouse_y");
			float yaw = turnSpeed * 10 * Time.deltaTime * mouseX;
			float pitch = turnSpeed * 10 * Time.deltaTime * mouseY;

			euler.x -= pitch;
			euler.y += yaw;
			euler.z = 0f;

			//Ensure x doesnt over-rotate
			//0 is middle -> goes 'down' to 90
			//360 also is middle -> goes 'up' to 270
			if (euler.x > BOUND && euler.x <= 180f)
				euler.x = BOUND;
			else if (euler.x < 360f - BOUND && euler.x > 180f)
				euler.x = 360f - BOUND;

			transform.eulerAngles = euler;
		}
	}
}