using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public AudioClip bgm;
        [Range(0f, 1f)] public float volume = 0.5f;
    }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Scene BGM")]
    [SerializeField] private SceneBGM[] sceneBGMs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlaySceneBGM(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneBGM(scene.name);
    }

    private void PlaySceneBGM(string sceneName)
    {
        foreach (SceneBGM data in sceneBGMs)
        {
            if (data.sceneName != sceneName) continue;

            if (audioSource.clip == data.bgm && audioSource.isPlaying) return;

            audioSource.Stop();

            audioSource.clip = data.bgm;
            audioSource.volume = data.volume;

            audioSource.Play();

            return;
        }
    }
}