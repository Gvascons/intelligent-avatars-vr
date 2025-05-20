using UnityEngine;

public class LipSyncFromAudioClip : MonoBehaviour
{
    public SkinnedMeshRenderer faceMesh;
    public int mouthOpenBlendShapeIndex = 0;
    public AudioSource audioSource;

    [Range(0f, 1f)]
    public float blendShapeMultiplier = 0.3f;  //  Multiplier to control the mouth intensity        

    void Update()
    {
        if (faceMesh != null && audioSource != null && audioSource.isPlaying)
        {
            float[] spectrum = new float[256];
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

            float intensity = Mathf.Clamp01(spectrum[0] * 1000f);
            float adjustedIntensity = intensity * 100f * blendShapeMultiplier;

            faceMesh.SetBlendShapeWeight(mouthOpenBlendShapeIndex, adjustedIntensity);
        }
        else if (faceMesh != null)
        {
            faceMesh.SetBlendShapeWeight(mouthOpenBlendShapeIndex, 0f);
        }
    }
}
