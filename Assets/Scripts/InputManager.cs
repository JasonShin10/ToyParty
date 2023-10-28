using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public List<GameObject> switchGems = new List<GameObject>();
    private RaycastHit hit;
    private Ray ray;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            CheckGemToSwitch();
            BoardManager.instance.HandleGemSwap(switchGems);
        }

        if (Input.GetMouseButtonUp(0) && switchGems.Count != 2)
        {
            BoardManager.instance.ResetGemSelection();
        }
    }

    private void CheckGemToSwitch()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = Utility.GetGameLayers(new string[] { Constant.LAYER_NAME_GEM });

        if (Physics.Raycast(ray, out hit, layerMask))
        {
            AddGemToSwitchList();
        }
    }

    private void AddGemToSwitchList()
    {
        bool isGemLayer = hit.transform.gameObject.layer == LayerMask.NameToLayer(Constant.LAYER_NAME_GEM);
        bool underGemLimit = switchGems.Count < 2;

        if (isGemLayer && underGemLimit)
        {
            Gem hitGem = hit.transform.GetComponent<Gem>();
            hitGem.touched = true;

            if (!switchGems.Contains(hit.transform.gameObject))
            {
                switchGems.Add(hit.transform.gameObject);
            }
        }
    }
}


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class InputManager : MonoBehaviour
//{

//    public List<GameObject> switchGemes = new List<GameObject>();
//    private RaycastHit hit;
//    private Ray ray;

//    void Start()
//    {
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        CheckInput();
//    }

//    void CheckInput()
//    {

//        if (Input.GetMouseButton(0))
//        {
//            CheckRaycastHit();
//        }


//        if (Input.GetMouseButtonUp(0) && switchGemes.Count == 2)
//        {
//            BoardManager.instance.HandleGemSwap(switchGemes);
//            BoardManager.instance.ResetGemSelection();
//        }
//    }

//    private void CheckRaycastHit()
//    {
//        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//        int layerMask = Utility.GetGameLayers(new string[] { Constant.LAYER_NAME_GEM });
//        if (Physics.Raycast(ray, out hit, layerMask))
//        {
//            if (hit.transform.gameObject.layer == LayerMask.NameToLayer(Constant.LAYER_NAME_GEM) && switchGemes.Count < 2)
//            {
//                hit.transform.GetComponent<Gem>().touched = true;
//                if (!switchGemes.Contains(hit.transform.gameObject))
//                {
//                    switchGemes.Add(hit.transform.gameObject);
//                }
//            }
//        }
//    }

//}
