using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

// List tile 없어도 될듯
public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    private Tile tileScript;
    private string scriptName;

    // 위, 오른쪽 위, 오른쪽 아래, 아래, 왼쪽 아래, 왼쪽 위
    int[] dx = { 0, 1, 1, 0, -1, -1 };
    int[] dy = { -1, -1, 0, 1, 1, 0 };

    // 타일 방문검사
    bool[,] threeVisited;
    bool[,] fourVisited;

    // 일직선 보석, 네개 보석 담는 리스트
    public List<Vector2Int> deleteGemsThreeVec = new List<Vector2Int>();
    public List<Vector2Int> deleteGemsFourVec = new List<Vector2Int>();
    public List<Vector2Int> deleteGemsVec = new List<Vector2Int>();

    // 보석 두개가 바뀔때 담는 리스트
    public List<GameObject> switchGems = new List<GameObject>();

    Dictionary<Vector2Int, GameObject> gems;
    Dictionary<GameObject, Vector2Int> gemPositions;
    List<Vector2Int> deleteGemesVecDuplicate;
    List<GameObject> moveGems = new List<GameObject>();
    // 인접한 네개의 보석이 백트래킹 탐색을 끝낸후 자기 자신에게 되돌아 왔는지 판단 해주는 변수
    bool match = false;

    // 처음 시작한 타일 키값
    Vector2Int originTilePos;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update

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
        scriptName = tile.GetType().Name;
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
    #region 타일 검색 기능 

    public bool CheckForMatches(Vector2Int tilePos)
    {
        // 배열 초기화 
        ResetArray(threeVisited);
        ResetArray(fourVisited);

        gems = tileScript.Gems;
        gemPositions = tileScript.GemPositions;
        match = false;
        originTilePos = tilePos;
        if (gems[tilePos])
        {
            deleteGemsThreeVec.Add(tilePos);
            deleteGemsFourVec.Add(tilePos);
            // 6가지 방향으로 보석 검사
            for (int dir = 0; dir < 6; dir++)
            {
                CheckThreeMatchesDFS(tilePos, dir);
                int oppositeDir = (dir + 3) % 6;
                CheckThreeMatchesDFS(tilePos, oppositeDir);

                // 한방향으로 보석이 3개 이하면 성립이 안되므로 다음 방향 검사때를 위해 초기화
                if (deleteGemsThreeVec.Count < 3)
                {
                    deleteGemsThreeVec.Clear();
                    deleteGemsThreeVec.Add(tilePos);
                }
                else
                {
                    deleteGemsVec.AddRange(deleteGemsThreeVec);
                    deleteGemsThreeVec.Clear();
                    deleteGemsThreeVec.Add(tilePos);
                }
            }
            // 6가지 방향 검사가 끝나고 3개 이상 있으면
            // 없애준다.
            deleteGemsThreeVec.Clear();
            CheckFourMatchesDFS(tilePos, 0);
            deleteGemsVec.AddRange(deleteGemsFourVec);

            if (deleteGemsVec.Count >= 3)
            {
                if (scriptName != "Tile")
                {
                    deleteGemesVecDuplicate = deleteGemsVec.Distinct().ToList();
                    foreach (Vector2Int pos in deleteGemesVecDuplicate)
                    {
                        moveGems.Add(gems[pos]);
                    }
                    foreach (GameObject gem in moveGems)
                    {
                        if (gemPositions.ContainsKey(gem)) // check if the key exists in the dictionary
                        {
                            Vector3 emptyPos = gem.transform.position;
                            Destroy(gem);
                            print("Destroy" + gem);
                            GemesRefill(gemPositions[gem], emptyPos);
                        }
                    }

                    deleteGemesVecDuplicate.Clear();
                }
                deleteGemsVec.Clear();
                deleteGemsFourVec.Clear();
                return false;
            }
            else
            {
                deleteGemsVec.Clear();
                deleteGemsFourVec.Clear();
                return true;
            }
        }
        else
        {
            return true;
        }
    }
    public void CheckThreeMatchesDFS(Vector2Int tilePos, int dir)
    {
        int q = tilePos.x;
        int r = tilePos.y;

        int nextQ = q + dx[dir];
        int nextR = r + dy[dir];

        if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !threeVisited[nextQ + 10, nextR + 10])
        {
            threeVisited[nextQ + 10, nextR + 10] = true;

            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);
            deleteGemsThreeVec.Add(nextTilePos);
            CheckThreeMatchesDFS(nextTilePos, dir);
        }
    }
    public void CheckFourMatchesDFS(Vector2Int tilePos, int depth)
    {
        int q = tilePos.x;
        int r = tilePos.y;

        fourVisited[q + 10, r + 10] = true;

        for (int i = 0; i < 6; i++)
        {
            int nextQ = q + dx[i];
            int nextR = r + dy[i];

            if (depth > 2 && nextQ == originTilePos.x && nextR == originTilePos.y)
            {
                match = true;
            }
            if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !fourVisited[nextQ + 10, nextR + 10])
            {
                fourVisited[nextQ + 10, nextR + 10] = true;

                Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);
                deleteGemsFourVec.Add(nextTilePos);
                CheckFourMatchesDFS(nextTilePos, depth + 1);

                if (match == false)
                {
                    fourVisited[nextQ + 10, nextR + 10] = false;
                    deleteGemsFourVec.RemoveAt(deleteGemsFourVec.Count - 1);
                }
            }
        }
    }
    private bool HasSameColor(int row1, int col1, int row2, int col2)
    {
        if (IsInsideGrid(row2, col2))
        {
            Vector2Int currentTile = new Vector2Int(row1, col1);
            Vector2Int nextTile = new Vector2Int(row2, col2);

            // Check if the keys are present in the 'gemes' dictionary
            if (gems.ContainsKey(currentTile) && gems.ContainsKey(nextTile))
            {
                if (gems[currentTile] && gems[nextTile])
                {
                    if (gems[currentTile].tag == gems[nextTile].tag)
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
        return (tileScript.Gems.ContainsKey(nextTile));
    }
    #endregion

    private float currentTime = 0;
    private float maxTime = 1f;
    private Vector3 jamOne;
    private Vector3 jamTwo;
    public bool swapping = false;
    private bool backPosOne = false;
    private bool backPosTwo = false;

    public void HandleGemSwap(List<GameObject> inputSwitchGemes)
    {
        scriptName = this.GetType().Name;
        switchGems = inputSwitchGemes;
        
        if (switchGems.Count == 2 && !swapping)
        {
            StartCoroutine(JamPosChange());
            swapping = true;
        }
    }

    public IEnumerator JamPosChange()
    {
        // 선택된 보석과 두번째 보석 위치정보
        jamOne = switchGems[0].transform.position;
        jamTwo = switchGems[1].transform.position;

        // 설정된 시간만큼 위치를 교환한다.
        while (currentTime < maxTime)
        {
            switchGems[0].transform.position = Vector3.Lerp(jamOne, jamTwo, currentTime / maxTime);
            switchGems[1].transform.position = Vector3.Lerp(jamTwo, jamOne, currentTime / maxTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        // 두 보석의 교환을 완전하게 마무리 하게끔 위치를 바꿔준다(lerp)로 인해 완전히 안바뀔수도 있다.
        switchGems[0].transform.position = jamTwo;
        switchGems[1].transform.position = jamOne;

        for (int i = 0; i < switchGems.Count; i++)
        {
            switchGems[i].GetComponent<Jam>().touched = false;
        }
        // 서로의 딕셔너리 값 바꿔준다.
        Vector2Int tilePosOne = tileScript.WorldToAxial(jamOne, tileScript.width);
        Vector2Int tilePosTwo = tileScript.WorldToAxial(jamTwo, tileScript.width);
        GameObject gemOne = gems[tilePosOne];
        GameObject gemTwo = gems[tilePosTwo];
        gems[tilePosOne] = gemTwo;
        gemPositions[gemTwo] = tilePosOne;
        gems[tilePosTwo] = gemOne;
        gemPositions[gemOne] = tilePosTwo;
        gems[tilePosOne].name = string.Format(gemTwo.tag + " " + " {0} , {1}", tilePosOne.x, tilePosOne.y);
        gems[tilePosTwo].name = string.Format(gemOne.tag + " " + " {0} , {1}", tilePosTwo.x, tilePosTwo.y);
        backPosOne = CheckForMatches(tilePosOne);

        backPosTwo = CheckForMatches(tilePosTwo);

        currentTime = 0;
        // 보석 교환 작업이 완료되었음
        // 둘다 매치가 안되었다면
        if (backPosOne && backPosTwo)
        {
            jamOne = switchGems[0].transform.position;
            jamTwo = switchGems[1].transform.position;

            while (currentTime < maxTime)
            {
                switchGems[0].transform.position = Vector3.Lerp(jamOne, jamTwo, currentTime / maxTime);
                switchGems[1].transform.position = Vector3.Lerp(jamTwo, jamOne, currentTime / maxTime);
                currentTime += Time.deltaTime;
                yield return null;
            }
            switchGems[0].transform.position = jamTwo;
            switchGems[1].transform.position = jamOne;
            for (int i = 0; i < switchGems.Count; i++)
            {
                switchGems[i].GetComponent<Jam>().touched = false;
            }
            currentTime = 0;
            // 매치 안됐으면 다시 바꿔준다.
            tilePosOne = tileScript.WorldToAxial(jamOne, tileScript.width);
            tilePosTwo = tileScript.WorldToAxial(jamTwo, tileScript.width);
            gemOne = gems[tilePosOne];
            gemTwo = gems[tilePosTwo];
            gems[tilePosOne] = gemTwo;
            gemPositions[gemTwo] = tilePosOne;
            gems[tilePosTwo] = gemOne;
            gemPositions[gemOne] = tilePosTwo;
            gems[tilePosOne].name = string.Format(gemTwo.tag + " " + " {0} , {1}", tilePosOne.x, tilePosOne.y);
            gems[tilePosTwo].name = string.Format(gemOne.tag + " " + " {0} , {1}", tilePosTwo.x, tilePosTwo.y);
        }
        swapping = false;
        switchGems.Clear();
    }

   
    private void GemesRefill(Vector2Int originPos, Vector3 originEmptyPos)
    {
        int x = originPos.x;
        int y = originPos.y - 1;
        Vector2Int nextPos = new Vector2Int(x, y);

        if (gems.ContainsKey(nextPos))
        {
            GameObject upGeme = gems[nextPos];
            Vector3 emptyUpPos = gems[nextPos].transform.position;
            upGeme.transform.position = originEmptyPos;
            gems[originPos] = upGeme;
            gemPositions[upGeme] = originPos;
            gems[originPos].name = string.Format(gems[originPos].tag + " " + " {0} , {1}", x, y+1);
            // originPos로 이동했으니
            gems.Remove(nextPos);
            GemesRefill(nextPos, emptyUpPos);
        }
        else
        {
            return;
        }

    }
    //while (currentTime < maxTime)
    //{
    //    upGeme.transform.position = Vector3.Lerp(upGeme.transform.position, emptyPos, currentTime / maxTime);
    //    currentTime += Time.deltaTime;
    //}

    //if (deleteGemesVec.Contains(nextPos))
    //{
    //    for (int i = 0; i < deleteGemesVecDuplicate.Count; i++)
    //    {
    //        if (deleteGemesVecDuplicate[i] == nextPos && gemes[nextPos].tag == gemes[originPos].tag)
    //        {
    //            deleteGemesVecDuplicate[i] = originPos;
    //        }
    //    }
    //}
    public void ResetJamSelection()
    {
        for (int i = 0; i < switchGems.Count; i++)
        {
            switchGems[i].GetComponent<Jam>().touched = false;
        }
        switchGems.Clear();
        swapping = false;
    }
}

// 다 지우고 나서 해야한다.