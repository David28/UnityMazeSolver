using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finder : MonoBehaviour
{
    public int timeScale = 1;

    public GameObject hPrefab;

    public GameObject vPrefab;

    public GameObject backPrefab;

    public GameObject stepPrefab;

    public GameObject blindPrefab;

    public GameObject endPrefab;
    GameObject[,] tiles;
    CellInfo[,] t;

    GameObject[,] caves;
    GameObject[,] verticalWalls;

    GameObject[,] horizontalWalls;
    float timer = 1;

    GameObject startT;
    GameObject endT;
    Vector2Int cur;
    Queue<Vector2Int> q;
    int visited;

    [SerializeField] private SizeData size;

    bool mazeDone;
    int iter;
    bool solved;

    Vector2Int here;

    Vector2Int end;
    Stack<Vector2Int> toClean;
    Stack<Vector2Int> tree = new Stack<Vector2Int>();

    Vector2Int walkDir = Vector2Int.zero;
    public void Fast() { timeScale = 3; skip = false; }

    public void Normal() { timeScale = 1; skip = false; }

    public void Stop() { timeScale = 0; skip = false; }
    GameObject head;

    GameObject backGround;

    bool skip = false;
    public void Skip()
    {
        skip = true;
        timeScale = 1;
    }

    void Start()
    {

        // make a 20 by 20 grid of objects
        tiles = new GameObject[size.width, size.height];
        caves = new GameObject[size.width, size.height];
        horizontalWalls = new GameObject[size.width, size.height + 1];
        verticalWalls = new GameObject[size.width + 1, size.height];
        t = new CellInfo[size.width, size.height];
        int x = size.width;
        while (x > 0)
        {
            x--;
            int y = size.height;
            while (y > 0)
            {
                y--;
                horizontalWalls[x, y] = Instantiate(hPrefab, new Vector2(x, y - 0.5f), Quaternion.identity);
                verticalWalls[x, y] = Instantiate(vPrefab, new Vector2(x - 0.5f, y), Quaternion.identity);
                caves[x, y] = Instantiate(blindPrefab, new Vector2(x, y), Quaternion.identity);
                t[x, y] = new CellInfo();
            }
            horizontalWalls[x, size.height] = Instantiate(hPrefab, new Vector2(x, size.height - 0.5f), Quaternion.identity);
        }
        for (int i = size.height - 1; i >= 0; i--)
        {
            verticalWalls[size.width, i] = Instantiate(vPrefab, new Vector2(size.width - 0.5f, i), Quaternion.identity);
        }
        Camera.main.transform.position = new Vector3(size.width / 2f, size.height / 2f, -10);
        Camera.main.orthographicSize = Mathf.Max(size.width, size.height) / 2f + 1;


        Vector2Int start = new Vector2Int(Random.Range(0, 2) * (size.width - 1), Random.Range(0, 2) * (size.height - 1));
        backGround = Instantiate(backPrefab, new Vector3(size.width / 2 - 0.5f, size.height / 2 - 0.5f), Quaternion.identity);
        backGround.transform.localScale += new Vector3(size.width - 1, size.height - 1);
        q = new Queue<Vector2Int>();
        cur = start;
        here = Vector2Int.zero;
        solved = false;
        toClean = new Stack<Vector2Int>();
        head = Instantiate(endPrefab, new Vector3(start.x, start.y), Quaternion.identity);
        head.GetComponent<SpriteRenderer>().color = Color.cyan;

    }
    Vector2Int RandomSpot(Vector2Int except)
    {
        int b = Random.Range(0, 2);
        if (b == 0)
        {
            int lr = Random.Range(0, 2) * (size.width - 1);
            int rh = Random.Range(0, size.height);
            if (lr == 0)
                Destroy(verticalWalls[lr, rh]);
            else
                Destroy(verticalWalls[lr + 1, rh]);
            return (new Vector2Int(lr, rh) == except) ? RandomSpot(except) : new Vector2Int(lr, rh);
        }
        else
        {
            int ud = Random.Range(0, 2) * (size.height - 1);
            int rw = Random.Range(0, size.height);
            if (ud == 0)
                Destroy(horizontalWalls[rw, ud]);
            else
                Destroy(horizontalWalls[rw, ud + 1]);
            return (new Vector2Int(rw, ud) == except) ? RandomSpot(here) : new Vector2Int(rw, ud);
        }
    }
    void Update()
    {
        timer -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Space))
        {
            if (!mazeDone)
            {
                if (skip)
                    while (!mazeDone)
                        CreateMaze(cur);
                else
                    for (int i = 0; i < timeScale; i++)
                        CreateMaze(cur);

            }
            else if (!solved)
            {
                if (skip)
                    while (!solved)
                        Algo();
                else
                    for (int i = 0; i < timeScale; i++)
                        Algo();

            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    bool cleaning;
    void Clean()
    {
        if (here != tree.Peek())
        {
            tiles[here.x, here.y].GetComponent<SpriteRenderer>().enabled = false;
            here = toClean.Pop();
            this.GetComponent<LineRenderer>().positionCount--;
        }
        else
        {
            tree.Pop();
            cleaning = false;
        }


    }
    void Algo()
    {
        if (tiles[here.x, here.y] == null)
            tiles[here.x, here.y] = Instantiate(stepPrefab, new Vector3(here.x, here.y), Quaternion.identity);
        if (!cleaning)
        {
            this.GetComponent<LineRenderer>().positionCount++;
            this.GetComponent<LineRenderer>().SetPosition(this.GetComponent<LineRenderer>().positionCount - 1, new Vector3(here.x, here.y));
            toClean.Push(here);
        }


        if (here == end)
        {
            solved = true;
            return;
        }
        Vector2Int nextDir = ChooseDir();
        if (!cleaning && nextDir != Vector2Int.zero)
        {

            if (CanMakeDecision())
            {
                tree.Push(here);
            }
            tiles[here.x, here.y].gameObject.transform.rotation = Quaternion.AngleAxis(Vector2.Angle(Vector2.up, nextDir), Vector3.back);
            walkDir = nextDir;
            here = here + walkDir;


        }
        else
        {
            cleaning = true;
            Clean();
        }
    }
    bool CanMakeDecision()
    {
        int possibleDir = ((Stop(here, Vector2Int.up)) ? 0 : 1) +
        ((Stop(here, Vector2Int.down)) ? 0 : 1) +
        ((Stop(here, Vector2Int.left)) ? 0 : 1) +
        ((Stop(here, Vector2Int.right)) ? 0 : 1);
        return possibleDir > 1;
    }
    Vector2Int ChooseDir()
    {
        List<Vector2Int> directions = new List<Vector2Int>();
        directions.Add(Vector2Int.up);
        directions.Add(Vector2Int.down);
        directions.Add(Vector2Int.left);
        directions.Add(Vector2Int.right);
        Vector2Int x = Vector2Int.zero;
        while (x == Vector2Int.zero && directions.Count > 0)
        {
            int j = Random.Range(0, directions.Count);
            Vector2Int test = directions[j];

            if (!Stop(here, test))
                x = test;
            else
                directions.RemoveAt(j);


        }
        return x;
    }

    bool Stop(Vector2Int i, Vector2Int dir)
    {
        Vector2Int p = i + dir;
        Vector2Int next = i + dir;
        if (dir.x == 0)
        {
            if (dir.y < 0)
                p = i;
            return !In(next) || horizontalWalls[p.x, p.y] != null || tiles[next.x, next.y] != null;
        }

        else
        {
            if (dir.x < 0)
                p = i;
            return !In(next) || verticalWalls[p.x, p.y] != null || tiles[next.x, next.y] != null;
        }
    }
    /*
    CODE RELATED TO LABIRINT GENERATION
    Algorithm works by moving in a random directions until it no longer can, after wich it returns 
    to the last non blocked cell. And continues until All cells are visited.
    */
    public class CellInfo
    {
        private bool _visited;
        public bool visited()
        {
            return _visited;
        }

        public void visit()
        {
            _visited = true;
        }
    }

    void RedoPointWall(GameObject x)
    {
        if(x != null)
        {
                    Vector3 j = x.transform.position;
        Vector2Int i = new Vector2Int((int)j.x, (int)j.y);
        if (i.x ==size.width- 1)
            verticalWalls[i.x + 1, i.y] = Instantiate(vPrefab, new Vector3(i.x + 0.5f, i.y), Quaternion.identity);
        else if (i.x == 0)
            verticalWalls[i.x, i.y] = Instantiate(vPrefab, new Vector3(i.x - 0.5f, i.y), Quaternion.identity);
        else if (i.y == size.height - 1)
            horizontalWalls[i.x, i.y + 1] = Instantiate(hPrefab, new Vector3(i.x, i.y + 0.5f), Quaternion.identity);
        else
            horizontalWalls[i.x, i.y] = Instantiate(hPrefab, new Vector3(i.x, i.y - 0.5f), Quaternion.identity);
        Destroy(x);
        }

    }
    public void InitiateSolving()
    {


            for (int i = 0; i < size.width; i++)
            {
                for (int j = 0; j < size.height; j++)
                {
                    if (tiles[i, j] != null)
                        Destroy(tiles[i, j]);
                }
            }
            tiles = new GameObject[size.width, size.height];
        
        RedoPointWall(startT);
        RedoPointWall(endT);

        Debug.Log("HEY");
            solved = false;

            toClean = new Stack<Vector2Int>();
            tree = new Stack<Vector2Int>();
            walkDir = Vector2Int.zero;
        here = RandomSpot(new Vector2Int(-1, -1));
        startT = Instantiate(endPrefab, new Vector3(here.x, here.y), Quaternion.identity);
        startT.GetComponent<SpriteRenderer>().material.color = Color.cyan;
        end = RandomSpot(here);
        endT = Instantiate(endPrefab, new Vector3(end.x, end.y), Quaternion.identity);
        endT.GetComponent<Renderer>().material.color = Color.green;
        this.GetComponent<LineRenderer>().positionCount = 0;

        backGround.GetComponent<SpriteRenderer>().color = Color.white;

    }
    void CreateMaze(Vector2Int i)
    {
        iter = 0;
        Visit(i);
        if (visited == size.height * size.width)
        {
            mazeDone = true;
            Destroy(head);
            InitiateSolving();
            return;
        }
        q.Enqueue(i);

        while (Blocked(i) && q.Count > 0)
        {
            i = q.Dequeue();
        }

        Vector2Int dir = RandomDirection(i);
        Vector2Int next = Move(i, dir);
        iter++;

        if (q.Count > 0)
        {
            i = next;
        }

        cur = i;
        head.transform.position = new Vector3(cur.x, cur.y);





    }

    Vector2Int Move(Vector2Int i, Vector2Int dir)
    {
        if (dir == Vector2Int.zero)
            return i;
        Vector2Int next = i + dir;
        Vector2Int res = i;
        if (In(next) && !Visited(next))
        {

            BreakWall(i, next, dir);
            res = next;
        }
        return res;

    }

    /// <summary>
    /// Visits a cell
    /// </summary>
    /// <param name="i"></param>
    void Visit(Vector2Int i)
    {
        Destroy(caves[i.x, i.y]);
        visited++;
        if (In(i))
        {
            t[i.x, i.y].visit();
        }
    }
    void BreakWall(Vector2Int i, Vector2Int j, Vector2Int dir)
    {
        Vector2Int p = j;
        if (dir.x == 0)
        {
            if (dir.y < 0)
                p = i;
            Destroy(horizontalWalls[p.x, p.y]);
        }

        else
        {
            if (dir.x < 0)
                p = i;
            Destroy(verticalWalls[p.x, p.y]);
        }
    }

    bool In(Vector2Int i)
    {
        return i.x >= 0 && i.x < size.width && i.y >= 0 && i.y < size.height;
    }
    bool Blocked(Vector2Int i)
    {
        bool res = true;
        if ((In(i + Vector2Int.up) && !Visited(i + Vector2Int.up))
            || (In(i + Vector2Int.down) && !Visited(i + Vector2Int.down))
            || (In(i + Vector2Int.left) && !Visited(i + Vector2Int.left))
            || (In(i + Vector2Int.right) && !Visited(i + Vector2Int.right)))
        {
            res = false;
        }

        return res;
    }

    bool Visited(Vector2Int i) => t[i.x, i.y].visited();
    Vector2Int RandomDirection(Vector2Int i)
    {
        List<Vector2Int> directions = new List<Vector2Int>();
        directions.Add(Vector2Int.up);
        directions.Add(Vector2Int.down);
        directions.Add(Vector2Int.left);
        directions.Add(Vector2Int.right);
        Vector2Int x = Vector2Int.zero;
        while (x == Vector2Int.zero && directions.Count > 0)
        {
            int j = Random.Range(0, directions.Count);
            Vector2Int test = directions[j];
            if (In(i + test))
            {
                if (!Visited(i + test))
                    x = test;
            }
            else
            {
                directions.RemoveAt(j);
            }

        }
        return x;
    }
}
