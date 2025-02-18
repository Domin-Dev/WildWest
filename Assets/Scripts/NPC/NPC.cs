using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{

    Rigidbody2D rigidbody2D;
    [SerializeField] Vector2 target; 
    [SerializeField] Vector2 newX;
    [SerializeField] NPCSpriteController characterSpriteController;

    public List<GridTile> path;
    public GridTile gridTile;
    
    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        characterSpriteController = GetComponent<NPCSpriteController>();
        NewPath(10,0);
    }

    private void FixedUpdate()
    {
       if (path != null) Follow();
    }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector2 x = GridVisualization.instance.GetGridPosition(MyTools.GetMouseWorldPosition());
        //    NewPath((int)x.x, (int)x.y);
        //}
    }
    public void Follow()
    {
        Vector2 vector2 = target - (Vector2)transform.position;
        rigidbody2D.velocity = vector2.normalized * 0.4f;
        characterSpriteController.UpdateSprite(vector2.normalized, vector2.normalized);
        if (Vector2.Distance(transform.position, target) <= 0.01)
        {
            path.RemoveAt(0);
            if (path.Count > 0)
            {
                NextTarget();
            }
            else
            {
                rigidbody2D.velocity = Vector2.zero;
                path = null;
                Debug.Log("end");
                NewPath(Random.Range(0, 20), Random.Range(0, 20));
            }
        }
       
    }

    public void NewPath(int x,int y)
    {
        Vector2 xy = GridVisualization.instance.GetGridPosition(transform.position);
        if (GridVisualization.instance.GetTileByGridPosition(xy).isWalkable)
        {
            gridTile = GridVisualization.instance.GetTileByGridPosition(xy);
            path = GridVisualization.instance.pathfinding.FindPath((int)xy.x, (int)xy.y, x, y);
            if (path != null && path.Count > 0)
            {
                NextTarget();
                return;
            }
        }
        Debug.Log(x + "  " + y + " " + gameObject.name);
        NewPath(Random.Range(0, 20), Random.Range(0, 20));
    }

    private void NextTarget()
    {
        target = GridVisualization.instance.GetWorldPosition(path[0].x, path[0].y) + new Vector2(0,0.1f);
    }
}
