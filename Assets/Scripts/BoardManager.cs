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

    // ��, ������ ��, ������ �Ʒ�, �Ʒ�, ���� �Ʒ�, ���� ��
    int[] dx = { 0, 1, 1, 0, -1, -1 };
    int[] dy = { -1, -1, 0, 1, 1, 0 };
    
    // ������ ���� ���� ��ġ �˻�
    bool[,] threeVisited;
    // ������ �װ� ���� ��ġ �˻�
    bool[,] fourVisited;

    // ������ ����, �װ� ���� ��� ����Ʈ
    public List<GameObject> deleteGemesThree = new List<GameObject>();
    public List<GameObject> deleteGemesFour = new List<GameObject>();

    // ���� �� ���� ����Ʈ ���ļ� �������ִ� ����Ʈ
    public List<GameObject> deleteGemes = new List<GameObject>();

    // ���� �ΰ��� �ٲ� ��� ����Ʈ
    public List<GameObject> switchGemes = new List<GameObject>();

    // **�̰� ��� �ɵ�?**
    public List<GameObject> switchGemesBefore = new List<GameObject>();

    // **���� Ű�� �ٸ� ���ӿ�����Ʈ�� �� �� ����.**
    Dictionary<Vector2Int, GameObject> tiles;
    Dictionary<Vector2Int, GameObject> gemes;

    // ó�� ������ ����
    GameObject originGem;

    // ������ �װ��� ������ ��Ʈ��ŷ Ž���� ������ �ڱ� �ڽſ��� �ǵ��� �Դ��� �Ǵ� ���ִ� ����
    bool match = false;

    // ó�� ������ Ÿ�� Ű��
    Vector2Int originTilePosBegin;
    Vector2Int originTilePos;
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
    #region Ÿ�� ��ġ ��� 


    public bool CheckForMatchesBegin(Vector2Int tilePos)
    {
        ResetArray(threeVisited);
        ResetArray(fourVisited);
        gemes = tileScript.Gemes;
        match = false;
        // Ÿ�� ��ǥ
        if (gemes[tilePos])
        {
            originTilePosBegin = tilePos;
            // originGem = ������ ����(����)

            originGem = gemes[tilePos];

            // ���� ������ ������ ���� ����Ʈ�� �־��ش�.

            deleteGemesThree.Add(originGem);
            deleteGemesFour.Add(originGem);
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
            // 6���� ���� �˻簡 ������ 3�� �̻� ������
            // �����ش�.
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
                // ����Ʈ ����
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
            // �湮 Ÿ�� üũ
            threeVisited[nextQ + 10, nextR + 10] = true;

            // ���� �湮�� Ÿ�� ��ǥ
            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

            // ������ ���� ����
            deleteGemesThree.Add(tileScript.Gemes[nextTilePos]);
            CheckThreeMatchesDFSBegin(nextTilePos, dir);
        }
    }
    public void CheckFourMatchesDFSBegin(Vector2Int tilePos, int depth)
    {

        int q = tilePos.x;
        int r = tilePos.y;

        // ó�� ��ǥ üũ
        fourVisited[q + 10, r + 10] = true;

        for (int i = 0; i < 6; i++)
        {
            int nextQ = q + dx[i];
            int nextR = r + dy[i];
            // ó������ ���ƿԴٸ�?
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

                // ó������ ���ƿ��� �ʾҴٸ�
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

    #region ���� Ÿ�� �˻����
    public bool CheckForMatches(GameObject hitObject)
    {
        tiles = tileScript.Tiles;
        gemes = tileScript.Gemes;
        ResetArray(threeVisited);
        ResetArray(fourVisited);
        match = false;
        if (tiles.ContainsValue(hitObject))
        {
            // vlaue(����)���� key(��ǥ)ã��
            Vector2Int tilePos = tiles.FirstOrDefault(x => x.Value == hitObject).Key;
            originTilePos = tilePos;
            GameObject tileObject = hitObject;
            // originGem = ������ ����(����)
            originGem = tileObject.GetComponent<TileRay>().color;
            // ���� ������ ������ ���� ����Ʈ�� �־��ش�.
            deleteGemesThree.Add(originGem);
            deleteGemesFour.Add(originGem);
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
            // 6���� ���� �˻簡 ������ 3�� �̻� ������
            // �����ش�.
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
                // ����Ʈ ����
                deleteGemesFour.Clear();
                return false;
            }
            // ����Ʈ ����

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
        // ó�� ��ǥ üũ
        fourVisited[q + 10, r + 10] = true;

        for (int i = 0; i < 6; i++)
        {
            int nextQ = q + dx[i];
            int nextR = r + dy[i];
            // ó������ ���ƿԴٸ�?
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

                // ó������ ���ƿ��� �ʾҴٸ�
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
            // �湮 Ÿ�� üũ
            threeVisited[nextQ + 10, nextR + 10] = true;

            // ���� �湮�� Ÿ�� ��ǥ
            Vector2Int nextTilePos = new Vector2Int(nextQ, nextR);

            // ������ ���� ����
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

    // inputManager���� ��ġ�� �ΰ��� ���� ������ �����´�.
    //if (backPos != true && !swapping )
    //{
    //    StartCoroutine(JamPosChange());
    //    backPos = false;
    //}
    private bool backPosOne = false;
    private bool backPosTwo = false;
    public void HandleGemSwap(List<GameObject> inputSwitchGemes)
    {
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
        // ���� ��ȯ �۾��� �Ϸ�Ǿ���
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