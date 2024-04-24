using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Components;
using Coffee.UIExtensions;

[Serializable]
public class AttractedParticle
{
    public int creditValue = 10;
    public GameObject particleSystemPrefab;
    public UIParticleAttractor attractor;
}

public class EndLevelUnlockGroup : MonoBehaviour {

    public Button unlockButtonEnabled;
    public Button unlockButtonDisabled;
    public UIParticle unlockButtonParticles;
    public float durationUnlockButtonParticles = 2.5f;
    public LevelProgressBar progressBar;
    [Tooltip("Duration Range = [x, y], Particles Count Range = [z, w]")]
    public Vector4 durationToEmitAttractedParticlesMapping = new Vector4(2, 7, 1, 400);
    public AnimationCurve attractedParticlesCurve;
    public List<AttractedParticle> attractedParticles;

    protected GameManager gm;
    protected GoalLevel goalLevel;

    public void Initialize(GoalLevel goalLevel) {
        gm = GameManager.Instance;
        this.goalLevel = goalLevel;
        unlockButtonParticles.gameObject.SetActive(false);
        progressBar.Initialize(maxValue: goalLevel.treshold, currentValue: gm.goalManager.GetTotalCreditScore());
        progressBar.onReachMaxValueVisual.AddListener(SwapToEnabledUnlockButton);
        InitializeAttractedParticles();
    }

    protected void InitializeAttractedParticles() {
        attractedParticles = attractedParticles.OrderByDescending(p => p.creditValue).ToList();
        Transform scoreCounterTransform = gm.GetInfiniteMap().scoreDisplayer.displayText.transform;
        foreach (AttractedParticle attractedParticle in attractedParticles) {
            ParticleSystem particleSystem = Instantiate(attractedParticle.particleSystemPrefab, parent: scoreCounterTransform).GetComponentInChildren<ParticleSystem>();
            attractedParticle.attractor.gameObject.SetActive(false);
            attractedParticle.attractor.particleSystem = particleSystem;
            attractedParticle.attractor.gameObject.SetActive(true);
        }
    }

    public void Display() {
        //DisplayProgressBar();
        DisplayUnlockButton();
        PlayAttractedParticles();
    }

    protected void PlayAttractedParticles() {
        List<ParticleSystem> particlesToPlay = GetParticlesToPlay();
        StartCoroutine(CPlayParticlesToPlay(particlesToPlay));
    }

    protected float GetDurationToEmitAttractedParticles(int nbOfParticles) {
        Vector4 m = durationToEmitAttractedParticlesMapping;
        float duration = MathCurves.Remap(nbOfParticles, m.z, m.w, m.x, m.y);
        return Mathf.Clamp(duration, m.x, m.y);
    }

    protected IEnumerator CPlayParticlesToPlay(List<ParticleSystem> particlesToPlay) {
        Timer timer = new UnpausableTimer(GetDurationToEmitAttractedParticles(particlesToPlay.Count));
        Dictionary<ParticleSystem, int> particleBatch = new Dictionary<ParticleSystem, int>();
        particlesToPlay.Distinct().ToList().ForEach(p => particleBatch[p] = 0);
        for (int i = 0; i < particlesToPlay.Count; i++) {
            float avancement = (float)i / particlesToPlay.Count;
            if(avancement > attractedParticlesCurve.Evaluate(timer.GetAvancement())) {
                SendParticleBatch(particleBatch);
                particlesToPlay.Distinct().ToList().ForEach(p => particleBatch[p] = 0);
                yield return null;
                i--;
                continue;
            }
            particleBatch[particlesToPlay[i]] += 1;
        }
        SendParticleBatch(particleBatch);
    }

    protected void SendParticleBatch(Dictionary<ParticleSystem, int> particleBatch) {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        foreach (KeyValuePair<ParticleSystem, int> pair in particleBatch) {
            pair.Key.Emit(emitParams, pair.Value);
        }
    }

    protected List<ParticleSystem> GetParticlesToPlay() {
        int score = gm.GetInfiniteMap().scoreManager.GetCurrentScore();
        int multiplicator = score <= 100_000 ? 5 : score >= 5_000_000 ? 9 : 8;
        List<ParticleSystem> triggeredParticles = new List<ParticleSystem>();
        foreach (AttractedParticle attractedParticle in attractedParticles) {
            bool isLastParticles = attractedParticle == attractedParticles.Last();
            int nb = !isLastParticles ? Mathf.FloorToInt(score / attractedParticle.creditValue)
                : Mathf.CeilToInt((float)score / attractedParticle.creditValue);
            if (nb >= 2 && !isLastParticles) {
                nb = multiplicator * nb / 10;
            }
            score = score - nb * attractedParticle.creditValue;
            triggeredParticles.AddRange(Enumerable.Repeat(attractedParticle.attractor.particleSystem, nb));
        }
        MathTools.Shuffle(triggeredParticles);
        return triggeredParticles;
    }

    public void SwapToEnabledUnlockButton() {
        unlockButtonEnabled.gameObject.SetActive(true);
        unlockButtonDisabled.gameObject.SetActive(false);
    }

    protected void DisplayUnlockButton() {
        bool hasReachedMaxValue = progressBar.IsFull();
        unlockButtonEnabled.gameObject.SetActive(hasReachedMaxValue);
        unlockButtonDisabled.gameObject.SetActive(!hasReachedMaxValue);
        string levelName = goalLevel.IsSet() ? goalLevel.GetNextMenuLevel().GetVisibleName() : "";
        unlockButtonEnabled.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { levelName };
        unlockButtonDisabled.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { levelName };
    }

    private void DisplayProgressBar() {
        int currentValue = gm.goalManager.GetTotalCreditScore();
        progressBar.SetCurrentValue(currentValue);
    }

    public void UnlockNextLevelButton() {
        UnlockPath();
        PlayUnlockButtonParticles();
        ReturnToSelectorIn(durationUnlockButtonParticles);
    }

    protected void PlayUnlockButtonParticles() {
        unlockButtonParticles.gameObject.SetActive(true);
        unlockButtonParticles.Play();
    }

    protected void ReturnToSelectorIn(float delay) {
        StartCoroutine(CReturnToSelectorIn(delay));
    }

    protected IEnumerator CReturnToSelectorIn(float delay) {
        yield return new WaitForSecondsRealtime(delay);
        gm.QuitterPartie();
    }

    protected void UnlockPath() {
        if (goalLevel.IsSet()) {
            goalLevel.GetPath().UnlockPath();
        }
    }
}
