using JetBrains.Annotations;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.tvOS;
using Sirenix.OdinInspector;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    private Tile tileScript;
    private string scriptName;

    // 위, 오른쪽 위, 오른쪽 아래, 아래, 왼쪽 아래, 왼쪽 위
    int[] dx = { 0, 1, 1, 0, -1, -1 };
    int[] dy = { -1, -1, 0, 1, 1, 0 };

    bool[,] threeVisited;
    bool[,] fourVisited;

    public List<Vector2Int> deleteGemsThreeMatches = new List<Vector2Int>();
    public List<Vector2Int> deleteGemsFourMatches = new List<Vector2Int>();

    // 파괴할 보석들을 모아두는 List
    public List<Vector2Int> deleteGemsFinal = new List<Vector2Int>();

    [ShowInInspector]
    public Dictionary<Vector2Int, GameObject> gems;
    [ShowInInspector]
    public Dictionary<GameObject, Vector2Int> gemPositions;
    [ShowInInspector]
    public Dictionary<Vector2Int, GameObject> mergeGem;
    [ShowInInspector]
    public Dictionary<GameObject, Vector2Int> mergeGemPos;
    Vector2Int originTilePos;

    public List<GameObject> switchGems = new List<GameObject>();

    bool match = false;
    private int movingGemsCounter = 0;

    public ParticleSystem deleteEffect;
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
        //Debug.Log(tileScript.newGems.Count);
    }

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

    #region check matches function 
    public void RefillAndRecheckBoard()
    {

        RefillGems();
        //ResetDictionary();

        //CreateRefillGem();
        //RecheckMatches();
    }

    private void RefillGems()
    {
        mergeGem = gems.Concat(tileScript.newGems).ToDictionary(pair => pair.Key, pair => pair.Value);
        mergeGemPos = gemPositions.Concat(tileScript.newGemsPos).ToDictionary(pair => pair.Key, pair => pair.Value);

        for (int q = 3; q >= -3; q--)
        {
            int r1 = Mathf.Max(-9, -q - 9);
            int r2 = Mathf.Min(3, -q + 3);

            for (int r = r2 - 1; r >= r1; r--)
            {
                Vector2Int currentPos = new Vector2Int(q, r);

                if (mergeGem.ContainsKey(currentPos) && mergeGem[currentPos] != null)
                {
                    continue;
                }

                Vector2Int? nextAvailableGemPos = FindNextAvailableGem(currentPos);

                if (!nextAvailableGemPos.HasValue)
                {
                    continue;
                }
                ShiftGemToEmptyPosition(currentPos, nextAvailableGemPos.Value);
            }
        }
    }

    private void ShiftGemToEmptyPosition(Vector2Int currentPos, Vector2Int availableGemPos)
    {
        GameObject movingGem = mergeGem[availableGemPos];
        mergeGem[currentPos] = movingGem;
        mergeGem[availableGemPos] = null;

        mergeGemPos[movingGem] = currentPos;
        movingGem.name = $"{movingGem.tag}  {currentPos.x} , {currentPos.y}";
        movingGemsCounter++;

        movingGem.GetComponent<Gem>().MoveAnimationPresent(
            tileScript.AxialToWorld(currentPos, tileScript.width),
            () =>
            {
                --movingGemsCounter;
                CheckAllGemsFinishedMoving();
            });
    }

    private void CheckAllGemsFinishedMoving()
    {
        if (movingGemsCounter == 0)
        {
            Debug.Log("ResetDictionary");
            ResetDictionary();
        }
    }
    public void ResetDictionary()
    {
        gems.Clear();
        gemPositions.Clear();
        tileScript.newGems.Clear();
        tileScript.newGemsPos.Clear();
        for (int q = -3; q <= 3; q++)
        {
            int r1 = Mathf.Max(-9, -q - 9);
            int r2 = Mathf.Min(3, -q + 3);

            for (int r = r1; r < r2; r++)
            {

                if (q == -3 && Mathf.Abs(r) > 3)
                {
                    continue;
                }
                else if (q == -2 && Mathf.Abs(r) > 5)
                {
                    continue;
                }
                else if (q == -1 && Mathf.Abs(r) > 7)
                {
                    continue;
                }
                else if (q == 1 && Mathf.Abs(r) > 8)
                {
                    continue;
                }
                else if (q == 2 && Mathf.Abs(r) > 7)
                {
                    continue;
                }
                else if (q == 3 && Mathf.Abs(r) > 6)
                {
                    continue;
                }
                Vector2Int axialCoord = new Vector2Int(q, r);
                int excludeR1 = Mathf.Max(-3, -q - 3);
                int excludeR2 = Mathf.Min(3, -q + 3);

                if (!(q >= -3 && q <= 3 && r >= excludeR1 && r < excludeR2))
                {
                    if (mergeGem.ContainsKey(axialCoord) && mergeGem[axialCoord] != null)
                    {
                        tileScript.newGems[axialCoord] = mergeGem[axialCoord];
                        tileScript.newGemsPos[mergeGem[axialCoord]] = axialCoord;
                    }
                    else
                    {
                        int random = Random.Range(0, tileScript.gemPrefabs.Length);
                        GameObject gem = Instantiate(tileScript.gemPrefabs[random], tileScript.newGem.transform);
                        Vector3 worldPos = tileScript.AxialToWorld(axialCoord, tileScript.width);
                        gem.transform.position = worldPos + new Vector3(0, 0, -0.25f);

                        gem.name = string.Format(gem.tag + " " + " {0} , {1}", axialCoord.x, axialCoord.y);
                        tileScript.newGems[axialCoord] = gem;
                        tileScript.newGemsPos[gem] = axialCoord;
                    }
                }
                else
                {
                    if (mergeGem.ContainsKey(axialCoord) && mergeGem[axialCoord] != null)
                    {
                        gems[axialCoord] = mergeGem[axialCoord];
                        gemPositions[mergeGem[axialCoord]] = axialCoord;
                    }
                }
            }
        }
        mergeGem.Clear();
        mergeGemPos.Clear();

        RecheckMatches();
    }

    private Vector2Int? FindNextAvailableGem(Vector2Int currentPos)
    {
        for (int y = currentPos.y - 1; y >= -9; y--)
        {
            Vector2Int checkPos = new Vector2Int(currentPos.x, y);
            if (mergeGem.ContainsKey(checkPos) && mergeGem[checkPos] != null)
            {
                return checkPos;
            }
        }
        return null;
    }

    private void RecheckMatches()
    {


        // 전체 게임판 순회
        for (int q = -3; q <= 3; q++)
        {
            int r1 = Mathf.Max(-3, -q - 3);
            int r2 = Mathf.Min(3, -q + 3);

            for (int r = r1; r < r2; r++)
            {
                Vector2Int currentPos = new Vector2Int(q, r);
                if (!CheckForMatches(currentPos, out List<GameObject> destroyGems))
                {

                    DestroyGemes(destroyGems);
                }
            }
        }


        //if (matchesFound)
        //{
        //    // 매치가 발견되면 재배치와 재검사 반복
        //    RefillAndRecheckBoard();
        //}
    }

    // 인접한 네개의 보석이 백트래킹 탐색을 끝낸후 자기 자신에게 되돌아 왔는지 판단 해주는 변수
    public bool CheckForMatches(Vector2Int tilePos, out List<GameObject> targetDestroyedGems)
    {
        targetDestroyedGems = new List<GameObject>();

        ResetArray(threeVisited);
        ResetArray(fourVisited);

        gems = tileScript.Gems;
        gemPositions = tileScript.GemPositions;
        match = false;
        originTilePos = tilePos;
        if (gems.ContainsKey(tilePos) && gems[tilePos])
        {
            deleteGemsThreeMatches.Add(tilePos);
            deleteGemsFourMatches.Add(tilePos);
          
            for (int dir = 0; dir < 6; dir++)
            {
                ThreeMatches(tilePos, dir);
                int oppositeDir = (dir + 3) % 6;
                ThreeMatches(tilePos, oppositeDir);


                if (deleteGemsThreeMatches.Count < 3)
                {
                    deleteGemsThreeMatches.Clear();
                    deleteGemsThreeMatches.Add(tilePos);
                }
                else
                {
                    deleteGemsFinal.AddRange(deleteGemsThreeMatches);
                    deleteGemsThreeMatches.Clear();
                    deleteGemsThreeMatches.Add(tilePos);
                }
            }
            // 6가지 방향 검사가 끝나고 3개 이상 있으면
            // 없애준다.
            deleteGemsThreeMatches.Clear();
            FourMatches(tilePos, 0);
            deleteGemsFinal.AddRange(deleteGemsFourMatches);

            List<GameObject> moveGems = new List<GameObject>();


            if (deleteGemsFinal.Count >= 3)
            {
                // 처음에 보석 세팅할때면 패스
                if (scriptName != "Tile")
                {
                    foreach (Vector2Int pos in deleteGemsFinal)
                    {
                        if (gems.ContainsKey(pos))
                        {
                            moveGems.Add(gems[pos]);
                        }
                    }


                    foreach (GameObject gem in moveGems)
                    {
                        if (gemPositions.ContainsKey(gem))
                        {
                            targetDestroyedGems.Add(gem);

                        }
                    }

                }

                deleteGemsFinal.Clear();
                deleteGemsFourMatches.Clear();

                return false;
            }
            else
            {
                deleteGemsFinal.Clear();
                deleteGemsFourMatches.Clear();
                return true;
            }
        }
        else
        {
            return true;
        }
    }

    public void ThreeMatches(Vector2Int tilePos, int dir)
    {
        int q = tilePos.x;
        int r = tilePos.y;

        int nextQ = q + dx[dir];
        int nextR = r + dy[dir];

        if (IsInsideGrid(nextQ, nextR) && HasSameColor(q, r, nextQ, nextR) && !threeVisited[nextQ + 10, nextR + 10])
        {
            threeVisited[nextQ + 10, nextR + 10] = true;
            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);
            deleteGemsThreeMatches.Add(nextTilePos);
            ThreeMatches(nextTilePos, dir);
        }
    }

    public void FourMatches(Vector2Int tilePos, int depth)
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
                deleteGemsFourMatches.Add(nextTilePos);
                FourMatches(nextTilePos, depth + 1);
                if (match == false)
                {
                    fourVisited[nextQ + 10, nextR + 10] = false;
                    deleteGemsFourMatches.RemoveAt(deleteGemsFourMatches.Count - 1);
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

    #region tile swap fuction
    private float currentTime = 0;
    private float maxTime = 1f;
    public bool swapping = false;

    public void HandleGemSwap(List<GameObject> inputSwitchGemes)
    {
        scriptName = this.GetType().Name;
        switchGems = inputSwitchGemes;
        if (switchGems.Count == 2 && !swapping)
        {
            StartCoroutine(GemPosChange());
            swapping = true;
        }
    }

    void DestroyGemes(List<GameObject> destroyTargetGems)
    {

        List<GameObject> toBeDestroyed = new List<GameObject>();
        destroyTargetGems = destroyTargetGems.Distinct().ToList();
        foreach (GameObject deleteGem in destroyTargetGems)
        {
            if (gemPositions.TryGetValue(deleteGem, out Vector2Int gemPos))
            {
                toBeDestroyed.Add(deleteGem);
            }
        }

        foreach (GameObject deleteGem in toBeDestroyed)
        {
            if (gemPositions.TryGetValue(deleteGem, out Vector2Int gemPos))
            {
                ParticleSystem particle = Instantiate(deleteEffect, deleteGem.transform.position, Quaternion.identity);
                ScoreSystem.instance.AddScore();
                Destroy(particle.gameObject, 1f);
                Destroy(deleteGem);

                gemPositions.Remove(deleteGem);
                gems.Remove(gemPos);
            }
        }

        RefillAndRecheckBoard();
    }
    public IEnumerator GemPosChange()
    {
        Vector3 gemOne = switchGems[0].transform.position;
        Vector3 gemTwo = switchGems[1].transform.position;

        yield return SwapGems(gemOne, gemTwo);

        ResetGemsTouchedState();

        UpdateGemPositions(gemOne, gemTwo);

        // 서로의 딕셔너리 값 바꿔준다.
        Vector2Int tilePosOne = tileScript.WorldToAxial(gemOne, tileScript.width);
        Vector2Int tilePosTwo = tileScript.WorldToAxial(gemTwo, tileScript.width);

        bool backPosOne = CheckForMatches(tilePosOne, out List<GameObject> destroyTargetGemsForOne);
        bool backPosTwo = CheckForMatches(tilePosTwo, out List<GameObject> destroyTargetGemsForTwo);
        List<GameObject> destroyTargetGems = new List<GameObject>(destroyTargetGemsForOne);
        destroyTargetGems.AddRange(destroyTargetGemsForTwo);



        DestroyGemes(destroyTargetGems);

        if (backPosOne && backPosTwo)
        {
            yield return SwapGems(gemTwo, gemOne);
            ResetGemsTouchedState();
            UpdateGemPositions(gemTwo, gemOne);
        }
        swapping = false;
        switchGems.Clear();
    }
    // 갈곳을 찾는 함수
    private IEnumerator SwapGems(Vector3 posOne, Vector3 posTwo)
    {

        // 설정된 시간만큼 위치를 교환한다.
        while (currentTime < maxTime)
        {
            switchGems[0].transform.position = Vector3.Lerp(posOne, posTwo, currentTime / maxTime);
            switchGems[1].transform.position = Vector3.Lerp(posTwo, posOne, currentTime / maxTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        switchGems[0].transform.position = posTwo;
        switchGems[1].transform.position = posOne;
        currentTime = 0;
    }

    private void ResetGemsTouchedState()
    {
        for (int i = 0; i < switchGems.Count; i++)
        {
            switchGems[i].GetComponent<Gem>().touched = false;
        }
    }

    private void UpdateGemPositions(Vector3 posOne, Vector3 posTwo)
    {
        Vector2Int tilePosOne = tileScript.WorldToAxial(posOne, tileScript.width);
        Vector2Int tilePosTwo = tileScript.WorldToAxial(posTwo, tileScript.width);
        GameObject newGemOne = gems[tilePosOne];
        GameObject newGemTwo = gems[tilePosTwo];
        gems[tilePosOne] = newGemTwo;
        gemPositions[newGemTwo] = tilePosOne;
        gems[tilePosTwo] = newGemOne;
        gemPositions[newGemOne] = tilePosTwo;
        gems[tilePosOne].name = string.Format(newGemTwo.tag + " " + " {0} , {1}", tilePosOne.x, tilePosOne.y);
        gems[tilePosTwo].name = string.Format(newGemOne.tag + " " + " {0} , {1}", tilePosTwo.x, tilePosTwo.y);
    }

    public void ResetGemSelection()
    {
        for (int i = 0; i < switchGems.Count; i++)
        {
            switchGems[i].GetComponent<Gem>().touched = false;
        }
        switchGems.Clear();
        swapping = false;
    }

    #endregion

    List<GameObject> refillGems = new List<GameObject>();
}



