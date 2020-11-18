using Unity.Entities;

//First system using a normal Component with an Entity field
[GenerateAuthoringComponent]
public struct PrefabEntityComponent : IComponentData
{
    public Entity prefabEntity;
}
