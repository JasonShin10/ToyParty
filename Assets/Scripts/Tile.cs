using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject hexPrefab;
    [SerializeField] private GameObject[] jamPrefabs;

    [SerializeField] int[] tileCounts = { 3, 4, 5, 6, 5, 4, 3 };

    private List<List<GameObject>> tiles = new List<List<GameObject>>();
    private List<List<GameObject>> gemes = new List<List<GameObject>>();
    public List<List<GameObject>> Tiles
    {
        get
        {
            return tiles;
        }
    }
    public List<List<GameObject>> Gemes
    {
        get
        {
            return gemes;
        }
    }
    private void Start()
    {
        BoardManager.instance.RegissterTileScript(this);
        CreateTiles();
        CreateJams();
    }

    private void CreateTiles()
    {
        float scaleY = hexPrefab.transform.localScale.y;
        for (int xOffset = -3; xOffset <= 3; xOffset++)
        {
            List<GameObject> columns = new List<GameObject>();
            int tileCount = tileCounts[xOffset + 3];
            Vector3 floorPos = new Vector3(xOffset * 0.75f, Mathf.Abs(xOffset) * (Mathf.Sqrt(3) * scaleY * 0.25f), 0);
            for (int i = 0; i < tileCount; i++)
            {
                GameObject tile = Instantiate(hexPrefab);
                floorPos.y += Mathf.Sqrt(3) * scaleY * 0.5f;
                tile.transform.position = floorPos;
                columns.Add(tile);
            }
            tiles.Add(columns);
        }
    }

    private void CreateJams()
    {
        float scaleY = hexPrefab.transform.localScale.y;

        for (int xOffset = -3; xOffset <= 3; xOffset++)
        {
            List<GameObject> columns = new List<GameObject>();
            int tileCount = tileCounts[xOffset + 3];
            Vector3 jamPos = new Vector3(xOffset * 0.75f, Mathf.Abs(xOffset) * (Mathf.Sqrt(3) * scaleY * 0.25f), 0);

            for (int i = 0; i < tileCount; i++)
            {
                int random = Random.Range(0, jamPrefabs.Length);
                GameObject jam = Instantiate(jamPrefabs[random]);
                jamPos.y += Mathf.Sqrt(3) * scaleY * 0.5f;
                jam.transform.position = jamPos + new Vector3(0, 0, -0.25f);
                columns.Add(jam);
            }
            gemes.Add(columns);
        }
    }
}