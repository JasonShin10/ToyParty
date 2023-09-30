using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class Tile : MonoBehaviour
{
    public static Tile instance;
    public GameObject hexPrefab;
    public GameObject[] gemPrefabs;
    public GameObject newGem;
    private Dictionary<Vector2Int, GameObject> gems = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, Vector2Int> gemPositions = new Dictionary<GameObject, Vector2Int>();

    public List<Vector2Int> gemFactoryTiles = new List<Vector2Int>();
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }
    private void Start()
    {
        BoardManager.instance.RegisterTileScript(this);
        CreateTiles();
        CreateGems();
        
        
    }
    [ShowInInspector]
    public Dictionary<Vector2Int, GameObject> Gems { get { return gems; } }
    [ShowInInspector]
    public Dictionary<GameObject, Vector2Int> GemPositions { get { return gemPositions; } }
    public float width;
    [ShowInInspector]
    public Dictionary<Vector2Int, GameObject> newGems = new Dictionary<Vector2Int, GameObject>();
    [ShowInInspector]
    public Dictionary<GameObject, Vector2Int> newGemsPos = new Dictionary<GameObject, Vector2Int>();
    private void CreateTiles()
    {
        // 6각형의 너비,폭(width)
        width = hexPrefab.transform.localScale.y;
        // q는 좌우방향
        for (int q = -3; q <= 3; q++)
        {
            // 수직 으로 쌓이는양
            // q = -3일때 r1 = 0
            // r2 = 3
            // 3개 쌓임
            // q = -2 일때 r1 = -1
            // r2 = 3
            // 4개 쌓임
           
            int r1 = Mathf.Max(-3, -q - 3);
            int r2 = Mathf.Min(3, -q + 3);

            for (int r = r1; r < r2; r++)
            {
               

                // 축 좌표를 생성한다.
                Vector2Int axialCoord = new Vector2Int(q, r);
                // 축 좌표를 월드 좌표로 변환한다.
                Vector3 worldPos = AxialToWorld(axialCoord, width);
                //int excludeR1 = Mathf.Max(-3, -q - 3);
                //int excludeR2 = Mathf.Min(3, -q + 3);
                //if (!(q >= -3 && q <= 3 && r >= excludeR1 && r < excludeR2))
                //{
                //    gemFactoryTiles.Add(tile);
                //}
                GameObject tile = Instantiate(hexPrefab,transform);
                tile.transform.position = worldPos;
                tile.name = string.Format("Tile {0} , {1}", q, r);
                //tiles[axialCoord] = tile;
            }
        }
    }
    private void CreateGems()
    {
        float scaleY = hexPrefab.transform.localScale.y;

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
                Vector3 worldPos = AxialToWorld(axialCoord, scaleY);

                int random = Random.Range(0, gemPrefabs.Length);
                GameObject gem = Instantiate(gemPrefabs[random],newGem.transform);

                gem.transform.position = worldPos + new Vector3(0, 0, -0.25f);
                gem.name = string.Format(gem.tag + " " + " {0} , {1}" , q, r);

                int excludeR1 = Mathf.Max(-3, -q - 3);
                int excludeR2 = Mathf.Min(3, -q + 3);
                // 따로 등록
                if (!(q >= -3 && q <= 3 && r >= excludeR1 && r < excludeR2))
                {
                    gemFactoryTiles.Add(axialCoord);
                    newGems[axialCoord] = gem;
                    newGemsPos[gem] = axialCoord;
                    //gems[axialCoord] = gem;
                    //gemPositions[gem] = axialCoord;
                }
                else
                {
                    gems[axialCoord] = gem;
                    gemPositions[gem] = axialCoord;
                }

                //GameObject tile = tiles[axialCoord];                                                       
                bool checkForMatchesBegin = BoardManager.instance.CheckForMatches(axialCoord, out List<GameObject> destroyGems);
        
                // false이면
                if (!checkForMatchesBegin)
                {
                    r--;
                    Destroy(gem);
                    gems.Remove(axialCoord);
                    gemPositions.Remove(gem);
                }
                else
                {
                    for(int i = 0; i < gemFactoryTiles.Count; i++)
                    {
                        //if (gemFactoryTiles[i].til)
                    }
                }

            }
        }
        
    }

    


    public Vector3 AxialToWorld(Vector2Int axialCoord, float width)
    {
        float x = axialCoord.x * width * 0.75f;
        float y = (axialCoord.y + (axialCoord.x / 2.0f)) * Mathf.Sqrt(3) * width * 0.5f;
        float z = -0.25f;
        // 2D 좌표계와 유니티 좌표계의 차이 때문에 y를 뒤집어준다.
        return new Vector3(x, -y, -z);
    }
    public Vector2Int WorldToAxial(Vector3 worldPos, float width)
    {
        float x = worldPos.x / (width * 0.75f);
        float y = -worldPos.y / (Mathf.Sqrt(3) * width * 0.5f) - (x / 2.0f);

        int q = Mathf.RoundToInt(x);
        int r = Mathf.RoundToInt(y);

        return new Vector2Int(q, r);
    }
}