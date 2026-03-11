// Scripts/CameraTestSwitcher.cs
using UnityEngine;
using Cinemachine;

public class CameraTestSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera camMain;
    public CinemachineVirtualCamera camLeft;
    public CinemachineVirtualCamera camCenter;
    public CinemachineVirtualCamera camRight;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ActivateCamera(camMain);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            ActivateCamera(camLeft);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            ActivateCamera(camCenter);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            ActivateCamera(camRight);
    }

    void ActivateCamera(CinemachineVirtualCamera cam)
    {
        camMain.Priority = 0;
        camLeft.Priority = 0;
        camCenter.Priority = 0;
        camRight.Priority = 0;

        cam.Priority = 10;
    }
}
