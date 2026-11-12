using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossVoiceLinePlayer : MonoBehaviour
{
    const string VoiceResourcePath = "Audio/Boss/FelisVex";
    const int VoiceLineCount = 21;

    [SerializeField, Min(0f)] Vector2 firstLineDelay = new Vector2(2.5f, 4f);
    [SerializeField, Min(0f)] Vector2 lineCooldown = new Vector2(11f, 18f);

    BossG1BulletPattern boss;
    AudioSource voiceSource;
    AudioClip[] voiceLines;
    Coroutine initializeRoutine;
    int lastPlayedIndex = -1;

    void Awake()
    {
        boss = GetComponent<BossG1BulletPattern>();
        voiceSource = GetComponent<AudioSource>();
        if (voiceSource == null) voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f;
        voiceSource.ignoreListenerPause = false;

        voiceLines = new AudioClip[VoiceLineCount];
    }

    void OnEnable()
    {
        if (initializeRoutine == null) initializeRoutine = StartCoroutine(LoadAndPlayVoiceLines());
    }

    void OnDisable()
    {
        if (initializeRoutine != null) StopCoroutine(initializeRoutine);
        initializeRoutine = null;
        if (voiceSource != null) voiceSource.Stop();
    }

    IEnumerator LoadAndPlayVoiceLines()
    {
        int loaded = 0;
        for (int i = 0; i < voiceLines.Length; i++)
        {
            string resource = $"{VoiceResourcePath}/FelisVex_Fala_{i + 1:00}";
            ResourceRequest request = Resources.LoadAsync<AudioClip>(resource);
            yield return request;
            voiceLines[i] = request.asset as AudioClip;
            if (voiceLines[i] != null) loaded++;
        }

        if (loaded == 0)
        {
            Debug.LogWarning($"Boss: nenhuma fala encontrada em Resources/{VoiceResourcePath}.", this);
            initializeRoutine = null;
            yield break;
        }

        yield return new WaitForSeconds(Random.Range(firstLineDelay.x, firstLineDelay.y));

        List<int> shuffledBag = new List<int>(loaded);
        while (boss != null && !boss.IsDead)
        {
            if (!CanSpeak())
            {
                yield return null;
                continue;
            }

            if (shuffledBag.Count == 0) RefillBag(shuffledBag);
            int index = shuffledBag[shuffledBag.Count - 1];
            shuffledBag.RemoveAt(shuffledBag.Count - 1);
            AudioClip line = voiceLines[index];
            if (line == null) continue;
            voiceSource.PlayOneShot(line, AudioVolumeSettings.VoiceVolume);
            lastPlayedIndex = index;

            yield return new WaitForSeconds(line.length + Random.Range(lineCooldown.x, lineCooldown.y));
        }

        initializeRoutine = null;
    }

    bool CanSpeak()
    {
        return boss != null && !boss.IsDead &&
            (GameManager.instance == null || GameManager.instance.IsPlaying) &&
            Time.timeScale > 0f;
    }

    void RefillBag(List<int> bag)
    {
        for (int i = 0; i < voiceLines.Length; i++)
            if (voiceLines[i] != null) bag.Add(i);
        for (int i = bag.Count - 1; i > 0; i--)
        {
            int swap = Random.Range(0, i + 1);
            (bag[i], bag[swap]) = (bag[swap], bag[i]);
        }

        if (bag.Count > 1 && bag[bag.Count - 1] == lastPlayedIndex)
        {
            int swap = Random.Range(0, bag.Count - 1);
            (bag[swap], bag[bag.Count - 1]) = (bag[bag.Count - 1], bag[swap]);
        }
    }
}
