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
    int a;
    int b;
    int d;
    List<GameObject> deleteGemes = new List<GameObject>();

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
        bool[,] visited = new bool[tileScript.Tiles.Count, tileScript.Tiles[0].Count];
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
        //for (int i = 2; i < 4; i++)
        //{
        //    int nextRow = r + dx[i];
        //    int nextColumn = c + dy[i];
        //    if (IsInsideGrid(nextRow, nextColumn) && !visited[nextRow, nextColumn] && HasSameColor(r, c, nextRow, nextColumn))
        //    {
        //        CheckThreeMatchesDFS(nextRow, nextColumn);
        //        visited[nextRow, nextColumn] = false;
        //    }
        //}
        //for (int i = 4; i < 6; i++)
        //{
        //    int nextRow = r + dx[i];
        //    int nextColumn = c + dy[i];
        //    if (IsInsideGrid(nextRow, nextColumn) && !visited[nextRow, nextColumn] && HasSameColor(r, c, nextRow, nextColumn))
        //    {
        //        CheckThreeMatchesDFS(nextRow, nextColumn);
        //        visited[nextRow, nextColumn] = false;
        //    }
        //}
    }


    public bool CheckThreeMatches(int r, int c)
    {
        Queue<Vector3Int> rowColumnQueue = new Queue<Vector3Int>();
        bool[,] visited = new bool[tileScript.Tiles.Count, tileScript.Tiles[0].Count];
        Vector3Int rowColumn = new Vector3Int(r, c, a);
        visited[r, c] = true;
        rowColumnQueue.Enqueue(rowColumn);

        while (rowColumnQueue.Count > 0)
        {
            Vector3Int current = rowColumnQueue.Dequeue();
            int currentRow = current.x;
            int currentColumn = current.y;
            for (int i = 0; i < 6; i++)
            {
                int nextRow = currentRow + dx[i];
                int nextColumn = currentColumn + dy[i];

                if (IsInsideGrid(nextRow, nextColumn) && !visited[nextRow, nextColumn] && HasSameColor(currentRow, currentColumn, nextRow, nextColumn))
                {
                    if (i < 2)
                    {
                        rowColumn = new Vector3Int(nextRow, nextColumn, a++);

                    }
                    else if (2 <= i && i < 4)
                    {
                        rowColumn = new Vector3Int(nextRow, nextColumn, b++);
                    }
                    else if (4 <= i && i < 6)
                    {
                        rowColumn = new Vector3Int(nextRow, nextColumn, d++);
                    }
                    rowColumnQueue.Enqueue(rowColumn);
                    visited[nextRow, nextColumn] = true;
                }
            }
        }

        return false;
    }

    private bool IsInsideGrid(int row, int col)
    {
        return row >= 0 && row < tileScript.Tiles.Count && col >= 0 && col < tileScript.Tiles[row].Count;
    }

    private bool HasSameColor(int row1, int col1, int row2, int col2)
    {
        return tileScript.Tiles[row1][col1].GetComponent<TileRay>().color == tileScript.Tiles[row2][col2].GetComponent<TileRay>().color;
    }
}
