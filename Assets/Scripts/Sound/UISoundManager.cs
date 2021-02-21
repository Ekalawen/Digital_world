using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UISoundManager : MonoBehaviour {
    static UISoundManager _instance;
    public static UISoundManager Instance { get { return _instance ?? (_instance = new GameObject().AddComponent<UISoundManager>()); } }

    public SoundBankUI sounds;

    protected Transform globalSoundsFolder;
    protected List<AudioSource> availableSources;
    protected List<AudioSource> usedSources;
    protected float musicVolume = 1.0f;
    protected float soundVolume = 1.0f;

    void Awake() {
        if (!_instance) { _instance = this; }
        Initialize();
    }

    public void Initialize() {
        name = "UISoundManager";
        globalSoundsFolder = new GameObject("Sounds").transform;
        globalSoundsFolder.parent = transform;
        availableSources = new List<AudioSource>();
        usedSources = new List<AudioSource>();
        DontDestroyOnLoad(this);
        StartCoroutine(CInitialize());
    }

    protected IEnumerator CInitialize() {
        AsyncOperationHandle<SoundBankUI> handle = Addressables.LoadAssetAsync<SoundBankUI>("Assets/Prefabs/Sound/SoundBankUI.asset");
        yield return handle;
        sounds = handle.Result;
    }

    public AudioSource GetAvailableSource() {
        // On vérifie si celles déjà utilisées ont finit d'être utilisées
        for (int i = 0; i < usedSources.Count; i++) {
            AudioSource source = usedSources[i];
            if (source == null) {
                usedSources.RemoveAt(i);
                i--;
                continue;
            }
            if (source.isPlaying == false) {
                availableSources.Add(source);
                usedSources.RemoveAt(i);
                i--;
                continue;
            }
        }

        // Si on en a déjà de disponible, c'est cool ! :D
        if (availableSources.Count > 0) {
            AudioSource res = availableSources[0];
            availableSources.RemoveAt(0);
            usedSources.Add(res);
            return res;
        }

        // Sinon on va devoir en créer une ! :)
        AudioSource newSource = new GameObject("Source").AddComponent<AudioSource>();
        AudioClipParamsHolder clipHolder = newSource.gameObject.AddComponent<AudioClipParamsHolder>();
        usedSources.Add(newSource);
        return newSource;
    }

    protected AudioSource PlayClipsOnSource(
        AudioClipParams audioClipParams,
        Vector3 pos = new Vector3(),
        Transform parent = null,
        float duration = -1.0f) {

        // On get la source
        AudioSource source = GetAvailableSource();

        // On set le volume
        if(audioClipParams.bIsMusic) {
            source.volume = musicVolume;
        } else {
            source.volume = soundVolume;
        }
        source.volume *= AudioClipParams.BASE_VOLUME * audioClipParams.relativeVolume;

        // On positionne la source
        source.transform.position = pos;
        source.transform.SetParent((parent == null) ? globalSoundsFolder: parent);

        // On get le bon clip
        AudioClip clip = audioClipParams.clips[Random.Range(0, audioClipParams.clips.Count)];
        source.clip = clip;
        source.GetComponent<AudioClipParamsHolder>().clipParams = audioClipParams;

        // On vérifie si c'est reverse ou pas
        if(audioClipParams.bReverse) {
            source.timeSamples = source.clip.samples - 1;
            source.pitch = -1;
        } else {
            source.timeSamples = 0;
            source.pitch = 1;
        }

        // On ajuste le pitch pour matcher avec la duration si celle-ci est spécifiée ! :)
        if(duration == -1) {
            source.pitch = source.pitch / Mathf.Abs(source.pitch); // Normalize à 1
        } else {
            float acceleration = clip.length / duration;
            source.pitch *= acceleration;
        }

        // On set le spatial blend !
        source.spatialBlend = audioClipParams.spatialBlend;

        // On vérifie si ça loop ou pas !
        source.loop = audioClipParams.bLoop;

        // Et enfin on joue la source ! <3
        source.Play();

        return source;
    }

    public void PlayHoverButtonClip() {
        PlayClipsOnSource(sounds.hoverButtonClips);
    }
    public void PlayClickedButtonClip() {
        PlayClipsOnSource(sounds.clickedButtonClips);
    }
}
