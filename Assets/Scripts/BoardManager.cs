using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    // Start is called before the first frame update

    private Tile tileScript;
    int row;
    int col;
    // 위, 아래, 오른쪽 위, 왼쪽 아래, 왼쪽 위, 오른쪽 아래
    int[] dx = { 0, 0, 1, -1, -1, 1 };
    int[] dy = { 1, -1, 0, -1, 0, -1 };
    bool[,] visited;
    public List<GameObject> deleteGemes = new List<GameObject>();

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

    public void RegissterTileScript(Tile tile)
    {
        tileScript = tile;
    }

    public void CheckForMatches(GameObject hitObject)
    {
        List<List<GameObject>> tiles = tileScript.Tiles;
        visited = new bool[100, 100];
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                GameObject tile = tiles[i][j];
                if (tile == hitObject)
                {
                    row = i;
                    col = j;
                    CheckThreeMatchesDFS(i, j,0);
                }
            }
        }
    }

    public void CheckThreeMatchesDFS(int r, int c, int dir)
    {
        visited[r, c] = true;
        deleteGemes.Add(tileScript.Tiles[r][c]);
        int dd = dir;
        // 3이면 2로 만들어야되는데
        if (dd < 2)
        {
            dd = 0;
        }
        else if (2 <= dd && dd < 4)
        {
            dd = 2;
        }
        else if (4 <= dd && dd < 6)
        {
            dd = 4;
        }

        for (int i = dd; i < 6 - dd; i++)
        {
            int nextRow = r + dx[i];
            int nextColumn = c + dy[i];
            if (IsInsideGrid(nextRow, nextColumn) && !visited[nextRow, nextColumn] && HasSameColor(r, c, nextRow, nextColumn))
            {
                if (i < 2)
                {
                    deleteGemes.Add(tileScript.Tiles[nextRow][nextColumn]);
                    CheckThreeMatchesDFS(nextRow, nextColumn, i);
                    visited[nextRow, nextColumn] = false;
                }
                else if (2 <= i && i < 4)
                {
                    deleteGemes.Add(tileScript.Tiles[nextRow][nextColumn]);
                    CheckThreeMatchesDFS(nextRow, nextColumn, i);
                    visited[nextRow, nextColumn] = false;
                }
                else if (4 <= i && i < 6)
                {
                    deleteGemes.Add(tileScript.Tiles[nextRow][nextColumn]);
                    CheckThreeMatchesDFS(nextRow, nextColumn, i);
                    visited[nextRow, nextColumn] = false;
                }
            }
        }  
    }

    private bool IsInsideGrid(int row, int col)
    {
        return row >= 0 && row < tileScript.Tiles.Count && col >= 0 && col < tileScript.Tiles[row].Count;
    }

    private bool HasSameColor(int row1, int col1, int row2, int col2)
    {
        if (IsInsideGrid(row1, col1) && IsInsideGrid(row2, col2))
        {
            return tileScript.Tiles[row1][col1].GetComponent<TileRay>().color.name == tileScript.Tiles[row2][col2].GetComponent<TileRay>().color.name;
        }
        return false;
    }
}
