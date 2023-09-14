using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VAM_ScriptEngine;

namespace VAMDeluxe
{
    public class BreatheAction
    {

        public float breath = 0.0f;

        private float breatheCylce = -0.1f;

        private float breathLerp = 0.08f;

        private OutTriggerFloat breathe;
        private DAZMorph breatheMorph;

        private FreeControllerV3 chest;
        private FreeControllerV3 head;

        public BreatheAction(Script script)
        {

            JSONStorable js = Utils.GetAtom("Person").GetStorableByID("geometry");
            if (js != null)
            {
                DAZCharacterSelector dcs = js as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;
                if (morphUI != null)
                {
                    breatheMorph = morphUI.GetMorphByDisplayName("Breath1");
                }
            }

            chest = Utils.GetAtom("Person").GetStorableByID("chestControl") as FreeControllerV3;
            if (chest != null)
            {
                chest.jointRotationDriveSpring = 60.0f;
                chest.jointRotationDriveDamper = 0.5f;
            }

            head = Utils.GetAtom("Person").GetStorableByID("headControl") as FreeControllerV3;
            if (head != null)
            {
                head.RBHoldPositionSpring = 4000.0f;
            }

            //breathe = script.RegisterOutFloat("Person", "geometry", "Breath1");
        }

        public void Init()
        {

        }

        public void Update(Activity intensity)
        {
            float iv = intensity.intensity / intensity.maxIntensity;

            breath += (iv - breath) * Time.deltaTime * breathLerp;
            breath = Mathf.Clamp01(breath);

            breatheCylce += Time.deltaTime * Mathf.Clamp((1.27f + breath * 7.0f),0.0f,20.0f);
            if (breatheMorph != null)
            {
                float power = Mathf.Clamp(breath, 0.5f, 0.7f);
                float cycle = Mathf.Sin(breatheCylce) * power;
                breatheMorph.morphValue = cycle;
            }

            if (chest != null)
            {
                float power = Remap(breath, 0.0f, 1.0f, -10, 10);
                float cycle = Mathf.Sin(breatheCylce * 2.0f + 0.4f) * - power;
                chest.jointRotationDriveXTarget = cycle;
                chest.jointRotationDriveXTargetSlider.value = cycle;
            }

            if (head != null)
            {
                //head.RBHoldPositionSpring = 2000;
                //head.RBHoldRotationSpring = 120;
                head.RBHoldPositionSpring = Remap(breath,0.3f,0.6f,4000,2000);
                head.RBHoldRotationSpring = Remap(breath, 0.3f, 0.6f, 120, 50);
            }
        }

        private float Remap(float x, float x1, float x2, float y1, float y2)
        {
            var m = (y2 - y1) / (x2 - x1);
            var c = y1 - m * x1; // point of interest: c is also equal to y2 - m * x2, though float math might lead to slightly different results.

            return m * x + c;
        }
    }
}
