using Unity.Entities;

public struct Character : IComponentData
{
    public bool isMove;
    public float startAnim;

    public int directionHead;
    public int directionBody;

    public Entity body;
    public Entity headParent;
    public Entity head;

    public Entity GetPart(BodyPart part)
    {
        switch(part)
        {
            case BodyPart.head:
                return head;
            case BodyPart.body:
                return body;
        }
        return Entity.Null;
    }
}