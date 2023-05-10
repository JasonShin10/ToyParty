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
    public List<Vector2Int> deleteGemesThreeVec = new List<Vector2Int>();
    public List<Vector2Int> deleteGemesFourVec = new List<Vector2Int>();
    public List<Vector2Int> deleteGemesVec = new List<Vector2Int>();

    // ���� �ΰ��� �ٲ� ��� ����Ʈ
    public List<GameObject> switchGemes = new List<GameObject>();
    public List<GameObject> switchGemesBefore = new List<GameObject>();

    Dictionary<Vector2Int, GameObject> gemes;

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

        gemes = tileScript.Gemes;
        match = false;
        originTilePos = tilePos;
        if (gemes[tilePos])
        {
            deleteGemesThreeVec.Add(tilePos);
            deleteGemesFourVec.Add(tilePos);
            // 6���� �������� ���� �˻�
            for (int dir = 0; dir < 6; dir++)
            {
                CheckThreeMatchesDFS(tilePos, dir);
                int oppositeDir = (dir + 3) % 6;
                CheckThreeMatchesDFS(tilePos, oppositeDir);

                // �ѹ������� ������ 3�� ���ϸ� ������ �ȵǹǷ� ���� ���� �˻綧�� ���� �ʱ�ȭ
                if (deleteGemesThreeVec.Count < 3)
                {
                    deleteGemesThreeVec.Clear();
                    deleteGemesThreeVec.Add(tilePos);
                }
                else
                {
                    deleteGemesVec.AddRange(deleteGemesThreeVec);
                    deleteGemesThreeVec.Clear();
                    deleteGemesThreeVec.Add(tilePos);
                }
            }
            // 6���� ���� �˻簡 ������ 3�� �̻� ������
            // �����ش�.
            deleteGemesThreeVec.Clear();
            CheckFourMatchesDFS(tilePos, 0);
            deleteGemesVec.AddRange(deleteGemesFourVec);

            if (deleteGemesVec.Count >= 3)
            {
                if (scriptName != "Tile")
                {
                    foreach (Vector2Int pos in deleteGemesVec)
                    {
                        Destroy(gemes[pos]);
                        GemesRefill(pos);
                    }
                }           
                deleteGemesVec.Clear();
                deleteGemesFourVec.Clear();
                return false;
            }
            else
            {
                deleteGemesVec.Clear();
                deleteGemesFourVec.Clear();
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
            deleteGemesThreeVec.Add(nextTilePos);
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
                deleteGemesFourVec.Add(nextTilePos);
                CheckFourMatchesDFS(nextTilePos, depth + 1);

                if (match == false)
                {
                    fourVisited[nextQ + 10, nextR + 10] = false;
                    deleteGemesFourVec.RemoveAt(deleteGemesFourVec.Count - 1);
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
            if (gemes.ContainsKey(currentTile) && gemes.ContainsKey(nextTile))
            {
                if (gemes[currentTile] && gemes[nextTile])
                {
                    if (gemes[currentTile].tag == gemes[nextTile].tag)
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
        return (tileScript.Gemes.ContainsKey(nextTile));
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
        // ���� Ŭ���� switchGemes�� �Ű��ص�
        switchGemes = inputSwitchGemes;
        switchGemesBefore = inputSwitchGemes;
        // ������ 2���̰� && ��ȯ ���� �ƴ϶��?
        if (switchGemes.Count == 2 && !swapping)
        {
            StartCoroutine(JamPosChange());
            // ��ȯ�۾��� �ѹ��� �����ϰԲ� 
            swapping = true;
            // ���� ���� ��ġ����������?
        }
    }

    public IEnumerator JamPosChange()
    {
        // ���õ� ������ �ι�° ���� ��ġ����
        jamOne = switchGemes[0].transform.position;
        jamTwo = switchGemes[1].transform.position;

        // ������ �ð���ŭ ��ġ�� ��ȯ�Ѵ�.
        while (currentTime < maxTime)
        {
            switchGemes[0].transform.position = Vector3.Lerp(jamOne, jamTwo, currentTime / maxTime);
            switchGemes[1].transform.position = Vector3.Lerp(jamTwo, jamOne, currentTime / maxTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        // �� ������ ��ȯ�� �����ϰ� ������ �ϰԲ� ��ġ�� �ٲ��ش�(lerp)�� ���� ������ �ȹٲ���� �ִ�.
        switchGemes[0].transform.position = jamTwo;
        switchGemes[1].transform.position = jamOne;

        for (int i = 0; i < switchGemes.Count; i++)
        {
            switchGemes[i].GetComponent<Jam>().touched = false;
        }
        Vector2Int tilePosOne = tileScript.WorldToAxial(jamOne, tileScript.width);
        Vector2Int tilePosTwo = tileScript.WorldToAxial(jamTwo, tileScript.width);
        GameObject gemOne = gemes[tilePosOne];
        GameObject gemTwo = gemes[tilePosTwo];
        gemes[tilePosOne] = gemTwo;
        gemes[tilePosTwo] = gemOne;
        gemes[tilePosOne].name = string.Format(gemTwo.tag + " " + " {0} , {1}", tilePosOne.x, tilePosOne.y);
        gemes[tilePosTwo].name = string.Format(gemOne.tag + " " + " {0} , {1}", tilePosTwo.x, tilePosTwo.y);
        backPosOne = CheckForMatches(tilePosOne);
        backPosTwo = CheckForMatches(tilePosTwo);

        currentTime = 0;
        // ���� ��ȯ �۾��� �Ϸ�Ǿ���
        if (!backPosOne && !backPosTwo)
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
        else
        {
            // ��ųʸ� ���� �ٲٴ� �Լ�
            tilePosOne = tileScript.WorldToAxial(jamOne, tileScript.width);
            tilePosTwo = tileScript.WorldToAxial(jamTwo, tileScript.width);         
            gemOne = gemes[tilePosOne];
            gemTwo = gemes[tilePosTwo];
            gemes[tilePosOne] = gemTwo;
            gemes[tilePosTwo] = gemOne;
            gemes[tilePosOne].name = string.Format(gemTwo.tag + " " + " {0} , {1}", tilePosOne.x, tilePosOne.y);
            gemes[tilePosTwo].name = string.Format(gemOne.tag + " " + " {0} , {1}", tilePosTwo.x, tilePosTwo.y);
        }
        swapping = false;
        switchGemes.Clear();
        
    }

    private void GemesRefill(Vector2Int pos)
    {
        int x = pos.x ;
        int y = pos.y - 1;
        Vector2Int upPos = new Vector2Int(x, y);
        if (deleteGemesVec.Contains(upPos))
        {
            return;
        }
        else
        {
            //gemes[upPos].transform.position = 
        }
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