using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

// List tile ��� �ɵ�
public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    private Tile tileScript;
    private string scriptName;

    // ��, ������ ��, ������ �Ʒ�, �Ʒ�, ���� �Ʒ�, ���� ��
    int[] dx = { 0, 1, 1, 0, -1, -1 };
    int[] dy = { -1, -1, 0, 1, 1, 0 };

    // Ÿ�� �湮�˻�
    bool[,] threeVisited;
    bool[,] fourVisited;

    // ������ ����, �װ� ���� ��� ����Ʈ
    public List<Vector2Int> deleteGemsThreeVec = new List<Vector2Int>();
    public List<Vector2Int> deleteGemsFourVec = new List<Vector2Int>();
    public List<Vector2Int> deleteGemsVec = new List<Vector2Int>();

    // ���� �ΰ��� �ٲ� ��� ����Ʈ
    public List<GameObject> switchGems = new List<GameObject>();

    Dictionary<Vector2Int, GameObject> gems;
    Dictionary<GameObject, Vector2Int> gemPositions;
    List<Vector2Int> deleteGemesVecDuplicate;
    List<GameObject> moveGems = new List<GameObject>();
    // ������ �װ��� ������ ��Ʈ��ŷ Ž���� ������ �ڱ� �ڽſ��� �ǵ��� �Դ��� �Ǵ� ���ִ� ����
    bool match = false;

    // ó�� ������ Ÿ�� Ű��
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
        // �迭 ���� ����
        threeVisited = new bool[100, 100];
        fourVisited = new bool[100, 100];
    }

    // Update is called once per frame
    void Update()
    {

    }

    // TileClass �޾ƿ��� �Լ�
    public void RegisterTileScript(Tile tile)
    {
        tileScript = tile;
        scriptName = tile.GetType().Name;
    }
    // �迭 �ʱ�ȭ �Լ�
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
    #region Ÿ�� �˻� ��� 

    public bool CheckForMatches(Vector2Int tilePos)
    {
        // �迭 �ʱ�ȭ 
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
            // 6���� �������� ���� �˻�
            for (int dir = 0; dir < 6; dir++)
            {
                CheckThreeMatchesDFS(tilePos, dir);
                int oppositeDir = (dir + 3) % 6;
                CheckThreeMatchesDFS(tilePos, oppositeDir);

                // �ѹ������� ������ 3�� ���ϸ� ������ �ȵǹǷ� ���� ���� �˻綧�� ���� �ʱ�ȭ
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
            // 6���� ���� �˻簡 ������ 3�� �̻� ������
            // �����ش�.
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
        // ���õ� ������ �ι�° ���� ��ġ����
        jamOne = switchGems[0].transform.position;
        jamTwo = switchGems[1].transform.position;

        // ������ �ð���ŭ ��ġ�� ��ȯ�Ѵ�.
        while (currentTime < maxTime)
        {
            switchGems[0].transform.position = Vector3.Lerp(jamOne, jamTwo, currentTime / maxTime);
            switchGems[1].transform.position = Vector3.Lerp(jamTwo, jamOne, currentTime / maxTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        // �� ������ ��ȯ�� �����ϰ� ������ �ϰԲ� ��ġ�� �ٲ��ش�(lerp)�� ���� ������ �ȹٲ���� �ִ�.
        switchGems[0].transform.position = jamTwo;
        switchGems[1].transform.position = jamOne;

        for (int i = 0; i < switchGems.Count; i++)
        {
            switchGems[i].GetComponent<Jam>().touched = false;
        }
        // ������ ��ųʸ� �� �ٲ��ش�.
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
        // ���� ��ȯ �۾��� �Ϸ�Ǿ���
        // �Ѵ� ��ġ�� �ȵǾ��ٸ�
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
            // ��ġ �ȵ����� �ٽ� �ٲ��ش�.
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
            // originPos�� �̵�������
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

// �� ����� ���� �ؾ��Ѵ�.