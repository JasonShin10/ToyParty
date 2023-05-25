using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.tvOS;

// List tile 없어도 될듯
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
    public List<Vector2Int> deleteGemsFinal = new List<Vector2Int>();

    Dictionary<Vector2Int, GameObject> gems;
    Dictionary<GameObject, Vector2Int> gemPositions;

    // 처음 시작한 타일 키값
    Vector2Int originTilePos;
    // 보석 두개가 바뀔때 담는 리스트
    public List<GameObject> switchGems = new List<GameObject>();

    bool match = false;
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
    #region check matches function 

    // 인접한 네개의 보석이 백트래킹 탐색을 끝낸후 자기 자신에게 되돌아 왔는지 판단 해주는 변수
    public bool CheckForMatches(Vector2Int tilePos)
    {
        // 배열 초기화 
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
            // 6가지 방향으로 보석 검사
            for (int dir = 0; dir < 6; dir++)
            {
                ThreeMatches(tilePos, dir);
                int oppositeDir = (dir + 3) % 6;
                ThreeMatches(tilePos, oppositeDir);

                // 한방향으로 보석이 3개 이하면 성립이 안되므로 다음 방향 검사때를 위해 초기화
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

            List<Vector2Int> deleteGemesVecDuplicate;
            List<GameObject> moveGems = new List<GameObject>();
            if (deleteGemsFinal.Count >= 3)
            {
                if (scriptName != "Tile")
                {
                    deleteGemesVecDuplicate = deleteGemsFinal.Distinct().ToList();
                    foreach (Vector2Int pos in deleteGemesVecDuplicate)
                    {
                        if (gems.ContainsKey(pos))
                        {
                            moveGems.Add(gems[pos]);
                        }
                    }
                    deleteGemesVecDuplicate.Clear();
                    foreach (GameObject gem in moveGems)
                    {
                        if (gemPositions.ContainsKey(gem))
                        {
                            Vector3 emptyPos = gem.transform.position;
                            Destroy(gem);
                            print("Destroy" + gem);
                            GemsRefill(gemPositions[gem], emptyPos);
                        }
                    }
                    deleteGemsFinal.Clear();
                    deleteGemsFourMatches.Clear();
                    List<GameObject> refillGemsCopy = new List<GameObject>(refillGems);
                    foreach (GameObject gem in refillGemsCopy)
                    {
                        CheckForMatches(gemPositions[gem]);
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


    public IEnumerator GemPosChange()
    {


        // Swap the positions of the gems
        Vector3 gemOne = switchGems[0].transform.position;
        Vector3 gemTwo = switchGems[1].transform.position;

        yield return SwapGems(gemOne, gemTwo);

        ResetGemsTouchedState();

        UpdateGemPositions(gemOne, gemTwo);

        // 서로의 딕셔너리 값 바꿔준다.
        Vector2Int tilePosOne = tileScript.WorldToAxial(gemOne, tileScript.width);
        Vector2Int tilePosTwo = tileScript.WorldToAxial(gemTwo, tileScript.width);

        bool backPosOne = CheckForMatches(tilePosOne);
        bool backPosTwo = CheckForMatches(tilePosTwo);

        // 보석 교환 작업이 완료되었음
        // 둘다 매치가 안되었다면
        if (backPosOne && backPosTwo)
        {
            yield return SwapGems(gemTwo, gemOne);
            ResetGemsTouchedState();
            UpdateGemPositions(gemTwo, gemOne);
        }
        swapping = false;
        switchGems.Clear();
    }

    //private IEnumerator SwapGems(GameObject gemOne, GameObject gemTwo)
    //{
    //    Vector3 gemOnePos = switchGems[0].transform.position;
    //    Vector3 gemTwoPos = switchGems[1].transform.position;
    //    switchGems[0].GetComponent<Jam>().moveGem(gemTwoPos);
    //    switchGems[1].GetComponent<Jam>().moveGem(gemOnePos);
    //    yield return null;

    //}

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
        // 두 보석의 교환을 완전하게 마무리 하게끔 위치를 바꿔준다(lerp)로 인해 완전히 안바뀔수도 있다.
        switchGems[0].transform.position = posTwo;
        switchGems[1].transform.position = posOne;
        currentTime = 0;
    }

    private void ResetGemsTouchedState()
    {
        for (int i = 0; i < switchGems.Count; i++)
        {
            switchGems[i].GetComponent<Jam>().touched = false;
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

    public void ResetJamSelection()
    {
        for (int i = 0; i < switchGems.Count; i++)
        {
            switchGems[i].GetComponent<Jam>().touched = false;
        }
        switchGems.Clear();
        swapping = false;
    }

    #endregion
    List<GameObject> refillGems = new List<GameObject>();
    private void GemsRefill(Vector2Int originPos, Vector3 originEmptyPos)
    {
        int x = originPos.x;
        int y = originPos.y - 1;
        Vector2Int nextPos = new Vector2Int(x, y);
        if (gems.ContainsKey(nextPos))
        {
            GameObject upGeme = gems[nextPos];
            //StartCoroutine(upGeme.GetComponent<Jam>().moveGem(originEmptyPos));
            Vector3 emptyUpPos = gems[nextPos].transform.position;
            upGeme.transform.position = originEmptyPos;
            gems[originPos] = upGeme;
            gemPositions[upGeme] = originPos;
            gems[originPos].name = string.Format(gems[originPos].tag + " " + " {0} , {1}", x, y + 1);
            // originPos로 이동했으니
            gems.Remove(nextPos);
            refillGems.Add(upGeme);
            GemsRefill(nextPos, emptyUpPos);
        }
        else
        {
            return;
        }
    }
}