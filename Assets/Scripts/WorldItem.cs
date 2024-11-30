using System;
using System.Data;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class WorldItem : MonoBehaviour
{
    ItemStats itemStats;
    Timer timer;
    Timer timerTransform;
    Timer timerFollow;


    private Action actionTodo;
    private Vector2? target = null;


    public int itemChunkIndex;
    private void SetUp(Vector2 target)
    {
        GetComponent<Collider2D>().enabled = false;
       // position = target;
        transform.localScale = new Vector2(0, 0);
        timer = Timer.Create
        (() =>
        {
            float scaleX = Mathf.Lerp(transform.localScale.x, 1.5f, Time.deltaTime * 18f); ;
            float scaleY = Mathf.Lerp(transform.localScale.y, 1.5f, Time.deltaTime * 15f);
            transform.localScale = new Vector3(scaleX, scaleY);
            if (transform.localScale.y > 0.99f)
            {
                return true;
            }
            return false;
        },
        () =>
        {
            float scaleX = Mathf.Lerp(transform.localScale.x, 1f, Time.deltaTime * 25);
            float scaleY = Mathf.Lerp(transform.localScale.y, 1f, Time.deltaTime * 20);
            transform.localScale = new Vector3(scaleX, scaleY);
            if (transform.localScale.x <= 1.01f)
            {
                transform.localScale = new Vector2(1f, 1f);
                return true;
            }
            return false;
        }
        );

        if (Vector2.Distance(transform.position, target) > 0.02f)
        {
            timerTransform = Timer.Create
            (() =>
            {
                Vector2 pos = Vector2.Lerp(transform.position, target, Time.deltaTime * 10f);
                transform.position = pos;
                if (Vector2.Distance(transform.position, target) < 0.03f)
                {
                    this.GetComponent<Collider2D>().enabled = true;
                    return true;
                }
                return false;
            },
            () =>
            {
                if(actionTodo != null) SetNextTarget();
                return true;
            }
            );
        }
        else
            this.GetComponent<Collider2D>().enabled = true;
    }
    public void SetItem(ItemStats itemStats,Vector2 target,int itemChunkIndex)
    {
        this.itemStats = itemStats;
        this.itemChunkIndex = itemChunkIndex;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = ItemsAsset.instance.GetIcon(itemStats.itemID);
        SetUp(target);
    }

    public void AddStacks(ChunkItem stackTochunkItem, ChunkItem currentItem)
    {
        target = stackTochunkItem.worldItem.position;
        actionTodo = () =>
        {
            if (GridVisualization.instance.AddStacks(stackTochunkItem, itemStats))
                GridVisualization.instance.RemoveWorldItem(transform.position, itemChunkIndex);
            else
                currentItem.position = GridVisualization.instance.GetGridPosition(stackTochunkItem.worldItem.position);
        };

        if (timerTransform == null || timerTransform.IsEnd())
        {
            Timer.Create(5, () => { SetNextTarget(); return false; });
        }
    }

    public void Move(Vector2 newPos,ChunkItem item,int oldChunk,int newChunk)
    {
        target = newPos;
        actionTodo = () =>
        {
            GridVisualization.instance.map.chunks[oldChunk].RemoveItem(itemChunkIndex);
            item.position = GridVisualization.instance.GetGridPosition(newPos);
            itemChunkIndex = GridVisualization.instance.map.chunks[newChunk].AddItem(item);
            GridVisualization.instance.RemoveWorldItem(transform.position, itemChunkIndex);
        };

        if (timerTransform == null || timerTransform.IsEnd())
        {
            SetNextTarget();
        }
    }
    private void SetNextTarget()
    {
        timerTransform = Timer.Create
        (() =>
        {
            Vector2 pos = Vector2.Lerp(transform.position, (Vector2)target, Time.deltaTime * 8f);
            transform.position = pos;
            if (Vector2.Distance(transform.position, (Vector2)target) < 0.03f)
            {
                this.GetComponent<Collider2D>().enabled = true;
                return true;
            }
            return false;
        },
        () =>
        {
            actionTodo();
            return true;
        }
        );
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent != null && collision.transform.parent.CompareTag("Player") )
        {
            timerFollow = Timer.Create(() =>
            {
                Vector2 pos = Vector2.Lerp(transform.position, collision.transform.position, Time.deltaTime * 13f);
                transform.position = pos;
                if (Vector2.Distance(transform.position, collision.transform.position) < 0.16f)
                {
                    AddItem();
                    return true;
                }
                return false;
            }, () =>
            {
                return false;
            });
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.parent != null && collision.transform.parent.CompareTag("Player"))
        {
            if (timerFollow != null) timerFollow.Cancel();
        }
    }
    private void AddItem()
    {
        if (EquipmentManager.instance.AddNewItem(itemStats))
        {
            Sounds.instance.Click();
            GridVisualization.instance.RemoveWorldItem(transform.position, itemChunkIndex);
        }
    }

    public void ClearTimers()
    {
        if (timerFollow != null) timerFollow.Cancel();
        if (timer != null) timer.Cancel();
        if (timerTransform != null) timerTransform.Cancel();
    }

    private void OnDestroy()
    {
        ClearTimers();
    }
}
