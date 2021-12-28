using UnityEngine;

public class SampleBobbingBehaviour : MonoBehaviour
{
    public float amplitude = 1, frequency = 1;

    public Vector3 axis = new Vector3(0, 1, 0);

    private Vector3 _origin; 

    // Start is called before the first frame update
    void Start()
    {
        _origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = _origin + axis * (amplitude * Mathf.Sin(frequency * Time.time));
    }
}
