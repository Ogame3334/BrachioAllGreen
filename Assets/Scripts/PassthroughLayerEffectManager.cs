using System.IO;
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
  [SerializeField]
  public GameObject effectOverlay;
  private readonly float effectWeightPerSecond = .4f;

  private float EE(float x)
  {
    return x*x*(3f-2f*x);
  }

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

    if(effectWeight < 0.2f)
    {
      effectOverlay.GetComponent<MeshRenderer>().material.SetFloat("_greenWeightOffset", 1f-effectWeight/0.2f);
    }
    else if(0.8f < effectWeight)
    {
      effectOverlay.GetComponent<MeshRenderer>().material.SetFloat("_greenWeightOffset", (effectWeight-0.8f)/0.2f);
    }
    passthroughLayer.SetColorLut(
      new OVRPassthroughColorLut(lutTexture, true),
      EE(EE(EE(effectWeight)))
    );
  }
}
