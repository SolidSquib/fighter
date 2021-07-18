using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : SingletonScriptBase<CameraManager>
{
    List<CameraTrigger> cameraStack { get; set; } = new List<CameraTrigger>();
    CinemachineStateDrivenCamera cameraStateMachine { get; set; }

    protected override void Awake()
    {
        base.Awake();
        cameraStateMachine = FindObjectOfType<CinemachineStateDrivenCamera>();
    }

    public void AddActiveCameraTrigger(CameraTrigger trigger)
    {
        cameraStack.Add(trigger);
        UpdateActiveVirtualCamera();
    }

    public void RemoveActiveCameraTrigger(CameraTrigger trigger)
    {
        cameraStack.Remove(trigger);
        UpdateActiveVirtualCamera();
    }

    void UpdateActiveVirtualCamera()
    {
        if (cameraStack.Count > 0)
        {
            cameraStateMachine.GetComponent<Animator>().SetTrigger(cameraStack.Last().triggerName);
        }
    }
}
