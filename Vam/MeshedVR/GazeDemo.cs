using UnityEngine;
using VAM_ScriptEngine;

namespace MacGruber
{
    class GazeDemo : Script
    {
        private GazeController gazeController;

        public override void OnPostLoad()
        {
            gazeController = new GazeController(this, "Person");
            gazeController.SetReference("Person", "hipControl");
            gazeController.SetLookAtPlayer(-0.10f*Vector3.up); // applying target offset, 10cm down from player center-eye
        }

        public override void OnFixedUpdate()
        {
            gazeController.OnFixedUpdate();
        }
    }
}
