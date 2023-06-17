using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public delegate void MoveAction(Vector3 targetPos);

    public MoveAction OnMove;
    public event Action onMoveComplete;

    public bool touched = false;
    public GameObject hitObject;
    public float speed = 0.01f;
    public bool moving = false;
    public Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        OnMove = DoMove;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            OnMove(targetPos);
        }
    }

    private void OnEnable()
    {
        OnMove += DoMove;
    }

    private void OnDisable()
    {
        OnMove -= DoMove;
    }

    public void TriggerMove()
    {
        if (moving)
        {
            OnMove?.Invoke(targetPos);
        }
    }

    public void DoMove(Vector3 targetPos)
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed);
        if(transform.position == targetPos)
        {
            moving = false;
            onMoveComplete?.Invoke();
        }
    }

}
