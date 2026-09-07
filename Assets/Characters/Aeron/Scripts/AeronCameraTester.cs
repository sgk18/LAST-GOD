using UnityEngine;
using Input = UnityEngine.Input;

namespace LastGod.Characters.Aeron
{
    public class AeronCameraTester : MonoBehaviour
    {
        public enum ViewDistance
        {
            FullBody,
            MediumShot,
            CloseUp
        }

        [Header("Target Framing")]
        [SerializeField] private Transform target;
        [SerializeField] private ViewDistance currentView = ViewDistance.FullBody;

        [Header("Framing Offsets")]
        [SerializeField] private Vector3 fullBodyOffset = new Vector3(0f, 3.2f, -8.0f);
        [SerializeField] private Vector3 mediumOffset = new Vector3(0f, 4.2f, -4.6f);
        [SerializeField] private Vector3 closeUpOffset = new Vector3(0f, 5.4f, -2.4f);

        [Header("Interpolation")]
        [SerializeField] private float smoothSpeed = 6.0f;

        private Vector3 _targetPos;

        private void Start()
        {
            SetView(currentView);
        }

        private void Update()
        {
            try
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1)) SetView(ViewDistance.FullBody);
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2)) SetView(ViewDistance.MediumShot);
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3)) SetView(ViewDistance.CloseUp);
            }
            catch (System.InvalidOperationException)
            {
                // Legacy input inactive
            }

            Vector3 basePos = target != null ? target.position : Vector3.zero;
            Vector3 desiredPos = basePos + _targetPos;
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);

            Vector3 lookTarget = basePos + (currentView switch
            {
                ViewDistance.FullBody => Vector3.up * 3.2f,
                ViewDistance.MediumShot => Vector3.up * 4.2f,
                ViewDistance.CloseUp => Vector3.up * 5.4f,
                _ => Vector3.up * 3.2f
            });

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position), Time.deltaTime * smoothSpeed);
        }

        public void SetView(ViewDistance view)
        {
            currentView = view;
            _targetPos = view switch
            {
                ViewDistance.FullBody => fullBodyOffset,
                ViewDistance.MediumShot => mediumOffset,
                ViewDistance.CloseUp => closeUpOffset,
                _ => fullBodyOffset
            };
        }

        private void OnGUI()
        {
            float width = 360f;
            float x = (Screen.width - width) * 0.5f;
            float y = 20f;

            GUI.color = new Color(0.02f, 0.04f, 0.08f, 0.85f);
            GUI.DrawTexture(new Rect(x - 10, y - 5, width + 20, 50), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(x, y, width, 45));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("[1] FULL BODY", GUILayout.Height(32))) SetView(ViewDistance.FullBody);
            if (GUILayout.Button("[2] MEDIUM SHOT", GUILayout.Height(32))) SetView(ViewDistance.MediumShot);
            if (GUILayout.Button("[3] CLOSE-UP", GUILayout.Height(32))) SetView(ViewDistance.CloseUp);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
