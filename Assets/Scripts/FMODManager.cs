using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StudioStopMode = FMOD.Studio.STOP_MODE;

/// <summary>
/// Ponto central de áudio do jogo. Carrega os bancos, mantém música e motor entre
/// cenas, aplica os volumes salvos e oferece chamadas sem dependência de prefabs.
/// </summary>
[DefaultExecutionOrder(-3000)]
public sealed class FMODManager : MonoBehaviour
{
    public static FMODManager instance;

    public const string PlayerShotEvent = "event:/SFX/SFX_ShotPlayer";
    public const string EnemyShotEvent = "event:/SFX/SFX_ShotEnemy";
    public const string PlayerDeathEvent = "event:/SFX/SFX_DeathPlayer";
    public const string EnemyDeathEvent = "event:/SFX/SFX_DeathEnemy";
    public const string PlayerEngineEvent = "event:/SFX/SFX_SpaceShipEngine";
    public const string BossWeaponEvent = "event:/SFX/SFX_Boss01_Machinegun";
    public const string ThemeMusicEvent = "event:/MUS/Mus_Theme";
    public const string SystemMusicEvent = "event:/MUS/Mus_System";
    public const string GameplayMusicEvent = "event:/MUS/Mus_Gameplay01";
    public const string BossMusicEvent = "event:/MUS/Mus_Boss01";
    public const string UiClickEvent = "event:/UI/UI_Click";
    public const string UiHoverEvent = "event:/UI/UI_Hover";
    public const string UiSelectEvent = "event:/UI/UI_Select";
    public const string UiStartEvent = "event:/UI/UI_StartGame";
    public const string UiUndoEvent = "event:/UI/UI_Undo";

    static readonly string[] PreloadedEventPaths =
    {
        PlayerShotEvent,
        EnemyShotEvent,
        BossWeaponEvent,
        PlayerEngineEvent,
        PlayerDeathEvent,
        EnemyDeathEvent,
        UiClickEvent,
        UiHoverEvent,
        UiSelectEvent,
        UiStartEvent,
        UiUndoEvent
    };

    const string PauseSnapshotEvent = "snapshot:/Pause";
    const string MasterBusPath = "bus:/";
    const string MusicVcaPath = "vca:/MUS";
    const string SfxVcaPath = "vca:/SFX";
    const string UiVcaPath = "vca:/UI";

    EventInstance musicInstance;
    EventInstance engineInstance;
    EventInstance bossWeaponInstance;
    EventInstance pauseSnapshotInstance;
    GameObject engineOwner;
    GameObject pendingEngineOwner;
    string currentMusicEvent;
    float nextPlayerShotSound;
    float nextEnemyShotSound;
    float nextBossWeaponSound;
    Coroutine bossWeaponStopRoutine;
    bool banksReady;
    bool shuttingDown;
    bool samplesPreloaded;
    readonly List<EventDescription> preloadedDescriptions = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (FindAnyObjectByType<FMODManager>(FindObjectsInactive.Include) != null) return;
        GameObject audioRoot = new GameObject("[Audio] FMOD Manager");
        audioRoot.AddComponent<FMODManager>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            // Cenas antigas possuem este componente. Mantemos o GameObject e
            // removemos apenas a cópia, para não afetar a hierarquia da cena.
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadBanks();
        ApplySavedVolumes();
        AudioVolumeSettings.Changed += ApplySavedVolumes;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        ConfigureSceneAudio(SceneManager.GetActiveScene());
    }

    void LoadBanks()
    {
        banksReady = false;
        try
        {
            // Além do preload do banco, os eventos frequentes são carregados
            // explicitamente. Isso evita a espera no primeiro tiro do player.
            bool banksLoaded = RuntimeManager.HaveAllBanksLoaded && HasRequiredBanks();
            if (banksLoaded)
                StartCoroutine(PrepareSamplesAsync());
            else
                StartCoroutine(WaitForBankLoading());
        }
        catch (Exception exception)
        {
            banksReady = false;
            Debug.LogError($"FMOD não conseguiu carregar os bancos Master: {exception.Message}", this);
        }
    }

    IEnumerator WaitForBankLoading()
    {
        while (!RuntimeManager.HaveAllBanksLoaded) yield return null;

        if (!HasRequiredBanks())
        {
            banksReady = false;
            Debug.LogError("FMOD terminou o carregamento sem disponibilizar os bancos Master.", this);
            yield break;
        }

        yield return PrepareSamplesAsync();
    }

    IEnumerator PrepareSamplesAsync()
    {
        PreloadFrequentEvents();

        // Nunca bloqueia a thread principal com flushSampleLoading. O carregamento
        // acontece durante o READY/GO e o áudio só é liberado quando termina.
        double timeoutAt = Time.realtimeSinceStartupAsDouble + 10d;
        bool loading;
        do
        {
            loading = false;
            foreach (EventDescription description in preloadedDescriptions)
            {
                if (!description.isValid()) continue;
                FMOD.RESULT result = description.getSampleLoadingState(out FMOD.Studio.LOADING_STATE state);
                if (result == FMOD.RESULT.OK && state == FMOD.Studio.LOADING_STATE.LOADING)
                {
                    loading = true;
                    break;
                }
            }

            if (loading) yield return null;
        }
        while (loading && Time.realtimeSinceStartupAsDouble < timeoutAt);

        if (loading)
            Debug.LogWarning("FMOD excedeu 10 segundos preparando os SFX; o jogo continuará sem bloquear a tela.", this);

        banksReady = HasRequiredBanks();
        ApplySavedVolumes();
        ConfigureSceneAudio(SceneManager.GetActiveScene());

        if (pendingEngineOwner != null)
        {
            GameObject owner = pendingEngineOwner;
            pendingEngineOwner = null;
            StartEngineInternal(owner);
        }
    }

    void PreloadFrequentEvents()
    {
        if (samplesPreloaded) return;
        preloadedDescriptions.Clear();

        foreach (string eventPath in PreloadedEventPaths)
        {
            try
            {
                EventDescription description = RuntimeManager.GetEventDescription(eventPath);
                if (!description.isValid()) continue;

                FMOD.RESULT result = description.loadSampleData();
                if (result == FMOD.RESULT.OK)
                    preloadedDescriptions.Add(description);
                else
                    Debug.LogWarning($"FMOD não conseguiu pré-carregar '{eventPath}': {result}.", this);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"FMOD não conseguiu preparar '{eventPath}': {exception.Message}", this);
            }
        }

        samplesPreloaded = true;
    }

    void ReleasePreloadedSamples()
    {
        foreach (EventDescription description in preloadedDescriptions)
            if (description.isValid()) description.unloadSampleData();

        preloadedDescriptions.Clear();
        samplesPreloaded = false;
    }

    static bool HasRequiredBanks()
    {
        return RuntimeManager.HasBankLoaded("Master.strings") &&
               RuntimeManager.HasBankLoaded("Master");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopPauseSnapshot();
        StopEngineInternal();
        StopBossWeaponInternal();
        ConfigureSceneAudio(scene);
        StartCoroutine(PrepareSceneAudioNextFrame());
    }

    IEnumerator PrepareSceneAudioNextFrame()
    {
        yield return null;
        EnsureStudioListener();
        InstallButtonAudio();
    }

    void ConfigureSceneAudio(Scene scene)
    {
        EnsureStudioListener();
        InstallButtonAudio();

        string sceneName = scene.name;
        if (sceneName.Equals("ingame", StringComparison.OrdinalIgnoreCase))
            PlayMusic(GameplayMusicEvent);
        else if (sceneName.Equals("MainMenu", StringComparison.OrdinalIgnoreCase))
            PlayMusic(ThemeMusicEvent);
        else
            PlayMusic(SystemMusicEvent);
    }

    void EnsureStudioListener()
    {
        StudioListener[] listeners = FindObjectsByType<StudioListener>(FindObjectsInactive.Exclude);
        foreach (StudioListener listener in listeners)
            if (listener.isActiveAndEnabled) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        StudioListener cameraListener = mainCamera.GetComponent<StudioListener>();
        if (cameraListener == null) cameraListener = mainCamera.gameObject.AddComponent<StudioListener>();
        cameraListener.enabled = true;
    }

    void InstallButtonAudio()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (Button button in buttons)
            if (button.GetComponent<FMODUIButtonAudio>() == null)
                button.gameObject.AddComponent<FMODUIButtonAudio>();
    }

    void ApplySavedVolumes()
    {
        if (!banksReady) return;
        SetBusVolume(MasterBusPath, AudioVolumeSettings.MasterVolume);
        SetVcaVolume(MusicVcaPath, 1f);
        SetVcaVolume(SfxVcaPath, AudioVolumeSettings.SfxVolume);
        SetVcaVolume(UiVcaPath, AudioVolumeSettings.SfxVolume);
    }

    static void SetBusVolume(string path, float value)
    {
        Bus bus = RuntimeManager.GetBus(path);
        if (bus.isValid()) bus.setVolume(Mathf.Clamp01(value));
    }

    static void SetVcaVolume(string path, float value)
    {
        VCA vca = RuntimeManager.GetVCA(path);
        if (vca.isValid()) vca.setVolume(Mathf.Clamp01(value));
    }

    void PlayMusic(string eventPath)
    {
        if (!banksReady || string.IsNullOrWhiteSpace(eventPath) || currentMusicEvent == eventPath) return;
        StopMusicInternal(StudioStopMode.ALLOWFADEOUT);
        try
        {
            musicInstance = RuntimeManager.CreateInstance(eventPath);
            if (musicInstance.isValid())
            {
                musicInstance.start();
                currentMusicEvent = eventPath;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"FMOD não encontrou a música '{eventPath}': {exception.Message}", this);
        }
    }

    void StopMusicInternal(StudioStopMode stopMode)
    {
        currentMusicEvent = null;
        if (!musicInstance.isValid()) return;
        musicInstance.stop(stopMode);
        musicInstance.release();
        musicInstance.clearHandle();
    }

    void StartEngineInternal(GameObject owner)
    {
        if (owner == null || engineOwner == owner) return;
        if (!banksReady)
        {
            pendingEngineOwner = owner;
            return;
        }

        StopEngineInternal();
        try
        {
            engineInstance = RuntimeManager.CreateInstance(PlayerEngineEvent);
            if (!engineInstance.isValid()) return;
            RuntimeManager.AttachInstanceToGameObject(engineInstance, owner, owner.GetComponent<Rigidbody2D>());
            engineInstance.start();
            engineOwner = owner;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"FMOD não iniciou o motor da nave: {exception.Message}", owner);
        }
    }

    void StopEngineInternal(GameObject owner = null)
    {
        if (owner == null || pendingEngineOwner == owner) pendingEngineOwner = null;
        if (owner != null && engineOwner != owner) return;
        engineOwner = null;
        if (!engineInstance.isValid()) return;
        RuntimeManager.DetachInstanceFromGameObject(engineInstance);
        engineInstance.stop(StudioStopMode.ALLOWFADEOUT);
        engineInstance.release();
        engineInstance.clearHandle();
    }

    void StartPauseSnapshot()
    {
        if (!banksReady || pauseSnapshotInstance.isValid()) return;
        pauseSnapshotInstance = RuntimeManager.CreateInstance(PauseSnapshotEvent);
        if (pauseSnapshotInstance.isValid()) pauseSnapshotInstance.start();
    }

    void StopPauseSnapshot()
    {
        if (!pauseSnapshotInstance.isValid()) return;
        pauseSnapshotInstance.stop(StudioStopMode.ALLOWFADEOUT);
        pauseSnapshotInstance.release();
        pauseSnapshotInstance.clearHandle();
    }

    void PlayOneShotInternal(string eventPath, Vector3 position)
    {
        if (!banksReady || shuttingDown) return;
        try
        {
            RuntimeManager.PlayOneShot(eventPath, position);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"FMOD não reproduziu '{eventPath}': {exception.Message}", this);
        }
    }

    void PlayBossWeaponInternal(Vector3 position)
    {
        if (!banksReady || shuttingDown) return;

        StopBossWeaponInternal();
        try
        {
            bossWeaponInstance = RuntimeManager.CreateInstance(BossWeaponEvent);
            if (!bossWeaponInstance.isValid()) return;

            bossWeaponInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            bossWeaponInstance.start();
            bossWeaponStopRoutine = StartCoroutine(StopBossWeaponAfterDelay(0.38f));
        }
        catch (Exception exception)
        {
            StopBossWeaponInternal();
            Debug.LogWarning($"FMOD não iniciou a arma do boss: {exception.Message}", this);
        }
    }

    IEnumerator StopBossWeaponAfterDelay(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        bossWeaponStopRoutine = null;
        ReleaseBossWeaponInstance();
    }

    void StopBossWeaponInternal()
    {
        if (bossWeaponStopRoutine != null)
        {
            StopCoroutine(bossWeaponStopRoutine);
            bossWeaponStopRoutine = null;
        }

        ReleaseBossWeaponInstance();
    }

    void ReleaseBossWeaponInstance()
    {
        if (!bossWeaponInstance.isValid()) return;
        bossWeaponInstance.stop(StudioStopMode.IMMEDIATE);
        bossWeaponInstance.release();
        bossWeaponInstance.clearHandle();
    }

    public static void PlayPlayerShot(Vector3 position)
    {
        if (instance == null || Time.unscaledTime < instance.nextPlayerShotSound) return;
        instance.nextPlayerShotSound = Time.unscaledTime + 0.035f;
        instance.PlayOneShotInternal(PlayerShotEvent, position);
    }

    public static void PlayEnemyShot(Vector3 position)
    {
        if (instance == null || Time.unscaledTime < instance.nextEnemyShotSound) return;
        instance.nextEnemyShotSound = Time.unscaledTime + 0.045f;
        instance.PlayOneShotInternal(EnemyShotEvent, position);
    }

    public static void PlayBossWeapon(Vector3 position)
    {
        if (instance == null || Time.unscaledTime < instance.nextBossWeaponSound) return;
        instance.nextBossWeaponSound = Time.unscaledTime + 0.08f;
        instance.PlayBossWeaponInternal(position);
    }

    public static void PlayEnemyDeath(Vector3 position) => instance?.PlayOneShotInternal(EnemyDeathEvent, position);
    public static void PlayPlayerDeath(Vector3 position) => instance?.PlayOneShotInternal(PlayerDeathEvent, position);
    public static void PlayUiClick() => instance?.PlayOneShotInternal(UiClickEvent, Vector3.zero);
    public static void PlayUiHover() => instance?.PlayOneShotInternal(UiHoverEvent, Vector3.zero);
    public static void PlayUiSelect() => instance?.PlayOneShotInternal(UiSelectEvent, Vector3.zero);
    public static void PlayUiStart() => instance?.PlayOneShotInternal(UiStartEvent, Vector3.zero);
    public static void PlayUiUndo() => instance?.PlayOneShotInternal(UiUndoEvent, Vector3.zero);
    public static void StartPlayerEngine(GameObject owner) => instance?.StartEngineInternal(owner);
    public static void StopPlayerEngine(GameObject owner) => instance?.StopEngineInternal(owner);
    public static void StopBossWeapon() => instance?.StopBossWeaponInternal();
    public static void PlayBossMusic() => instance?.PlayMusic(BossMusicEvent);

    public static void EnterGameOverAudio()
    {
        if (instance == null) return;
        instance.StopEngineInternal();
        instance.StopBossWeaponInternal();
        instance.StartPauseSnapshot();
        instance.PlayMusic(SystemMusicEvent);
    }

    public static void EnterStageCompleteAudio()
    {
        if (instance == null) return;
        instance.StopEngineInternal();
        instance.StopBossWeaponInternal();
        instance.StartPauseSnapshot();
        instance.PlayMusic(SystemMusicEvent);
    }

    public static void ExitPausedState() => instance?.StopPauseSnapshot();

    // API antiga mantida para UnityEvents já serializados.
    public void SetVolume(string type, float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (string.Equals(type, "music", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(type, "master", StringComparison.OrdinalIgnoreCase))
            AudioVolumeSettings.SetMasterVolume(volume);
        else if (string.Equals(type, "sfx", StringComparison.OrdinalIgnoreCase))
            AudioVolumeSettings.SetSfxVolume(volume);
        else if (string.Equals(type, "dub", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(type, "voice", StringComparison.OrdinalIgnoreCase))
            AudioVolumeSettings.SetVoiceVolume(volume);
    }

    public void PlayMenuClickSound() => PlayUiClick();
    public void StopMusic() => StopMusicInternal(StudioStopMode.ALLOWFADEOUT);

    void OnDestroy()
    {
        if (instance != this) return;
        shuttingDown = true;
        AudioVolumeSettings.Changed -= ApplySavedVolumes;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopPauseSnapshot();
        StopEngineInternal();
        StopBossWeaponInternal();
        StopMusicInternal(StudioStopMode.IMMEDIATE);
        ReleasePreloadedSamples();
        instance = null;
    }
}

/// <summary>Feedback sonoro instalado automaticamente em cada Button da cena.</summary>
[DisallowMultipleComponent]
public sealed class FMODUIButtonAudio : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerClickHandler
{
    float lastFeedbackTime = -1f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanPlay()) return;
        lastFeedbackTime = Time.unscaledTime;
        FMODManager.PlayUiHover();
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Clique por ponteiro já possui hover + click. Select fica reservado
        // para navegação por teclado/controle e evita dois sons simultâneos.
        if (eventData is PointerEventData) return;
        if (!CanPlay()) return;
        lastFeedbackTime = Time.unscaledTime;
        FMODManager.PlayUiSelect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Button button = GetComponent<Button>();
        if (button == null || !button.IsInteractable()) return;

        string key = gameObject.name.ToLowerInvariant();
        if (ContainsAny(key, "start", "play", "continuar", "continue", "next", "jogar"))
            FMODManager.PlayUiStart();
        else if (ContainsAny(key, "back", "close", "exit", "undo", "voltar", "fechar", "sair"))
            FMODManager.PlayUiUndo();
        else
            FMODManager.PlayUiClick();
    }

    bool CanPlay()
    {
        Button button = GetComponent<Button>();
        return button != null && button.IsInteractable() && Time.unscaledTime - lastFeedbackTime > 0.04f;
    }

    static bool ContainsAny(string value, params string[] terms)
    {
        foreach (string term in terms)
            if (value.Contains(term)) return true;
        return false;
    }
}
