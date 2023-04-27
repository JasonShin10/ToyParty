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
    // 오른쪽 위, 오른쪽 아래, 아래, 왼쪽 아래, 왼쪽 위, 위
    int[] dx = { 1, 1, 0, -1, -1, 0 };
    int[] dy = { -1, 0, 1, 1, 0, -1 };
    bool[,] visited;
    bool[,] BFSVistied;
    public List<GameObject> deleteGemes = new List<GameObject>();
    public List<GameObject> deleteGEmesBFS = new List<GameObject>();

    public class CustomQueueItem
    {
        public Vector2Int Vec { get; set; }
        public int Value { get; set;}

        public CustomQueueItem(Vector2Int vec, int value)
        {
            Vec = vec;
            Value = value;
        }
    }

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
        BFSVistied = new bool[100, 100];

       
        if (tiles.ContainsValue(hitObject))
        {
            // vlaue(보석)으로 key(좌표)찾기
            Vector2Int tilePos = tiles.FirstOrDefault(x => x.Value == hitObject).Key;

            GameObject tileObject = hitObject;
            // originGem = 움직인 보석(기준)
            originGem = tileObject.GetComponent<TileRay>().color;
            // 기준 보석을 삭제할 보석 리스트에 넣어준다.
            deleteGemes.Add(originGem);

            // 6가지 방향으로 보석 검사
            for (int dir = 0; dir < 6; dir++)
            {
                // dfs 같은 보석이 일직선에 3개 같은지 검사
                CheckThreeMatchesDFS(tilePos, dir);
                // 반대 direction 방향 
                int oppositeDir = (dir + 3) % 6;
                // 반대 DFS
                CheckThreeMatchesDFS(tilePos, oppositeDir);
                // 한방향으로 보석이 3개 이하면 성립이 안되므로 다음 방향 검사때를 위해 초기화
                if (deleteGemes.Count < 3)
                {
                    deleteGemes.Clear();
                    deleteGemes.Add(originGem);
                }
            }
            // 6가지 방향 검사가 끝나고 3개 이상 있으면
            // 없애준다.
            if (deleteGemes.Count >= 3)
            {
                foreach (GameObject gameObject in deleteGemes)
                {
                    Destroy(gameObject);
                }
            }
            // 리스트 정리
            deleteGemes.Clear();
        }
    }

    public void CheckFourMatchesBFS(Vector2Int tilePos, int dir)
    {
        // 큐 선언
        Queue<CustomQueueItem> myQueue = new Queue<CustomQueueItem>();

        Vector2Int myVector = tilePos;
        int myValue = 0;

        // 큐에 노드(tilePos) 
        myQueue.Enqueue(new CustomQueueItem(myVector, myValue));
        int originQ = tilePos.x;
        int originR = tilePos.y;
        BFSVistied[tilePos.x + 10, tilePos.y + 10] = true;
        
        while (!myQueue.Any())
        {
            int q = myQueue.First().Vec.x;
            int r = myQueue.First().Vec.y;
            myQueue.Dequeue();
            for (int i = 0; i < q; i++)
            {
                if (IsInsideGrid(q, r) && HasSameColor(q, r, originQ, originR) && !visited[q + 10, r + 10])
                {
                    Vector2Int nextTilePos = new Vector2Int(q,r);
                    BFSVistied[q,r] = true;

                    myQueue.Enqueue(new CustomQueueItem(nextTilePos, myValue + 1));
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
            // 방문 타일 체크
            visited[nextQ + 10, nextR + 10] = true;

            // 다음 방문할 타일 좌표
            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

            // 삭제할 보석 저장
            deleteGemes.Add(tileScript.Tiles[nextTilePos].GetComponent<TileRay>().color);
            CheckThreeMatchesDFS(nextTilePos, dir);
        }
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

        }
        return false;
    }
}
