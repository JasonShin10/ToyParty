using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject hexPrefab;
    [SerializeField] private GameObject[] gemPrefabs;
    [SerializeField] private GameObject Gem;
    private Dictionary<Vector2Int, GameObject> gems = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, Vector2Int> gemPositions = new Dictionary<GameObject, Vector2Int>();
    
    private void Start()
    {
        BoardManager.instance.RegisterTileScript(this);
        CreateTiles();
        CreateJams();
        CreateRefillGem();
    }

    public Dictionary<Vector2Int, GameObject> Gems { get { return gems; } }
    public Dictionary<GameObject, Vector2Int> GemPositions { get { return gemPositions; } }
    public float width;

    private void CreateTiles()
    {
        // 6������ �ʺ�,��(width)
        width = hexPrefab.transform.localScale.y;
        // q�� �¿����
        for (int q = -3; q <= 3; q++)
        {
            // ���� ���� ���̴¾�
            // q = -3�϶� r1 = 0
            // r2 = 3
            // 3�� ����
            // q = -2 �϶� r1 = -1
            // r2 = 3
            // 4�� ����
            int r1 = Mathf.Max(-3, -q - 3);
            int r2 = Mathf.Min(3, -q + 3);

            for (int r = r1; r < r2; r++)
            {
                // �� ��ǥ�� �����Ѵ�.
                Vector2Int axialCoord = new Vector2Int(q, r);
                // �� ��ǥ�� ���� ��ǥ�� ��ȯ�Ѵ�.
                Vector3 worldPos = AxialToWorld(axialCoord, width);
                GameObject tile = Instantiate(hexPrefab,transform);
                tile.transform.position = worldPos;
                tile.name = string.Format("Tile {0} , {1}", q, r);
                //tiles[axialCoord] = tile;
            }
        }
    }
    private void CreateJams()
    {
        float scaleY = hexPrefab.transform.localScale.y;

        for (int q = -3; q <= 3; q++)
        {
            int r1 = Mathf.Max(-3, -q - 3);
            int r2 = Mathf.Min(3, -q + 3);

            for (int r = r1; r < r2; r++)
            {
                Vector2Int axialCoord = new Vector2Int(q, r);
                Vector3 worldPos = AxialToWorld(axialCoord, scaleY);
                int random = Random.Range(0, gemPrefabs.Length);
                GameObject gem = Instantiate(gemPrefabs[random],Gem.transform);
                gem.transform.position = worldPos + new Vector3(0, 0, -0.25f);
                gem.name = string.Format(gem.tag + " " + " {0} , {1}" , q, r);
                gems[axialCoord] = gem;
                gemPositions[gem] = axialCoord;
                // ��ġ�� �� �뿡�� Ÿ���� �Ǵ��ϴ� �������� ���
                //GameObject tile = tiles[axialCoord];                                                       
                bool checkForMatchesBegin = BoardManager.instance.CheckForMatches(axialCoord, out var destroyedPositions);
                
                // false�̸�
                if(!checkForMatchesBegin)
                {
                    
                    r--;
                    Destroy(gem);
                    gems.Remove(axialCoord);
                    gemPositions.Remove(gem);
                }
                // Ÿ�Ͽ��� ��� ray�� ������ �˻��Ҷ�
                // ���������� �ƴ��� �˻����ִ� �Լ� �ߵ�
            }
        }
        
    }

    private void CreateRefillGem()
    {
        float scaleY = hexPrefab.transform.localScale.y;
        Vector2Int axialCoord = new Vector2Int(0, -4);
        Vector3 worldPos = AxialToWorld(axialCoord, scaleY);
        int random = Random.Range(0, gemPrefabs.Length);
        GameObject gem = Instantiate(gemPrefabs[random],Gem.transform);
        gem.transform.position = worldPos + new Vector3(0, 0, -0.25f);
        gem.name = string.Format(gem.tag + " " + " {0} , {1}" , 0, -4);
        gems[axialCoord] = gem;
    }

    private Vector3 AxialToWorld(Vector2Int axialCoord, float width)
    {
        float x = axialCoord.x * width * 0.75f;
        float y = (axialCoord.y + (axialCoord.x / 2.0f)) * Mathf.Sqrt(3) * width * 0.5f;
        // 2D ��ǥ��� ����Ƽ ��ǥ���� ���� ������ y�� �������ش�.
        return new Vector3(x, -y, 0);
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