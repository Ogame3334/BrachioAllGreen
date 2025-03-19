using UnityEngine;

public class PassthroughLayerEffectManager : MonoBehaviour
{
  [SerializeField]
  public bool isEffectActive = false;
  [SerializeField]
  public float effectWeight = 0f;
  [SerializeField]
  public OVRPassthroughLayer passthroughLayer;
  [SerializeField]
  public Texture2D lutTexture;
  private readonly float effectWeightPerSecond = 1f;

  public void Update()
  {
    if (isEffectActive && effectWeight < 1f)
    {
      effectWeight = Mathf.Min(1f, effectWeight + effectWeightPerSecond * Time.deltaTime);
    }
    else if (!isEffectActive && 0f < effectWeight)
    {
      effectWeight = Mathf.Max(0f, effectWeight - effectWeightPerSecond * Time.deltaTime);
    }
    else
    {
      return;
    }

    passthroughLayer.SetColorLut(new OVRPassthroughColorLut(lutTexture, true), effectWeight);
  }
}
