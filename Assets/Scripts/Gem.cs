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
    public SpriteRenderer spriteRenderer;
    public float rayLength = 100f;
    
    bool hitHexagon = false;
    public void Start()
    {
      
    }
    void Update()
    {
            if (isMoving)
            {
                transform.position = Vector2.Lerp(transform.position, targetPos, 0.1f);
                isMoving = Vector2.Distance(transform.position, targetPos) > 0.01f;
                if (!isMoving)
                {

                    OnMoveComplete?.Invoke();
                    OnMoveComplete = null;
                }
            }
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;

        RaycastHit hitInfo;
        if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, rayLength))
        {
            hitObject = hitInfo.collider.gameObject;
            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Hexagon"))
            {
                hitHexagon = true;
            }
        }
            spriteRenderer.enabled = hitHexagon;
    }

    

        public void MoveAnimationPresent(Vector3 targetPos, System.Action onAnimationComplete = null)
    {
        isMoving = true;
        this.targetPos = targetPos;
        
        OnMoveComplete?.Invoke();
        OnMoveComplete = onAnimationComplete;
    }

}
