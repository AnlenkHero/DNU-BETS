using UnityEngine;

public class SpinAnimation : MonoBehaviour
{
    private readonly float _rotationSpeed = 30f;
    void Update()
    { 
        transform.Rotate(new Vector3(0,0, _rotationSpeed * Time.deltaTime));
    }
}
