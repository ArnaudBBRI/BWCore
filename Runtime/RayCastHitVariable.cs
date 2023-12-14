using UnityEngine;

namespace Buildwise.Core
{
    [CreateAssetMenu(menuName = "Buildwise/Variables/RayCastHit", fileName = "RayCastHitVariable")]
    public class RayCastHitVariable : ScriptableObject
    {
        public RaycastHit Value;
    }
}