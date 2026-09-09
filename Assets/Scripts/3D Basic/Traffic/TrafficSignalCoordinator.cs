using System.Collections;
using UnityEngine;

[System.Serializable]
public class TrafficSignalPhase
{
    public string phaseName;

    [Tooltip("이 Phase에서 초록불이 되는 신호 그룹")]
    public TrafficLightController[] greenGroups;
}

public class TrafficSignalCoordinator : MonoBehaviour
{
    [Header("이 교차로의 모든 신호 그룹")]
    [SerializeField]
    private TrafficLightController[] allGroups;

    [Header("신호 Phase")]
    [SerializeField]
    private TrafficSignalPhase[] phases;


    [Header("시간")]
    [SerializeField]
    private float greenDuration = 15f;

    [SerializeField]
    private float yellowDuration = 3f;

    [SerializeField]
    private float allRedDuration = 1f;


    private void Start()
    {
        SetAllLights(TrafficLightController.LightState.Red);

        StartCoroutine(SignalCycleRoutine());
    }


    private IEnumerator SignalCycleRoutine()
    {
        if (phases == null || phases.Length == 0) yield break;

        while (true)
        {
            for (int i = 0; i < phases.Length; i++)
            {
                TrafficSignalPhase phase = phases[i];

                if (phase == null) continue;

                SetAllLights(TrafficLightController.LightState.Red);

                SetPhaseLights(phase, TrafficLightController.LightState.Green);

                yield return new WaitForSeconds(greenDuration);

                SetPhaseLights(phase, TrafficLightController.LightState.Yellow);

                yield return new WaitForSeconds(yellowDuration);

                SetAllLights(TrafficLightController.LightState.Red);

                yield return new WaitForSeconds(allRedDuration);
            }
        }
    }


    private void SetAllLights(TrafficLightController.LightState state)
    {
        if (allGroups == null) return;

        foreach (TrafficLightController light in allGroups)
        {
            if (light != null)
            {
                light.SetState(state);
            }
        }
    }


    private void SetPhaseLights(TrafficSignalPhase phase, TrafficLightController.LightState state)
    {
        if (phase.greenGroups == null) return;

        foreach (TrafficLightController light in phase.greenGroups)
        {
            if (light != null)
            {
                light.SetState(state);
            }
        }
    }
}