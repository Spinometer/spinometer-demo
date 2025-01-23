using UnityEngine;

public class BackgroundPlaneUpdateSize : MonoBehaviour
{
  private Vector2Int _screenSize = Vector2Int.zero;


  void Update()
  {
    var currentSize = new Vector2Int(Screen.width, Screen.height);
    if (_screenSize == currentSize)
      return;

    _screenSize = currentSize;

    var s = transform.localScale;
    transform.localScale = new Vector3(s.y * currentSize.x / currentSize.y,
                                       s.y, s.z);
  }
}
