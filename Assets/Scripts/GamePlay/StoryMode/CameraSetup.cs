using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    
    public void FirstStage()
    {
        virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_XDamping = 1;
        virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>().m_UnlimitedSoftZone = false;
        StartCoroutine(ScaleCamera(26, 3));
    }

    public void SecondStage()
    {
        StartCoroutine(ScaleCamera(35, 3));
    }

    public void ThirdStage()
    {
        StartCoroutine(ScaleCamera(15, 8));
    }

    private IEnumerator ScaleCamera(float newSize, float duration)
    {
        float oldSize = virtualCamera.m_Lens.OrthographicSize;
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            virtualCamera.m_Lens.OrthographicSize = Mathf.SmoothStep(oldSize, newSize, (Time.time - startTime) / duration);
            yield return null;
        }
        virtualCamera.m_Lens.OrthographicSize = newSize;
    }
}
