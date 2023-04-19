using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public bool swapping = false;
    public bool isCorotineRunning;
    public List<GameObject> switchJames = new List<GameObject>();
    public RaycastHit hit;
    public Ray ray;
    // Start is called before the first frame update
    void Start()
    {

    }
    float currentTime = 0;
    float maxTime = 1f;
    Vector3 jamOne;
    Vector3 jamTwo;
    public List<Vector3> jamPos = new List<Vector3>();
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {           
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layerMask = 1 << LayerMask.NameToLayer("Jam");
            if (Physics.Raycast(ray, out hit, layerMask))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Jam") && switchJames.Count <2)
                {
                    hit.transform.GetComponent<Jam>().touched = true;
                    if (!switchJames.Contains(hit.transform.gameObject))
                    {
                        switchJames.Add(hit.transform.gameObject);              
                    }
                }
            }
        }
        // swapping이 true가 아니고 리스트 개수가 2개일때
        if (switchJames.Count == 2 && !swapping)
        {
            StartCoroutine(JamPosChange());
            swapping = true;
        }
        if (Input.GetMouseButtonUp(0) && switchJames.Count != 2)
        {
            for (int i = 0; i < switchJames.Count; i++)
            {
                switchJames[i].GetComponent<Jam>().touched = false;           
            }
            switchJames.Clear();
            swapping= false;
        }
    }

    private IEnumerator JamPosChange()
    {
        jamOne = switchJames[0].transform.position;
        jamTwo = switchJames[1].transform.position;
        while (currentTime < maxTime)
        {
            switchJames[0].transform.position = Vector3.Lerp(jamOne, jamTwo, currentTime / maxTime);
            switchJames[1].transform.position = Vector3.Lerp(jamTwo, jamOne, currentTime / maxTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        switchJames[0].transform.position = jamTwo;
        switchJames[1].transform.position = jamOne;
        for (int i = 0; i < switchJames.Count; i++)
        {
            switchJames[i].GetComponent<Jam>().touched = false;
        }
        switchJames[0].GetComponent<Jam>().FindMyHexagon();
        switchJames.Clear();
        currentTime = 0;
        swapping = false;
    }
}
