using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class SpriteAuthoring : MonoBehaviour
{
    public Sprite sprite;
    public Material material; // Materia³ z shaderem obs³uguj¹cym sprite'y
}

public class SpriteBaker : Baker<SpriteAuthoring>
{
    public override void Bake(SpriteAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        // Ustawienie tekstury w materiale
        Material materialInstance = new Material(authoring.material);
        materialInstance.mainTexture = authoring.sprite.texture; // Ustawienie Sprite jako tekstury

        // Tworzenie siatki quada
        Mesh quadMesh = CreateQuadMesh();

        // Tworzenie RenderMeshArray (potrzebne w Unity 6)
        var renderMeshArray = new RenderMeshArray(new Material[] { materialInstance }, new Mesh[] { quadMesh });

        // Dodanie komponentów renderowania
        AddComponent(entity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)); // Indeksy: materia³ = 0, mesh = 0
        AddComponent(entity, new RenderBounds { Value = new AABB { Extents = new float3(0.5f, 0.5f, 0) } });
        AddSharedComponentManaged(entity, renderMeshArray);
    }

    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0)
        };

        mesh.triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        return mesh;
    }
}
