using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update

    private Tile tileScript;

    // 위, 오른쪽 위, 오른쪽 아래, 아래, 왼쪽 아래, 왼쪽 위
    int[] dx = { 0, 1, 1, 0, -1, -1 };
    int[] dy = { -1, -1, 0, 1, 1, 0 };
    
    // 일직선 보석 세개 매치 검사
    bool[,] threeVisited;
    // 인접한 네개 보석 매치 검사
    bool[,] fourVisited;

    // 일직선 보석, 네개 보석 담는 리스트
    public List<GameObject> deleteGemesThree = new List<GameObject>();
    public List<GameObject> deleteGemesFour = new List<GameObject>();

    // 위에 두 보석 리스트 합쳐서 삭제해주는 리스트
    public List<GameObject> deleteGemes = new List<GameObject>();

    // 보석 두개가 바뀔때 담는 리스트
    public List<GameObject> switchGemes = new List<GameObject>();

    // **이건 없어도 될듯?**
    public List<GameObject> switchGemesBefore = new List<GameObject>();

    // **같은 키에 다른 게임오브젝트가 들어갈 수 없다.**
    Dictionary<Vector2Int, GameObject> tiles;
    Dictionary<Vector2Int, GameObject> gemes;

    // 처음 움직인 보석
    GameObject originGem;

    // 인접한 네개의 보석이 백트래킹 탐색을 끝낸후 자기 자신에게 되돌아 왔는지 판단 해주는 변수
    bool match = false;

    // 처음 시작한 타일 키값
    Vector2Int originTilePosBegin;
    Vector2Int originTilePos;
    void Start()
    {
        // 배열 공간 설정
        threeVisited = new bool[100, 100];
        fourVisited = new bool[100, 100];
    }

    // Update is called once per frame
    void Update()
    {

    }

    // TileClass 받아오는 함수
    public void RegisterTileScript(Tile tile)
    {
        tileScript = tile;
    }
    // 배열 초기화 함수
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
    #region 타일 설치 기능 


    public bool CheckForMatchesBegin(Vector2Int tilePos)
    {
        ResetArray(threeVisited);
        ResetArray(fourVisited);
        gemes = tileScript.Gemes;
        match = false;
        // 타일 좌표
        if (gemes[tilePos])
        {
            originTilePosBegin = tilePos;
            // originGem = 움직인 보석(기준)

            originGem = gemes[tilePos];

            // 기준 보석을 삭제할 보석 리스트에 넣어준다.

            deleteGemesThree.Add(originGem);
            deleteGemesFour.Add(originGem);
            // 6가지 방향으로 보석 검사
            for (int dir = 0; dir < 6; dir++)
            {
                // dfs 같은 보석이 일직선에 3개 같은지 검사
                CheckThreeMatchesDFSBegin(tilePos, dir);
                // 반대 direction 방향 
                int oppositeDir = (dir + 3) % 6;
                // 반대 DFS
                CheckThreeMatchesDFSBegin(tilePos, oppositeDir);

                // 한방향으로 보석이 3개 이하면 성립이 안되므로 다음 방향 검사때를 위해 초기화
                if (deleteGemesThree.Count < 3)
                {
                    deleteGemesThree.Clear();
                    deleteGemesThree.Add(originGem);
                }
                else
                {
                    deleteGemes.AddRange(deleteGemesThree);
                    deleteGemesThree.Clear();
                    deleteGemesThree.Add(originGem);
                }
            }
            // 6가지 방향 검사가 끝나고 3개 이상 있으면
            // 없애준다.
            deleteGemesThree.Clear();
            CheckFourMatchesDFSBegin(tilePos, 0);
            deleteGemes.AddRange(deleteGemesFour);

            if (deleteGemes.Count >= 3)
            {
                deleteGemes.Clear();
                deleteGemesFour.Clear();
                return false;
            }
            else
            {
                deleteGemes.Clear();
                // 리스트 정리
                deleteGemesFour.Clear();
                return true;
            }
        }
        else
        {
            return false;
        }
    }
    public void CheckThreeMatchesDFSBegin(Vector2Int tilePos, int dir)
    {
        int q = tilePos.x;
        int r = tilePos.y;

        int nextQ = q + dx[dir];
        int nextR = r + dy[dir];

        if (IsInsideGrid(nextQ, nextR) && HasSameColorBegin(q, r, nextQ, nextR) && !threeVisited[nextQ + 10, nextR + 10])
        {
            // 방문 타일 체크
            threeVisited[nextQ + 10, nextR + 10] = true;

            // 다음 방문할 타일 좌표
            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

            // 삭제할 보석 저장
            deleteGemesThree.Add(tileScript.Gemes[nextTilePos]);
            CheckThreeMatchesDFSBegin(nextTilePos, dir);
        }
    }
    public void CheckFourMatchesDFSBegin(Vector2Int tilePos, int depth)
    {

        int q = tilePos.x;
        int r = tilePos.y;

        // 처음 좌표 체크
        fourVisited[q + 10, r + 10] = true;

        for (int i = 0; i < 6; i++)
        {
            int nextQ = q + dx[i];
            int nextR = r + dy[i];
            // 처음으로 돌아왔다면?
            if (depth > 2 && nextQ == originTilePosBegin.x && nextR == originTilePosBegin.y)
            {
                match = true;
            }
            if (IsInsideGrid(nextQ, nextR) && HasSameColorBegin(q, r, nextQ, nextR) && !fourVisited[nextQ + 10, nextR + 10])
            {
                fourVisited[nextQ + 10, nextR + 10] = true;

                Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

                deleteGemesFour.Add(tileScript.Gemes[nextTilePos]);

                CheckFourMatchesDFSBegin(nextTilePos, depth + 1);

                // 처음으로 돌아오지 않았다면
                if (match == false)
                {
                    fourVisited[nextQ + 10, nextR + 10] = false;
                    deleteGemesFour.RemoveAt(deleteGemesFour.Count - 1);
                }
            }
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

    #endregion

    #region 같은 타일 검색기능
    public bool CheckForMatches(GameObject hitObject)
    {
        tiles = tileScript.Tiles;
        gemes = tileScript.Gemes;
        ResetArray(threeVisited);
        ResetArray(fourVisited);
        match = false;
        if (tiles.ContainsValue(hitObject))
        {
            // vlaue(보석)으로 key(좌표)찾기
            Vector2Int tilePos = tiles.FirstOrDefault(x => x.Value == hitObject).Key;
            originTilePos = tilePos;
            GameObject tileObject = hitObject;
            // originGem = 움직인 보석(기준)
            originGem = tileObject.GetComponent<TileRay>().color;
            // 기준 보석을 삭제할 보석 리스트에 넣어준다.
            deleteGemesThree.Add(originGem);
            deleteGemesFour.Add(originGem);
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
                if (deleteGemesThree.Count < 3)
                {
                    deleteGemesThree.Clear();
                    deleteGemesThree.Add(originGem);
                }
                else
                {
            foreach (GameObject a in deleteGemesThree)
            {
                        print(a.name);
                Vector2Int tileWhere = gemes.FirstOrDefault(x => x.Value == a).Key;
                print("Three" + tileWhere);
            }
                    deleteGemes.AddRange(deleteGemesThree);
                    deleteGemesThree.Clear();
                    deleteGemesThree.Add(originGem);
                }
            }
            // 6가지 방향 검사가 끝나고 3개 이상 있으면
            // 없애준다.
            //CheckFourMatchesBFS(tilePos);
            match = false;
            deleteGemesThree.Clear();
            CheckFourMatchesDFS(tilePos, 0);
            foreach (GameObject a in deleteGemesFour)
            {
                Vector2Int tileWhere = gemes.FirstOrDefault(x => x.Value == a).Key;
                print("BackTracking" + tileWhere);
            }
            deleteGemes.AddRange(deleteGemesFour);
            if (deleteGemes.Count >= 3)
            {
                foreach (GameObject gameObject in deleteGemes)
                {
                    Destroy(gameObject);
                }
                deleteGemes.Clear();
                deleteGemesFour.Clear();
                return true;
            }
            else
            {
                deleteGemes.Clear();
                // 리스트 정리
                deleteGemesFour.Clear();
                return false;
            }
            // 리스트 정리

        }
        else
        {
        return false;

        }
    }

    public void CheckFourMatchesDFS(Vector2Int tilePos, int depth)
    {
        int q = tilePos.x;
        int r = tilePos.y;
        print(q + " " + r);
        // 처음 좌표 체크
        fourVisited[q + 10, r + 10] = true;

        for (int i = 0; i < 6; i++)
        {
            int nextQ = q + dx[i];
            int nextR = r + dy[i];
            // 처음으로 돌아왔다면?
            if (depth > 2 && nextQ == originTilePos.x && nextR == originTilePos.y)
            {
                match = true;
            }
            if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !fourVisited[nextQ + 10, nextR + 10])
            {
                fourVisited[nextQ + 10, nextR + 10] = true;

                Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

                deleteGemesFour.Add(tileScript.Tiles[nextTilePos].GetComponent<TileRay>().color);
                //print("deleteGemesFour" + deleteGemesFour.Count);
                //print(match);
                CheckFourMatchesDFS(nextTilePos, depth + 1);

                // 처음으로 돌아오지 않았다면
                if (match == false)
                {
                    fourVisited[nextQ + 10, nextR + 10] = false;
                    deleteGemesFour.RemoveAt(deleteGemesFour.Count - 1);
                }
            }
        }


    }

    public void CheckThreeMatchesDFS(Vector2Int tilePos, int dir)
    {
        int q = tilePos.x;
        int r = tilePos.y;
        //print(q + " " + r);

        int nextQ = q + dx[dir];
        int nextR = r + dy[dir];
        //print(nextQ + " " + nextR);
        if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !threeVisited[nextQ + 10, nextR + 10])
        {
            // 방문 타일 체크
            threeVisited[nextQ + 10, nextR + 10] = true;

            // 다음 방문할 타일 좌표
            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

            // 삭제할 보석 저장
            deleteGemesThree.Add(tileScript.Tiles[nextTilePos].GetComponent<TileRay>().color);
            //print("deleteGemesThree" + deleteGemesThree.Count);
            CheckThreeMatchesDFS(nextTilePos, dir);
        }
        foreach (GameObject a in deleteGemesThree)
        {
            Vector2Int tileWhere = gemes.FirstOrDefault(x => x.Value == a).Key;
            //print("Three" + tileWhere);
        }
    }
    //public void CheckFourMatchesBFS(Vector2Int tilePos)
    //{
    //    // 큐 선언
    //    Queue<Vector2Int> myQueue = new Queue<Vector2Int>();

    //    // 큐에 노드(tilePos) 
    //    myQueue.Enqueue(tilePos);

    //    // 처음 좌표 q값
    //    int originQ = tilePos.x;
    //    // 처음 좌표 r값
    //    int originR = tilePos.y;
    //    // 처음 좌표 방문 처리
    //    BFSVisited[tilePos.x + 10, tilePos.y + 10] = true;

    //    while (myQueue.Any())
    //    {
    //        int q = myQueue.First().x;
    //        int r = myQueue.First().y;
    //        myQueue.Dequeue();
    //        // 6방향 검사
    //        for (int i = 0; i < 6; i++)
    //        {
    //            // 좌표가 존재하고 && 같은 색의 보석이 존재하고 && 방문하지 않은 상태라면
    //            int nextQ = q + dx[i];
    //            int nextR = r + dy[i];

    //            // 방문 체크해주고
    //            if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !BFSVisited[nextQ + 10, nextR + 10])
    //            {
    //                Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);
    //                BFSVisited[nextQ + 10, nextR + 10] = true;

    //                // 보석 리스트에 담는다.
    //                deleteGemesBFS.Add(tileScript.Tiles[nextTilePos].GetComponent<TileRay>().color);

    //                // 다음 타일 좌표 추가
    //                myQueue.Enqueue(nextTilePos);
    //            }
    //        }
    //    }
    //}
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
    #endregion 
    private float currentTime = 0;
    private float maxTime = 1f;
    private Vector3 jamOne;
    private Vector3 jamTwo;
    public bool swapping = false;

    // inputManager에서 터치된 두개의 보석 정보를 가져온다.
    //if (backPos != true && !swapping )
    //{
    //    StartCoroutine(JamPosChange());
    //    backPos = false;
    //}
    private bool backPosOne = false;
    private bool backPosTwo = false;
    public void HandleGemSwap(List<GameObject> inputSwitchGemes)
    {
        // 여기 클래스 switchGemes로 옮겨준뒤
        switchGemes = inputSwitchGemes;
        switchGemesBefore = inputSwitchGemes;
        // 갯수가 2개이고 && 교환 중이 아니라면?
        if (switchGemes.Count == 2 && !swapping)
        {
            StartCoroutine(JamPosChange());
            // 교환작업이 한번만 실행하게끔 
            swapping = true;
            // 같은 색이 매치되지않으면?
        }
    }

 
    public IEnumerator JamPosChange()
    {
        jamOne = switchGemes[0].transform.position;
        jamTwo = switchGemes[1].transform.position;

        while (currentTime < maxTime)
        {
            switchGemes[0].transform.position = Vector3.Lerp(jamOne, jamTwo, currentTime / maxTime);
            switchGemes[1].transform.position = Vector3.Lerp(jamTwo, jamOne, currentTime / maxTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        switchGemes[0].transform.position = jamTwo;
        switchGemes[1].transform.position = jamOne;
        for (int i = 0; i < switchGemes.Count; i++)
        {
            switchGemes[i].GetComponent<Jam>().touched = false;
        }
        backPosOne = switchGemes[0].GetComponent<Jam>().FindMyHexagon();
        //backPosTwo = switchGemes[1].GetComponent<Jam>().FindMyHexagon();   
        currentTime = 0;
        // 보석 교환 작업이 완료되었음
        if (!backPosOne)
        { 
            jamOne = switchGemes[0].transform.position;
            jamTwo = switchGemes[1].transform.position;

            while (currentTime < maxTime)
            {
                switchGemes[0].transform.position = Vector3.Lerp(jamOne, jamTwo, currentTime / maxTime);
                switchGemes[1].transform.position = Vector3.Lerp(jamTwo, jamOne, currentTime / maxTime);
                currentTime += Time.deltaTime;
                yield return null;
            }
            switchGemes[0].transform.position = jamTwo;
            switchGemes[1].transform.position = jamOne;
            for (int i = 0; i < switchGemes.Count; i++)
            {
                switchGemes[i].GetComponent<Jam>().touched = false;
            }
            currentTime = 0;
        }
        swapping = false;
        switchGemes.Clear();
    }

    public void ResetJamSelection()
    {
        for (int i = 0; i < switchGemes.Count; i++)
        {
            switchGemes[i].GetComponent<Jam>().touched = false;
        }
        switchGemes.Clear();
        swapping = false;
    }
}