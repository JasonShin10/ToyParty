using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private bool swapping = false;
    private List<GameObject> switchJames = new List<GameObject>();
    private RaycastHit hit;
    private Ray ray;

    void Start()
    {

    }

    private float currentTime = 0;
    private float maxTime = 1f;
    private Vector3 jamOne;
    private Vector3 jamTwo;

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }

    void CheckInput()
    {
        // ������ ��������
        if (Input.GetMouseButton(0))
        {
            CheckRaycastHit();
        }

        // ������ �ΰ��� �Ǿ����� �ٲ۴�.
        if (switchJames.Count == 2 && !swapping)
        {
            StartCoroutine(JamPosChange());
            swapping = true;
        }

        // ���� ������ ������ �ٽ� �ǵ�����.
        if (Input.GetMouseButtonUp(0) && switchJames.Count != 2)
        {
            ResetJamSelection();
        }
    }

    private void CheckRaycastHit()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << LayerMask.NameToLayer("Jam");
        if (Physics.Raycast(ray, out hit, layerMask))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Jam") && switchJames.Count < 2)
            {
                hit.transform.GetComponent<Jam>().touched = true;
                if (!switchJames.Contains(hit.transform.gameObject))
                {
                    switchJames.Add(hit.transform.gameObject);
                }
            }
        }
    }

    private void ResetJamSelection()
    {
        for (int i = 0; i < switchJames.Count; i++)
        {
            switchJames[i].GetComponent<Jam>().touched = false;
        }
        switchJames.Clear();
        swapping = false;
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
