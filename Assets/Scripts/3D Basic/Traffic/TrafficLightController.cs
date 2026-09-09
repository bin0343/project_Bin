using UnityEngine;
using UnityEngine.AI;

public class TrafficLightController : MonoBehaviour
{
    public enum LightState
    {
        Green,
        Yellow,
        Red
    }

    [Header("현재 차량 신호")]
    public LightState currentState = LightState.Red;

    [Header("보행자 통제용")]
    [SerializeField] private GameObject pedestrianBarrierParent;

    private NavMeshObstacle[] pedestrianBarriers;


    private void Awake()
    {
        if (pedestrianBarrierParent != null)
        {
            pedestrianBarriers = pedestrianBarrierParent.GetComponentsInChildren<NavMeshObstacle>(true);
        }
        else
        {
            pedestrianBarriers = new NavMeshObstacle[0];
        }
    }


    public void SetState(LightState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case LightState.Green:
                SetPedestrianBarriers(true);
                break;

            case LightState.Yellow:
                SetPedestrianBarriers(true);
                break;

            case LightState.Red:
                SetPedestrianBarriers(false);
                break;
        }
    }


    private void SetPedestrianBarriers(bool isActive)
    {
        if (pedestrianBarriers == null) return;

        foreach (NavMeshObstacle barrier in pedestrianBarriers)
        {
            if (barrier != null)
            {
                barrier.enabled = isActive;
            }
        }
    }
}