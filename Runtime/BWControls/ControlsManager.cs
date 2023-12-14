using UnityEngine;

namespace Buildwise.Core
{
    /// <summary>
    /// This class is used to manage the controls of the game. This is where the switch is done between the different Action Maps.
    /// </summary>
    public class ControlsManager : MonoBehaviour
    {
        public static BWControls BWControls;
        // Start is called before the first frame update
        void Awake()
        {
            BWControls = new BWControls();
            BWControls.InGame.Enable();
        }
    }
}