using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.tvOS;

// List tile ��� �ɵ�
public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;

    private Tile tileScript;
    private string scriptName;

    // ��, ������ ��, ������ �Ʒ�, �Ʒ�, ���� �Ʒ�, ���� ��
    int[] dx = { 0, 1, 1, 0, -1, -1 };
    int[] dy = { -1, -1, 0, 1, 1, 0 };

    bool[,] threeVisited;
    bool[,] fourVisited;

    public List<Vector2Int> deleteGemsThreeMatches = new List<Vector2Int>();
    public List<Vector2Int> deleteGemsFourMatches = new List<Vector2Int>();
    public List<Vector2Int> deleteGemsFinal = new List<Vector2Int>();

    Dictionary<Vector2Int, GameObject> gems;
    Dictionary<GameObject, Vector2Int> gemPositions;

    // ó�� ������ Ÿ�� Ű��
    Vector2Int originTilePos;
    // ���� �ΰ��� �ٲ� ��� ����Ʈ
    public List<GameObject> switchGems = new List<GameObject>();

    bool match = false;
    // �⺻�Լ��� ������ Awake -> Start ������ �Լ� ����������Ŭ�� ���ߴ� ���� ���� �ڵ� ������ ������ �� �����ϴ�.
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

    // region ������ �ٰ��ݿ� �޼��� ������ �ٰ����� �����ϰ� �����ϴ� ���� �����ϴ�.
    // �� ���� �۾��̶�� ���� �ڵ� ��Ÿ���� ������ ���� ������, ���� �۾��� ���� ���θ��� ��Ÿ���� �����س����� ���� ������ ���鿡�� �����մϴ�.

    #region check matches function 

    // ������ �װ��� ������ ��Ʈ��ŷ Ž���� ������ �ڱ� �ڽſ��� �ǵ��� �Դ��� �Ǵ� ���ִ� ����
    public bool CheckForMatches(Vector2Int tilePos, out List<Vector2Int> destroyedPositions)
    {
        destroyedPositions = new List<Vector2Int>();

        // �迭 �ʱ�ȭ 
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
            // 6���� �������� ���� �˻�
            for (int dir = 0; dir < 6; dir++)
            {
                ThreeMatches(tilePos, dir);
                int oppositeDir = (dir + 3) % 6;
                ThreeMatches(tilePos, oppositeDir);

                // �ѹ������� ������ 3�� ���ϸ� ������ �ȵǹǷ� ���� ���� �˻綧�� ���� �ʱ�ȭ
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
            // 6���� ���� �˻簡 ������ 3�� �̻� ������
            // �����ش�.
            deleteGemsThreeMatches.Clear();
            FourMatches(tilePos, 0);
            deleteGemsFinal.AddRange(deleteGemsFourMatches);

            // �Ʒ��� ���� ���� ���ǹ��� �ݺ����� ���� ��� �� ���� �ڵ尡 ������ �ʽ��ϴ�.
            // ���� ���ǹ��� �ݺ����� ���� ��� �ּ��� �޾��ִ� ���� �����ϴ�.
            //
            // else return�� �߰�ȣ�� �޾��ֽô� �� �ڵ带 �ϰ������� ���̰� �ϴ� �κ��̶� ���ҽ��ϴ�!
            // ���ǹ�, �ݺ����� �ڵ带 ���� �߰�ȣ�� ���, �ٹٲ� ���� �ڵ� ��Ÿ���� �ϰ����̾ �������� ���ҽ��ϴ�.
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
                            Destroy(gem.gameObject);
                            destroyedPositions.Add(gemPositions[gem]);
                            

                            // To do : Destory ���� ����
                            //gem.PlayDestroyAnimation();

                            GemsRefill(gemPositions[gem]);
                            print("Destroy" + gem);
                        }
                    }
                    //deleteGemsFinal.Clear();
                    //deleteGemsFourMatches.Clear();
                    //List<GameObject> refillGemsCopy = new List<GameObject>(refillGems);
                    //foreach (GameObject gem in refillGemsCopy)
                    //{
                    //    CheckForMatches(gemPositions[gem]);
                    //}
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

    private bool isPlayingAnimation = false;

    public IEnumerator GemPosChange()
    {
        // Swap the positions of the gems
        Vector3 gemOne = switchGems[0].transform.position;
        Vector3 gemTwo = switchGems[1].transform.position;

        yield return SwapGems(gemOne, gemTwo);

        ResetGemsTouchedState();

        UpdateGemPositions(gemOne, gemTwo);

        // ������ ��ųʸ� �� �ٲ��ش�.
        Vector2Int tilePosOne = tileScript.WorldToAxial(gemOne, tileScript.width);
        Vector2Int tilePosTwo = tileScript.WorldToAxial(gemTwo, tileScript.width);

        List<Vector2Int> destroyedPositionsOne = new List<Vector2Int>();
        List<Vector2Int> destroyedPositionsTwo = new List<Vector2Int>();
        bool backPosOne = CheckForMatches(tilePosOne, destroyedPositionsOne);
        bool backPosTwo = CheckForMatches(tilePosTwo, destroyedPositionsTwo);

        while(backPosOne || backPosTwo)
        {
            if(backPosOne)
            {
                foreach(var gemPos in destroyedPositionsOne)
                {
                    List<GameObject> refillGemsCopy = new List<GameObject>(refillGems);
                    foreach (GameObject gem in refillGemsCopy)
                    {
                        CheckForMatches(gemPositions[gem]);
                    }
                }
            }
            
            if(backPosTwo)
            {
                foreach(var gemPos in destroyedPositionsTwo)
                {
                    CheckForMatches(gemPos, out var );
                }
            }

            destroyedPositionsOne = new List<Vector2Int>();
            destroyedPositionsTwo = new List<Vector2Int>();
            backPosOne = CheckForMatches(tilePosOne, destroyedPositionsOne);
            backPosTwo = CheckForMatches(tilePosTwo, destroyedPositionsTwo);
        }

        // ���� ��ȯ �۾��� �Ϸ�Ǿ���
        // �Ѵ� ��ġ�� �ȵǾ��ٸ�
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
        // �ڷ�ƾ���� �ð� ���� �۾��� �Ͻ� �� Time.deltaTime�� ����Ͻñ� ���ٴ�
        // yield return new Waitfortime(n)�� ����Ͻô� �� ȿ�������� ���Դϴ�.
        // ������ �ð���ŭ ��ġ�� ��ȯ�Ѵ�.
        while (currentTime < maxTime)
        {
            switchGems[0].transform.position = Vector3.Lerp(posOne, posTwo, currentTime / maxTime);
            switchGems[1].transform.position = Vector3.Lerp(posTwo, posOne, currentTime / maxTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        // �� ������ ��ȯ�� �����ϰ� ������ �ϰԲ� ��ġ�� �ٲ��ش�(lerp)�� ���� ������ �ȹٲ���� �ִ�.
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

    public void ResetJamSelection()
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

    private void GemsRefill(Vector2Int originPos)
    {
        int x = originPos.x;
        int y = originPos.y - 1;
        Vector2Int nextPos = new Vector2Int(x, y);

        if (gems.ContainsKey(nextPos) && gems[nextPos] && gems[nextPos].GetComponent<Gem>())
        {
            GameObject upGem = gems[nextPos];
            Vector3 emptyUpPos = gems[nextPos].transform.position;
            var gem = upGem.GetComponent<Gem>();
            gem.MoveAnimationPresent(originPos, () => { GemsRefill(nextPos, emptyUpPos); });
            //upGem.transform.position = originEmptyPos;
            gems[originPos] = upGem;
            gemPositions[upGem] = originPos;
            gems[originPos].name = string.Format(gems[originPos].tag + " " + " {0} , {1}", x, y + 1);
            gems.Remove(nextPos);
            refillGems.Add(upGem);
        }
    }
}