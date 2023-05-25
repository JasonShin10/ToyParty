using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jam : MonoBehaviour
{

    public bool touched = false;

    public GameObject hitObject;
    private float currentTime = 0;
    private float maxTime = 1f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    //public IEnumerator moveGem(Vector3 targetPos)
    //{
    //    while (currentTime < maxTime)
    //    {
    //        if (gameObject == !inhe)
    //        {
    //            yield break; // Exit the coroutine if the object has been destroyed
    //        }

    //        Vector3 myPos = gameObject.transform.position;
    //        gameObject.transform.position = Vector3.Lerp(myPos, targetPos, currentTime / maxTime);
    //        currentTime += Time.deltaTime;
    //        yield return null;
    //    }
    //    if (gameObject != null)
    //    {
    //        gameObject.transform.position = targetPos;
    //        currentTime = 0;
    //    }
    //}
}
