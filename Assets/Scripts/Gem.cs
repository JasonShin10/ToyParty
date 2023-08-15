using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public event Action OnMoveComplete;
    public bool touched = false;
    public GameObject hitObject;
    public float speed = 0.01f;
    public bool isMoving = false;
    public bool canMove = false;
    public Vector3 targetPos;


    public void Start()
    {
      
    }
    void Update()
    {
            if (isMoving)
            {
                transform.position = Vector2.Lerp(transform.position, targetPos, 0.1f);
                isMoving = Vector2.Distance(transform.position, targetPos) > 0.001f;
                if (!isMoving)
                {
                    print(this.gameObject.name + "is" + " " + "done");
                    OnMoveComplete?.Invoke();
                    OnMoveComplete = null;
                }
            }
    }

  

    public void MoveAnimationPresent(Vector3 targetPos, System.Action onAnimationComplete = null)
    {
        isMoving = true;
        this.targetPos = targetPos;
        OnMoveComplete += onAnimationComplete;
    }

}
