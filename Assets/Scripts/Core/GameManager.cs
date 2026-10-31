using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastGod.Act1
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GamePhase { NarrativeIntro, CombatWave, NarrativeOutro, Results, GameOver }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.NarrativeIntro;

        [Header("Wave Configuration")]
        public int currentWave = 1;
        public int maxWaves = 3;
        public Transform[] spawnPointsRight;
        public Transform[] spawnPointsLeft;
        public GameObject standardGuardPrefab;
        public GameObject eliteGuardPrefab;

        [Header("Stats Tracking")]
        public int enemiesTaken = 0;
        public int enemiesCutDown = 0;
        public int bulletsReflected = 0;
        public int echoesPerformed = 0;
        public int hitsTaken = 0;

        private int activeEnemiesInWave = 0;
        private bool hasSaidReflect = false;
        private bool hasSaidEcho = false;
        private bool hasSaidTakeFirst = false;
        private bool hasSaidTakeHalf = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Initial state set by Act1LabDirector
        }

        private void Update()
        {
            if (CurrentPhase == GamePhase.Results || CurrentPhase == GamePhase.GameOver)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }

        public void StartCombatWaves()
        {
            CurrentPhase = GamePhase.CombatWave;
            currentWave = 1;
            UIManager.Instance?.ShowWaveBanner("WAVE 1");
            UIManager.Instance?.ShowControlsHint(true);
            SpawnWave(1);
        }

        private void SpawnWave(int waveNumber)
        {
            currentWave = waveNumber;
            activeEnemiesInWave = 0;

            if (waveNumber == 1)
            {
                // Wave 1: 3 standard guards from right
                StartCoroutine(SpawnGuardsRoutine(3, false, 1.2f));
            }
            else if (waveNumber == 2)
            {
                // Wave 2: 2 standard guards from right, 1 from left
                UIManager.Instance?.ShowWaveBanner("WAVE 2");
                StartCoroutine(SpawnGuardsRoutine(2, false, 1.0f));
                StartCoroutine(SpawnGuardsLeftRoutine(1, false, 1.5f));
            }
            else if (waveNumber == 3)
            {
                // Wave 3: 1 Elite Guard + 3 Standard guards
                UIManager.Instance?.ShowWaveBanner("WAVE 3 - SOMETHING STRONGER");
                StartCoroutine(SpawnGuardsRoutine(1, true, 0.5f)); // Elite
                StartCoroutine(SpawnGuardsRoutine(2, false, 2.0f));
                StartCoroutine(SpawnGuardsLeftRoutine(1, false, 2.5f));
            }
        }

        private IEnumerator SpawnGuardsRoutine(int count, bool elite, float delay)
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = (spawnPointsRight != null && spawnPointsRight.Length > 0)
                    ? spawnPointsRight[i % spawnPointsRight.Length].position
                    : new Vector3(12f + i * 1.5f, -0.6f, 0f);

                GameObject prefab = elite ? eliteGuardPrefab : standardGuardPrefab;
                if (prefab != null)
                {
                    Instantiate(prefab, pos, Quaternion.identity);
                    activeEnemiesInWave++;
                }
                yield return new WaitForSeconds(0.8f);
            }
        }

        private IEnumerator SpawnGuardsLeftRoutine(int count, bool elite, float delay)
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = (spawnPointsLeft != null && spawnPointsLeft.Length > 0)
                    ? spawnPointsLeft[i % spawnPointsLeft.Length].position
                    : new Vector3(-6f - i * 1.5f, -0.6f, 0f);

                GameObject prefab = elite ? eliteGuardPrefab : standardGuardPrefab;
                if (prefab != null)
                {
                    Instantiate(prefab, pos, Quaternion.identity);
                    activeEnemiesInWave++;
                }
                yield return new WaitForSeconds(0.8f);
            }
        }

        public void OnEnemyDefeated(GuardAI guard)
        {
            activeEnemiesInWave--;

            if (CurrentPhase == GamePhase.CombatWave && activeEnemiesInWave <= 0)
            {
                StartCoroutine(WaveClearRoutine());
            }
        }

        private IEnumerator WaveClearRoutine()
        {
            yield return new WaitForSeconds(1.8f);

            if (currentWave < maxWaves)
            {
                SpawnWave(currentWave + 1);
            }
            else
            {
                // Combat victory -> Transition to Outro Narrative
                Act1LabDirector.Instance?.StartOutroSequence();
            }
        }

        public void RecordTake(float newHumanity)
        {
            enemiesTaken++;
            if (!hasSaidTakeFirst && newHumanity <= 90f)
            {
                hasSaidTakeFirst = true;
                UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "YES. TAKE IT.", 2.2f);
            }
            else if (!hasSaidTakeHalf && newHumanity <= 50f)
            {
                hasSaidTakeHalf = true;
                UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "YOU ARE BECOMING WHAT I MADE.", 2.8f);
            }
            else if (newHumanity <= 15f)
            {
                UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "GOOD. FEEL NOTHING.", 2.4f);
            }
        }

        public void RecordCutDown()
        {
            enemiesCutDown++;
        }

        public void RecordReflect()
        {
            bulletsReflected++;
            if (!hasSaidReflect)
            {
                hasSaidReflect = true;
                UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "EVEN THEIR WEAPONS OBEY YOU.", 2.4f);
            }
        }

        public void RecordEcho()
        {
            echoesPerformed++;
            if (!hasSaidEcho)
            {
                hasSaidEcho = true;
                UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "YOU SAW IT BEFORE IT HAPPENED.", 2.4f);
            }
        }

        public void RecordHitTaken()
        {
            hitsTaken++;
        }

        public void OnPlayerDied()
        {
            CurrentPhase = GamePhase.GameOver;
            UIManager.Instance?.ShowGameOver();
        }

        public void TriggerResultsScreen()
        {
            CurrentPhase = GamePhase.Results;
            UIManager.Instance?.ShowResults(
                AeronController.Instance != null ? AeronController.Instance.humanity : 100f,
                enemiesTaken,
                enemiesCutDown,
                bulletsReflected,
                echoesPerformed,
                hitsTaken
            );
        }
    }
}

