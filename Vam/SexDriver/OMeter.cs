using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VAM_ScriptEngine;

namespace VAMDeluxe
{
    public class OMeter
    {
        Activity activity;
        private Image meter;
        public OMeter(Activity activity)
        {
            GameObject OMeterGO = GameObject.Find("Orgasm Meter");
            if (OMeterGO != null)
            {
                meter = OMeterGO.GetComponent<Image>();
            }
            this.activity = activity;
        }

        public void Update()
        {
            if (meter != null)
            {
                meter.fillAmount = this.activity.intensity;
            }
        }
    }
}
