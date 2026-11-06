using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;
using System.Collections;

public class QuestCutsceneController : MonoBehaviour
{
    [Header("시네마틱 연출")]
    [SerializeField] private bool useCinematicPresentation = true;
    [SerializeField] private CinematicLetterbox letterbox;
    [SerializeField] private bool useAutoLetterbox = true;

    [Header("Timeline")]
    [SerializeField] private PlayableDirector director;

    [Header("컷신 종료 후 실행")]
    [SerializeField] private UnityEvent onCutsceneFinished;

    private bool isPlaying;

    public void PlayCutscene()
    {
        if (director == null)
        {
            Debug.LogWarning("[QuestCutscene] PlayableDirector가 없습니다.");
            return;
        }

        if (isPlaying) return;

        isPlaying = true;

        if (BattleManager.instance != null)
        {
            BattleManager.instance.SetPlayerControlLocked(true);
        }

        if (useCinematicPresentation)
        {
            if (UI_Manager.Instance != null)
            {
                UI_Manager.Instance.EnterCinematicMode();
            }
        }

        if (useCinematicPresentation && useAutoLetterbox)
        {
            if (letterbox == null)
            {
                letterbox = CinematicLetterbox.Instance;
            }

            if (letterbox != null)
            {
                letterbox.Show();
            }
        }

        BindRuntimeCinemachineBrain();

        director.time = 0;
        director.Evaluate();
        director.Play();
    }

    private void BindRuntimeCinemachineBrain()
    {
        if (director == null) return;

        TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;

        if (timelineAsset == null) return;

        CinemachineBrain brain = null;

        if (Camera.main != null)
        {
            brain = Camera.main.GetComponent<CinemachineBrain>();
        }

        if (brain == null)
        {
            brain = FindObjectOfType<CinemachineBrain>();
        }

        if (brain == null)
        {
            Debug.LogWarning("[QuestCutscene] CinemachineBrain을 찾지 못했습니다.");
            return;
        }

        foreach (TrackAsset track in timelineAsset.GetOutputTracks())
        {
            if (track is CinemachineTrack)
            {
                director.SetGenericBinding(track, brain);
            }
        }
    }

    private void HandleDirectorStopped(PlayableDirector stoppedDirector)
    {
        if (stoppedDirector != director) return;
        if (!isPlaying) return;

        StartCoroutine(FinishCutsceneRoutine());
    }

    private IEnumerator FinishCutsceneRoutine()
    {
        isPlaying = false;

        if (useCinematicPresentation && useAutoLetterbox)
        {
            if (letterbox != null)
            {
                letterbox.Hide();

                yield return new WaitForSecondsRealtime(
                    letterbox.SlideDuration);
            }
        }

        if (useCinematicPresentation)
        {
            if (UI_Manager.Instance != null)
            {
                UI_Manager.Instance.ExitCinematicMode();
            }
        }

        if (BattleManager.instance != null)
        {
            BattleManager.instance.SetPlayerControlLocked(false);
        }

        onCutsceneFinished?.Invoke();
    }

    private void OnEnable()
    {
        if (director != null)
        {
            director.stopped += HandleDirectorStopped;
        }
    }

    private void OnDisable()
    {
        if (director != null)
        {
            director.stopped -= HandleDirectorStopped;
        }
    }
}
