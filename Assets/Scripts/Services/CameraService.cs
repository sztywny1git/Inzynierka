using UnityEngine;
using Unity.Cinemachine;

public class CameraService : ICameraService
{
    private readonly CinemachineCamera _camera;

    public CameraService(CinemachineCamera camera)
    {
        _camera = camera;
    }

    public void SetFollowTarget(Transform target)
    {
        if (_camera != null)
        {
            _camera.Follow = target;
            _camera.LookAt = target;
        }
    }
}