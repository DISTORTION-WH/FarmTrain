// WagonController.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WagonController : MonoBehaviour
{
    private Animator wagonAnimator;

    private void Awake()
    {
        wagonAnimator = GetComponent<Animator>();

        LocomotiveController.OnLocomotiveReady += Initialize;
    }

    private void OnDestroy()
    {
        // ������������ �� ����� ������� ��� ����������� ������
        if (LocomotiveController.Instance != null)
        {
            LocomotiveController.Instance.OnTrainStateChanged -= HandleTrainStateChange;
        }
        LocomotiveController.OnLocomotiveReady -= Initialize;
    }

    private void Initialize()
    {
        // ������ �� �������, ��� LocomotiveController.Instance �� null.
        if (LocomotiveController.Instance != null)
        {
            // 1. ������������� �� ������� ��������� ���������
            LocomotiveController.Instance.OnTrainStateChanged += HandleTrainStateChange;

            // 2. ����� �� ������������� ���������� ��������� ���������
            bool isCurrentlyMoving = LocomotiveController.Instance.currentState == LocomotiveController.TrainState.Moving;
            HandleTrainStateChange(isCurrentlyMoving);
        }
    }

    private void HandleTrainStateChange(bool isMoving)
    {
        if (wagonAnimator != null)
        {
            wagonAnimator.SetBool("isMoving", isMoving);
            Debug.Log($"<color=yellow>[Wagon]</color> ����� '{gameObject.name}' ������� ������� � ��������� isMoving �: {isMoving}");
        }
    }
}