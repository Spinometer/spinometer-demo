using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace GetBack.Spinometer
{
  public class Opening : MonoBehaviour
  {
    [SerializeField] SpriteRenderer _openingLogoLogoRenderer;
    [SerializeField] SpriteRenderer _openingLogoLightRenderer;
    [SerializeField] LocalizedSprite _openingLogoLogoSpriteAsset;
    [SerializeField] LocalizedSprite _openingLogoLightSpriteAsset;

    void OnEnable()
    {
      _openingLogoLogoRenderer.sprite = _openingLogoLogoSpriteAsset.LoadAsset();
      _openingLogoLightRenderer.sprite = _openingLogoLightSpriteAsset.LoadAsset();
    }
  }
}
