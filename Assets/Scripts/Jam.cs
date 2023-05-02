using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jam : MonoBehaviour
{

    public bool touched = false;
    Ray ray;
    RaycastHit hit;
    public GameObject hitObject;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

       
    }

    public void FindMyHexagon()
    {
        ray = new Ray(transform.position, transform.up);
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.blue);
        if (Physics.Raycast(ray, out hit))
        {
            hitObject = hit.transform.gameObject;
            BoardManager.instance.CheckForMatches(hitObject);
        }
    }

    
}
