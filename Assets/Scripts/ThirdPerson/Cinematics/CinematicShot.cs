using System;
using UnityEngine;

namespace LastGod.ThirdPerson.Cinematics
{
    [Serializable]
    public class CinematicShot
    {
        public string shotName;
        public Vector3 cameraPosition;
        public Vector3 cameraEulerAngles;
        public float fov = 60f;
        public float duration = 3.0f;
        public string speakerName;
        [TextArea(1, 3)]
        public string dialogueText;
        public AudioClip sfxClip;
    }
}
