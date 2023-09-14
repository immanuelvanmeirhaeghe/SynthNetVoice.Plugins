using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using VAM_ScriptEngine;

namespace VAMDeluxe
{
    public class Expression
    {
        public NamedAudioClip audioClip;
        public float minIntensity = 0.0f;
        public float maxIntensity = 1.0f;
        public bool keepMouthClosed = false;

        Dictionary<string, Facial> facialLookup;

        List<string> facialList = new List<string>();

        public Expression(JSONNode expressionData, Dictionary<string, Facial> facialDict)
        {
            audioClip = LoadAudio(expressionData["audio"]);

            if (expressionData["minIntensity"] != null)
            {
                minIntensity = float.Parse(expressionData["minIntensity"]);
            }

            if (expressionData["maxIntensity"] != null)
            {
                maxIntensity = float.Parse(expressionData["maxIntensity"]);
            }

            if (expressionData["mouthClosed"] != null)
            {
                keepMouthClosed = expressionData["mouthClosed"].AsBool;
            }

            var facialListNode = expressionData["facial"];
            for (int i=0; i< facialListNode.Count; i++)
            {
                facialList.Add(facialListNode[i]);
            }

            facialLookup = facialDict;
        }

        public bool ShouldPlay(float intensity, float maxIntensity, float desiredIntensity)
        {
            float iv = (intensity + (desiredIntensity - intensity) * 0.5f) / maxIntensity;
            return (iv >= this.minIntensity && iv <= this.maxIntensity);
        }

        public void TriggerFacial(float holdDuration)
        {
            foreach(string facialKey in facialList)
            {
                Facial facial;
                facialLookup.TryGetValue(facialKey, out facial);
                if (facial!=null)
                {
                    facial.Trigger(holdDuration);
                }
            }
        }

        public void Stop()
        {
            foreach (string facialKey in facialList)
            {
                Facial facial;
                facialLookup.TryGetValue(facialKey, out facial);
                if (facial != null)
                {
                    facial.Stop();
                }
            }
        }

        public static NamedAudioClip LoadAudio(string filePath)
        {
            string audioPath = SexDriver.SAVEPATH + filePath;
            URLAudioClip clip = URLAudioClipManager.singleton.QueueClip(audioPath, filePath);
            return Utils.GetAudioClip(audioPath);
        }
    }
}
