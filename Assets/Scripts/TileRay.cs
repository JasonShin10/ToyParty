using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileRay : MonoBehaviour
{
    public bool blue = false;
    public bool green = false;
    public bool red = false;
    public bool yellow = false;
    public bool orange = false;
    public bool purple = false;
    public GameObject color;
    public RaycastHit hit;
    public Ray ray;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ray = new Ray(transform.position, transform.up);
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.blue);

        
        if (Physics.Raycast(ray, out hit))
        {            
            color = hit.transform.gameObject;
            //print(color);
        }
    }


}
