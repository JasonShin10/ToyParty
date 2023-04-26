using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    // Start is called before the first frame update

    private Tile tileScript;
    // 위, 아래, 오른쪽 위, 왼쪽 아래, 왼쪽 위, 오른쪽 아래
    // x > 3
    int[] dx = { 0, 0, 1, -1, -1, 1 };
    int[] dy = { -1, 1, -1, 1, 0, 0 };
    // x < 3 
    bool[,] visited;
    public List<GameObject> deleteGemes = new List<GameObject>();

    // 움직인 보석
    GameObject originGem;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RegisterTileScript(Tile tile)
    {
        tileScript = tile;
    }

    public void CheckForMatches(GameObject hitObject)
    {
        Dictionary<Vector2Int, GameObject> tiles = tileScript.Tiles;
        visited = new bool[100, 100];
        if (tiles.ContainsValue(hitObject))
        {
            Vector2Int tilePos = tiles.FirstOrDefault(x => x.Value == hitObject).Key;
            GameObject tileObject = hitObject;
            originGem = tileObject.GetComponent<TileRay>().color;
            deleteGemes.Add(originGem);
            CheckThreeMatchesDFS(tilePos,-1);
        }      
    }

    public void CheckThreeMatchesDFS(Vector2Int tilePos, int dir)
    {
        int q = tilePos.x;
        int r = tilePos.y;
        
        if (0 <= dir && dir <= 1)
        {
            dir = 0;
        }
        else if (2 <= dir && dir < 4)
        {
            dir = 2;
        }
        else if ( 4 <= dir && dir < 6)
        {
            dir = 4;
        }

        int a = 0;
        if (dir == 0)
        {
            a = 4;
        }
        else if (dir == 2)
        {
            a = 2;
        }
        else if (dir == 4)
        {
            a = 0;
        }
        if (dir == -1)
        {
            dir = 0;
        }
        for (int i = dir; i<6-a; i++)
        {
            int nextQ = q + dx[i];
            int nextR = r + dy[i];
            if (IsInsideGrid(nextQ, nextR) && HasSameColor(q,r,nextQ,nextR) && !visited[nextQ + 10,nextR + 10])
            {
                visited[nextQ+10,nextR+10] = true;
                Vector2Int nextTilePos = new Vector2Int(nextQ,nextR);
                deleteGemes.Add(tileScript.Tiles[nextTilePos].GetComponent<TileRay>().color);
                CheckThreeMatchesDFS(nextTilePos, i);
                if (deleteGemes.Count < 2)
                {
                    deleteGemes.Clear();
                    deleteGemes.Add(originGem);
                }
            }
        }


        //deleteGemes.Add(tileScript.Tiles[r][c]);
        //int dd = dir;
        //// 3이면 2로 만들어야되는데
        //if (0 <= dd && dd < 2)
        //{
        //    dd = 0;
        //}
        //else if (2 <= dd && dd < 4)
        //{
        //    dd = 2;
        //}
        //else if (4 <= dd && dd < 6)
        //{
        //    dd = 4;
        //}

        //int a = 0;
        //if (dd == 0)
        //{
        //    a = 4;
        //}
        //else if (dd == 2)
        //{
        //    a = 2;
        //}
        //else if (dd == 4)
        //{
        //    a = 0;
        //}
        //if (dd == -1)
        //{
        //    dd = 0;
        //}
        //for (int i = dd; i < 6 - a; i++)
        //{
        //    int nextRow = r + dx[i];
        //    int nextColumn = c + dy[i];
        //    if (IsInsideGrid(nextRow, nextColumn) && !visited[nextRow, nextColumn] && HasSameColor(r, c, nextRow, nextColumn))
        //    {
        //        if (i < 2)
        //        {
        //            visited[nextRow, nextColumn] = true;
        //            CheckThreeMatchesDFS(nextRow, nextColumn, i);

        //        }
        //        else if (2 <= i && i < 4)
        //        {
        //            visited[nextRow, nextColumn] = true;
        //            CheckThreeMatchesDFS(nextRow, nextColumn, i);

        //        }
        //        else if (4 <= i && i < 6)
        //        {
        //            visited[nextRow, nextColumn] = true;
        //            CheckThreeMatchesDFS(nextRow, nextColumn, i);
        //        }
        //    }
        //}
    }

    private bool IsInsideGrid(int nextQ, int nextR)
    {
        Vector2Int nextTile = new Vector2Int(nextQ,nextR);
        if (tileScript.Tiles.ContainsKey(nextTile))
        {
            return true;
        }
        else
        {
            return false;
        }
        //return nextQ >= 0 && nextR < tileScript.Tiles.Count && nextQ >= 0 && nextQ < tileScript.Tiles[nextR].Count;
    }

    private bool HasSameColor(int row1, int col1, int row2, int col2)
    {
        if (IsInsideGrid(row2, col2))
        {
            Vector2Int currentTile = new Vector2Int(row1, col1);
            Vector2Int nextTile = new Vector2Int(row2, col2);
            if (tileScript.Tiles[currentTile].GetComponent<TileRay>().color.name == tileScript.Tiles[nextTile].GetComponent<TileRay>().color.name)
            {
                return true;
            }
            //return tileScript.Tiles[row1][col1].GetComponent<TileRay>().color.name == tileScript.Tiles[row2][col2].GetComponent<TileRay>().color.name;
        }
        return false;
    }
}
