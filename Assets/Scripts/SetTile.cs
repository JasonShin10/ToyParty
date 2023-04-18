using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTile : MonoBehaviour
{
    public GameObject hexPrefab;
    GameObject floor;
    float scaleY;
    List<List<int>> tiles= new List<List<int>>();
    // Start is called before the first frame update
    void Start()
    {
        Vector3 floorPos = new Vector3 (0,0,0);
        scaleY = hexPrefab.transform.localScale.y;
        for(int i = 0; i <7; i++)
        {
            tiles.Add(new List<int>());
        }
        for (int i = 0; i < 6; i ++)
        {                        
            floor = Instantiate(hexPrefab);
            floorPos.y += Mathf.Sqrt(3)*(scaleY * 0.5f);    
            floor.transform.position = floorPos;
            tiles[4].Add( i );
        }
        floorPos = new Vector3(0, 0, 0);
        floorPos.x += 0.75f * 1;
        floorPos.y += Mathf.Sqrt(3) * (scaleY * 0.25f);
        for (int i = 0; i < 5; i++)
        {
            floor = Instantiate(hexPrefab);
            floorPos.y += Mathf.Sqrt(3) * (scaleY * 0.5f);
            floor.transform.position = floorPos;
        }
        floorPos = new Vector3(0, 0, 0);
        floorPos.x += 2*(0.75f * 1);
        floorPos.y += 2*(Mathf.Sqrt(3) * (scaleY * 0.25f));
        for (int i = 0; i < 4; i++)
        {
            floor = Instantiate(hexPrefab);
            floorPos.y += Mathf.Sqrt(3) * (scaleY * 0.5f);
            floor.transform.position = floorPos;
        }
        floorPos = new Vector3(0, 0, 0);
        floorPos.x += 3 * (0.75f * 1);
        floorPos.y += 3 * (Mathf.Sqrt(3) * (scaleY * 0.25f));
        for (int i = 0; i < 3; i++)
        {
            floor = Instantiate(hexPrefab);
            floorPos.y += Mathf.Sqrt(3) * (scaleY * 0.5f);
            floor.transform.position = floorPos;
        }
        floorPos = new Vector3(0, 0, 0);
        floorPos.x -= 0.75f * 1;
        floorPos.y += Mathf.Sqrt(3) * (scaleY * 0.25f);
        for (int i = 0; i < 5; i++)
        {
            floor = Instantiate(hexPrefab);
            floorPos.y += Mathf.Sqrt(3) * (scaleY * 0.5f);
            floor.transform.position = floorPos;
        }
        floorPos = new Vector3(0, 0, 0);
        floorPos.x -= 2 * (0.75f * 1);
        floorPos.y += 2 * (Mathf.Sqrt(3) * (scaleY * 0.25f));
        for (int i = 0; i < 4; i++)
        {
            floor = Instantiate(hexPrefab);
            floorPos.y += Mathf.Sqrt(3) * (scaleY * 0.5f);
            floor.transform.position = floorPos;
        }
        floorPos = new Vector3(0, 0, 0);
        floorPos.x -= 3 * (0.75f * 1);
        floorPos.y += 3 * (Mathf.Sqrt(3) * (scaleY * 0.25f));
        for (int i = 0; i < 3; i++)
        {
            floor = Instantiate(hexPrefab);
            floorPos.y += Mathf.Sqrt(3) * (scaleY * 0.5f);
            floor.transform.position = floorPos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
