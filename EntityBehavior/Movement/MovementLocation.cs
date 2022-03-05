using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementLocation : MonoBehaviour
{
    // Start is called before the first frame update
    public float spaceRange;

    void Start()
    {
        transform.localScale = new Vector3(spaceRange, spaceRange, spaceRange);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MovementLocation>())
        {
            Debugger.InConsole(1562, $"Movement locations collided");
            var direction = -V3Helper.Direction(other.transform.position, transform.position);
            var displaceMent = direction * spaceRange;
            transform.position += new Vector3(displaceMent.x, 0, displaceMent.z);
        }
    }
}
