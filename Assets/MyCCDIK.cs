using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCCDIK : MonoBehaviour
{
    public int iterationCount = 5;
    public Transform[] links;
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int j = 0; j < iterationCount; j++)
        {
            for (int i = links.Length - 2; i > -1; i--)
            {
                float w = 1 / (links.Length - 2);

                Vector3 currentDir = links[links.Length - 1].position - links[i].position;
                Vector3 idealDir = target.position - links[i].position;

                Quaternion fromToRotation = Quaternion.FromToRotation(currentDir, idealDir);

                links[i].rotation = Quaternion.Lerp( fromToRotation * links[i].rotation,links[i].rotation,w);
            }
        }
    }
}
