using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    public List<GameObject> switchGemes = new List<GameObject>();
    private RaycastHit hit;
    private Ray ray;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }

    void CheckInput()
    {
        // 보석을 눌렀을때
        if (Input.GetMouseButton(0))
        {
            CheckRaycastHit();
        }       
        BoardManager.instance.HandleGemSwap(switchGemes);

        // 손을 뗏을때 보석을 다시 되돌린다.
        if (Input.GetMouseButtonUp(0) && switchGemes.Count != 2)
        {
            BoardManager.instance.ResetJamSelection();
        }
    }

    private void CheckRaycastHit()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << LayerMask.NameToLayer("Jam");
        if (Physics.Raycast(ray, out hit, layerMask))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Jam") && switchGemes.Count < 2)
            {
                hit.transform.GetComponent<Gem>().touched = true;
                if (!switchGemes.Contains(hit.transform.gameObject))
                {
                    switchGemes.Add(hit.transform.gameObject);
                }
            }
        }
    }

}
