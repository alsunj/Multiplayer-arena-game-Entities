using Unity.Entities;
using UnityEngine;

public class TeamAuthoring : MonoBehaviour
{
    public TeamType TeamType;

    public class TeamBaker : Baker<TeamAuthoring>
    {
        public override void Bake(TeamAuthoring authoring)
        {
            //transform.none?
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new TeamTypes { Value = authoring.TeamType });
        }
    }
}