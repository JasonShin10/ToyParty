using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public bool touched = false;

    public GameObject hitObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    // 젬을 targerPos로 일정 속도로 이동시키는 코루틴
    public IEnumerator MoveGem(Vector3 targetPos)
    {
        float speed = 0.01f;

        while (this != null && transform.position != targetPos)
        {
            // If the game object has been destroyed, stop the coroutine.
            if (gameObject == null)
            {
                yield break;
            }

            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed);
            yield return new WaitForEndOfFrame();
        }
    }

   
}
