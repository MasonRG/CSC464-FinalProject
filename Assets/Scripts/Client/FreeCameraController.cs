using UnityEngine;

namespace Player.Cameras
{
	public class FreeCameraController : MonoBehaviour
	{
		private float startDelay = 0.5f;
		public float moveSpeed = 4f;
		public float turnSpeed = 5f;

		private bool active = true;
		public bool Active { get { return active; } set { active = value; } }

		void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		void Update()
		{
			if (Time.timeScale == 0) return;
			if (startDelay > 0) { startDelay -= Time.deltaTime; return; }

			if (!active) return;

			RotateCamera();
			MoveCamera();
		}

		const float DAMP = 0.10f;
		const float THRESHOLD = 0.10f;
		Vector3 move = new Vector3();
		private void MoveCamera()
		{
			float spd = (Input.GetKey(KeyCode.LeftShift) ? 3.0f : 1f) * moveSpeed * 10f * Time.deltaTime;
			float fac = 1 + (Time.deltaTime / Time.fixedDeltaTime);

			float Z = Input.GetAxisRaw("move_foreback");
			float X = Input.GetAxisRaw("move_rightleft");
			float Y = Input.GetAxisRaw("move_updown");
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

			float mouseX = Input.GetAxis("mouse_x");
			float mouseY = Input.GetAxis("mouse_y");

			float speed = 10 * turnSpeed * Time.deltaTime;
			float yaw = speed * mouseX;
			float pitch = speed * mouseY;

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