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
    // ������ ��, ������ �Ʒ�, �Ʒ�, ���� �Ʒ�, ���� ��, ��
    //int[] dx = { 1, 1, 0, -1, -1, 0 };
    //int[] dy = { -1, 0, 1, 1, 0, -1 };
    // ��, ������ ��, ������ �Ʒ�, �Ʒ�, ���� �Ʒ�, ���� ��
    int[] dx = { 0, 1, 1, 0, -1, -1 };
    int[] dy = { -1, -1, 0, 1, 1, 0 };
    bool[,] visited;
    bool[,] DFSVisited;
    public List<GameObject> deleteGemes = new List<GameObject>();
    public List<GameObject> deleteGemesBFS = new List<GameObject>();
    public List<GameObject> deleteGemesDFS = new List<GameObject>();
    public List<GameObject> deleteGemesThree = new List<GameObject>();

    Dictionary<Vector2Int, GameObject> tiles;
    Dictionary<Vector2Int, GameObject> gemes; 
    // ������ ����
    GameObject originGem;
    bool match = false;
    Vector2Int originTilePos;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }
    void Start()
    {
        visited = new bool[100, 100];
        DFSVisited = new bool[100, 100];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RegisterTileScript(Tile tile)
    {
        tileScript = tile;
    }
    void ResetArray(bool[,] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                array[i, j] = false;
            }
        }
    }
    public bool CheckForMatchesBegin(Vector2Int tilePos)
    {
        ResetArray(visited);
        ResetArray(DFSVisited);
        gemes = tileScript.Gemes;
       
        // Ÿ�� ��ǥ
        if (gemes[tilePos])
        {  
            originTilePos = tilePos;
            
            // originGem = ������ ����(����)

            originGem = gemes[tilePos];

            // ���� ������ ������ ���� ����Ʈ�� �־��ش�.

            deleteGemes.Add(originGem);
            deleteGemesDFS.Add(originGem);
            // 6���� �������� ���� �˻�
            for (int dir = 0; dir < 6; dir++)
            {
                // dfs ���� ������ �������� 3�� ������ �˻�
                CheckThreeMatchesDFSBegin(tilePos, dir);
                // �ݴ� direction ���� 
                int oppositeDir = (dir + 3) % 6;
                // �ݴ� DFS
                CheckThreeMatchesDFSBegin(tilePos, oppositeDir);

                // �ѹ������� ������ 3�� ���ϸ� ������ �ȵǹǷ� ���� ���� �˻綧�� ���� �ʱ�ȭ
                if (deleteGemes.Count < 3)
                {
                    deleteGemes.Clear();
                    deleteGemes.Add(originGem);
                }
                else
                {
                    deleteGemesThree.AddRange(deleteGemes);
                    deleteGemes.Clear();
                    deleteGemes.Add(originGem);
                }
            }
            // 6���� ���� �˻簡 ������ 3�� �̻� ������
            // �����ش�.
            deleteGemes.Clear();
            CheckFourMatchesDFSBegin(tilePos, 0);
            deleteGemesThree.AddRange(deleteGemesDFS);

            if (deleteGemesThree.Count >= 3)
            {
                deleteGemesThree.Clear();
                deleteGemesDFS.Clear();
                return false;
            }
            else
            {
            deleteGemesThree.Clear();
            // ����Ʈ ����
            deleteGemesDFS.Clear();
                return true;
            }
        }
        else
        {
            return false;
        }


    }

    public void CheckForMatches(GameObject hitObject)
    {
        tiles = tileScript.Tiles;
        gemes = tileScript.Gemes;
        ResetArray(visited);
        ResetArray(DFSVisited);

        if (tiles.ContainsValue(hitObject))
        {
            // vlaue(����)���� key(��ǥ)ã��
            Vector2Int tilePos = tiles.FirstOrDefault(x => x.Value == hitObject).Key;
            originTilePos = tilePos;
            GameObject tileObject = hitObject;
            // originGem = ������ ����(����)
            originGem = tileObject.GetComponent<TileRay>().color;
            // ���� ������ ������ ���� ����Ʈ�� �־��ش�.
            deleteGemes.Add(originGem);
            deleteGemesDFS.Add(originGem);
            // 6���� �������� ���� �˻�
            for (int dir = 0; dir < 6; dir++)
            {
                // dfs ���� ������ �������� 3�� ������ �˻�
                CheckThreeMatchesDFS(tilePos, dir);
                // �ݴ� direction ���� 
                int oppositeDir = (dir + 3) % 6;
                // �ݴ� DFS
                CheckThreeMatchesDFS(tilePos, oppositeDir);
                // �ѹ������� ������ 3�� ���ϸ� ������ �ȵǹǷ� ���� ���� �˻綧�� ���� �ʱ�ȭ
                if (deleteGemes.Count < 3)
                {
                    deleteGemes.Clear();
                    deleteGemes.Add(originGem);
                }
            }
            // 6���� ���� �˻簡 ������ 3�� �̻� ������
            // �����ش�.
            //CheckFourMatchesBFS(tilePos);
            CheckFourMatchesDFS(tilePos, 0);
            deleteGemes.AddRange(deleteGemesDFS);
            if (deleteGemes.Count >= 3)
            {
                foreach (GameObject gameObject in deleteGemes)
                {
                    Destroy(gameObject);
                }

            }
            // ����Ʈ ����
            deleteGemes.Clear();
            deleteGemesDFS.Clear();
        }
    }

    //public void CheckFourMatchesBFS(Vector2Int tilePos)
    //{
    //    // ť ����
    //    Queue<Vector2Int> myQueue = new Queue<Vector2Int>();

    //    // ť�� ���(tilePos) 
    //    myQueue.Enqueue(tilePos);

    //    // ó�� ��ǥ q��
    //    int originQ = tilePos.x;
    //    // ó�� ��ǥ r��
    //    int originR = tilePos.y;
    //    // ó�� ��ǥ �湮 ó��
    //    BFSVisited[tilePos.x + 10, tilePos.y + 10] = true;

    //    while (myQueue.Any())
    //    {
    //        int q = myQueue.First().x;
    //        int r = myQueue.First().y;
    //        myQueue.Dequeue();
    //        // 6���� �˻�
    //        for (int i = 0; i < 6; i++)
    //        {
    //            // ��ǥ�� �����ϰ� && ���� ���� ������ �����ϰ� && �湮���� ���� ���¶��
    //            int nextQ = q + dx[i];
    //            int nextR = r + dy[i];

    //            // �湮 üũ���ְ�
    //            if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !BFSVisited[nextQ + 10, nextR + 10])
    //            {
    //                Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);
    //                BFSVisited[nextQ + 10, nextR + 10] = true;

    //                // ���� ����Ʈ�� ��´�.
    //                deleteGemesBFS.Add(tileScript.Tiles[nextTilePos].GetComponent<TileRay>().color);

    //                // ���� Ÿ�� ��ǥ �߰�
    //                myQueue.Enqueue(nextTilePos);
    //            }
    //        }
    //    }
    //}
    public void CheckFourMatchesDFSBegin(Vector2Int tilePos, int depth)
    {
        
        int q = tilePos.x;
        int r = tilePos.y;

        // ó�� ��ǥ üũ
        DFSVisited[q + 10, r + 10] = true;

        for (int i = 0; i < 6; i++)
        {
            int nextQ = q + dx[i];
            int nextR = r + dy[i];
            // ó������ ���ƿԴٸ�?
            if (depth > 2 && nextQ == originTilePos.x && nextR == originTilePos.y)
            {
                match = true;
            }
            if (IsInsideGrid(nextQ, nextR) && HasSameColorBegin(q, r, nextQ, nextR) && !DFSVisited[nextQ + 10, nextR + 10])
            {
                DFSVisited[nextQ + 10, nextR + 10] = true;

                Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

                deleteGemesDFS.Add(tileScript.Gemes[nextTilePos]);

                CheckFourMatchesDFS(nextTilePos, depth + 1);

                // ó������ ���ƿ��� �ʾҴٸ�
                if (match == false)
                {
                    DFSVisited[q + 10, r + 10] = false;
                    deleteGemesDFS.RemoveAt(deleteGemesDFS.Count - 1);
                }
            }
        }
    }

    public void CheckThreeMatchesDFSBegin(Vector2Int tilePos, int dir)
    {
        int q = tilePos.x;
        int r = tilePos.y;

        int nextQ = q + dx[dir];
        int nextR = r + dy[dir];

        if (IsInsideGrid(nextQ, nextR) && HasSameColorBegin(q, r, nextQ, nextR) && !visited[nextQ + 10, nextR + 10])
        {
            // �湮 Ÿ�� üũ
            visited[nextQ + 10, nextR + 10] = true;

            // ���� �湮�� Ÿ�� ��ǥ
            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

            // ������ ���� ����
            deleteGemes.Add(tileScript.Gemes[nextTilePos]);
            CheckThreeMatchesDFS(nextTilePos, dir);
        }
    }

    public void CheckFourMatchesDFS(Vector2Int tilePos, int depth)
    {

        int q = tilePos.x;
        int r = tilePos.y;

        // ó�� ��ǥ üũ
        DFSVisited[q + 10, r + 10] = true;

        for (int i = 0; i < 6; i++)
        {
            int nextQ = q + dx[i];
            int nextR = r + dy[i];
            // ó������ ���ƿԴٸ�?
            if (depth > 2 && nextQ == originTilePos.x && nextR == originTilePos.y)
            {
                match = true;
            }
            if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !DFSVisited[nextQ + 10, nextR + 10])
            {
                DFSVisited[nextQ + 10, nextR + 10] = true;

                Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

                deleteGemesDFS.Add(tileScript.Tiles[nextTilePos].GetComponent<TileRay>().color);

                CheckFourMatchesDFS(nextTilePos, depth + 1);

                // ó������ ���ƿ��� �ʾҴٸ�
                if (match == false)
                {
                    DFSVisited[q + 10, r + 10] = false;
                    deleteGemesDFS.RemoveAt(deleteGemesDFS.Count - 1);
                }
            }
        }
    }

    public void CheckThreeMatchesDFS(Vector2Int tilePos, int dir)
    {
        int q = tilePos.x;
        int r = tilePos.y;

        int nextQ = q + dx[dir];
        int nextR = r + dy[dir];

        if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !visited[nextQ + 10, nextR + 10])
        {
            // �湮 Ÿ�� üũ
            visited[nextQ + 10, nextR + 10] = true;

            // ���� �湮�� Ÿ�� ��ǥ
            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

            // ������ ���� ����
            deleteGemes.Add(tileScript.Tiles[nextTilePos].GetComponent<TileRay>().color);
            CheckThreeMatchesDFS(nextTilePos, dir);
        }
    }
    private bool HasSameColorBegin(int row1, int col1, int row2, int col2)
    {
        if (IsInsideGrid(row2, col2))
        {
            Vector2Int currentTile = new Vector2Int(row1, col1);
            Vector2Int nextTile = new Vector2Int(row2, col2);

            // Check if the keys are present in the 'gemes' dictionary
            if (gemes.ContainsKey(currentTile) && gemes.ContainsKey(nextTile))
            {
                if (gemes[currentTile] && gemes[nextTile])
                {
                    if (gemes[currentTile].name == gemes[nextTile].name)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private bool IsInsideGrid(int nextQ, int nextR)
    {
        Vector2Int nextTile = new Vector2Int(nextQ, nextR);
        if (tileScript.Tiles.ContainsKey(nextTile))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool HasSameColor(int row1, int col1, int row2, int col2)
    {
        if (IsInsideGrid(row2, col2))
        {
            Vector2Int currentTile = new Vector2Int(row1, col1);
            Vector2Int nextTile = new Vector2Int(row2, col2);

            TileRay currentTileRay = tileScript.Tiles[currentTile].GetComponent<TileRay>();
            TileRay nextTileRay = tileScript.Tiles[nextTile].GetComponent<TileRay>();

            if (currentTileRay != null && currentTileRay.color != null && nextTileRay != null && nextTileRay.color != null)
            {
                if (currentTileRay.color.name == nextTileRay.color.name)
                {
                    return true;
                }
            }
        }
        return false;
    }
}