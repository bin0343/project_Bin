using System.Collections.Generic;
using UnityEngine;

public class IntersectionController : MonoBehaviour
{
    private CarPathFollower currentVehicle;

    private readonly Queue<CarPathFollower> waitingVehicles = new Queue<CarPathFollower>();

    private readonly HashSet<CarPathFollower> queuedVehicles = new HashSet<CarPathFollower>();

    public CarPathFollower CurrentVehicle => currentVehicle;
    public bool IsOccupied => currentVehicle != null;


    // 교차로 진입 요청
    public bool RequestEntry(CarPathFollower vehicle)
    {
        if (vehicle == null) return false;

        CleanInvalidVehicles();

        // 이미 이 차량이 사용권을 가지고 있음
        if (currentVehicle == vehicle)
        {
            return true;
        }

        // 아무도 사용하고 있지 않음
        if (currentVehicle == null)
        {
            // 대기 차량도 없다면 바로 승인
            if (waitingVehicles.Count == 0)
            {
                currentVehicle = vehicle;
                return true;
            }

            // 대기열 1등 차량이라면 승인
            if (waitingVehicles.Peek() == vehicle)
            {
                waitingVehicles.Dequeue();
                queuedVehicles.Remove(vehicle);

                currentVehicle = vehicle;
                return true;
            }
        }

        // 현재 교차로가 사용 중이면 대기열 등록
        if (!queuedVehicles.Contains(vehicle))
        {
            waitingVehicles.Enqueue(vehicle);
            queuedVehicles.Add(vehicle);
        }

        return false;
    }


    // 교차로를 다 빠져나온 차량이 호출
    public void Release(CarPathFollower vehicle)
    {
        if (vehicle == null) return;

        if (currentVehicle != vehicle) return;

        currentVehicle = null;

        GiveReservationToNextVehicle();
    }


    private void GiveReservationToNextVehicle()
    {
        while (waitingVehicles.Count > 0)
        {
            CarPathFollower nextVehicle = waitingVehicles.Dequeue();

            queuedVehicles.Remove(nextVehicle);

            if (!IsValidVehicle(nextVehicle)) continue;

            currentVehicle = nextVehicle;
            return;
        }
    }


    private void CleanInvalidVehicles()
    {
        // 현재 사용 차량이 Destroy/Disable된 경우
        if (currentVehicle != null && !IsValidVehicle(currentVehicle))
        {
            currentVehicle = null;
        }

        int count = waitingVehicles.Count;

        for (int i = 0; i < count; i++)
        {
            CarPathFollower vehicle = waitingVehicles.Dequeue();

            if (IsValidVehicle(vehicle))
            {
                waitingVehicles.Enqueue(vehicle);
            }
            else
            {
                queuedVehicles.Remove(vehicle);
            }
        }

        if (currentVehicle == null)
        {
            GiveReservationToNextVehicle();
        }
    }


    private bool IsValidVehicle(CarPathFollower vehicle)
    {
        return vehicle != null
            && vehicle.gameObject.activeInHierarchy;
    }
}