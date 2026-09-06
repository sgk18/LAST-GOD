using System;
using UnityEngine;

namespace LastGod.ThirdPerson.Audio
{
    public enum AudioBus
    {
        Music,
        Ambience,
        SFX,
        Voice,
        UI
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioSource uiSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureSources();
        }

        private void EnsureSources()
        {
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (ambienceSource == null) ambienceSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (voiceSource == null) voiceSource = gameObject.AddComponent<AudioSource>();
            if (uiSource == null) uiSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void PlayVoice(AudioClip clip, float volume = 1f)
        {
            if (clip != null && voiceSource != null)
            {
                voiceSource.PlayOneShot(clip, volume);
            }
        }

        public void SetMusic(AudioClip clip, bool loop = true, float volume = 0.6f)
        {
            if (musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.volume = volume;
                musicSource.Play();
            }
        }

        public void SetAmbience(AudioClip clip, bool loop = true, float volume = 0.5f)
        {
            if (ambienceSource != null)
            {
                ambienceSource.clip = clip;
                ambienceSource.loop = loop;
                ambienceSource.volume = volume;
                ambienceSource.Play();
            }
        }

        // ========================================================
        // PROCEDURAL AUDIO SYNTHESIZERS FOR ACT 1 CUES
        // ========================================================
        public static AudioClip GenerateHeartbeatClip()
        {
            int sampleRate = 44100;
            float length = 0.7f;
            int totalSamples = (int)(sampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float beat1 = (t < 0.22f) ? Mathf.Sin(2f * Mathf.PI * 55f * t) * Mathf.Exp(-t * 18f) : 0f;
                float t2 = t - 0.25f;
                float beat2 = (t2 >= 0f && t2 < 0.22f) ? Mathf.Sin(2f * Mathf.PI * 45f * t2) * Mathf.Exp(-t2 * 16f) * 0.75f : 0f;
                samples[i] = Mathf.Clamp(beat1 + beat2, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Heartbeat", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateAlarmKlaxonClip()
        {
            int sampleRate = 44100;
            float length = 1.2f;
            int totalSamples = (int)(sampleRate * length);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(620f, 960f, Mathf.PingPong(t * 3.5f, 1f));
                float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.4f;
                samples[i] = wave;
            }

            AudioClip clip = AudioClip.Create("Procedural_Alarm", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateGlassShatterClip()
        {
            int sampleRate = 44100;
            float length = 0.9f;
            int totalSamples = (int)(sampleRate * length);
            float[] samples = new float[totalSamples];
            System.Random rand = new System.Random(42);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float noise = ((float)rand.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 7f);
                float crackle = Mathf.Sin(2f * Mathf.PI * 2400f * t) * Mathf.Exp(-t * 9f) * 0.5f;
                samples[i] = Mathf.Clamp(noise + crackle, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_GlassShatter", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GeneratePunchImpactClip()
        {
            int sampleRate = 44100;
            float length = 0.35f;
            int totalSamples = (int)(sampleRate * length);
            float[] samples = new float[totalSamples];
            System.Random rand = new System.Random(7);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float thud = Mathf.Sin(2f * Mathf.PI * 80f * t) * Mathf.Exp(-t * 22f);
                float crunch = ((float)rand.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 28f) * 0.4f;
                samples[i] = Mathf.Clamp(thud + crunch, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Punch", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateGunshotClip()
        {
            int sampleRate = 44100;
            float length = 0.5f;
            int totalSamples = (int)(sampleRate * length);
            float[] samples = new float[totalSamples];
            System.Random rand = new System.Random(19);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float blast = ((float)rand.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 25f);
                float tone = Mathf.Sin(2f * Mathf.PI * 180f * t) * Mathf.Exp(-t * 14f) * 0.6f;
                samples[i] = Mathf.Clamp(blast + tone, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Gunshot", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
