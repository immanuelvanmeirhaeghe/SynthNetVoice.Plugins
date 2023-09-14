using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using VAM_ScriptEngine;


namespace VAMDeluxe
{

    public class ExpressionManager
    {
        private string facialFile = "./SharedAnimations/facial.json";

        public float nextPlayTime = 0.0f;

        Shuffler<Expression> expressions;
        Shuffler<Expression> climaxes;
        Expression lastPlayedExpression;

        Dictionary<string, Facial> facial = new Dictionary<string, Facial>();

        public Expression breathingIdle;
        public Expression breathingActive;
        public Expression panting;

        string[] facialKeys;

        SexDriver actor;

        Dropdown personalityDropdown = null;
        string[] localExpressionFiles;

        public ExpressionManager(SexDriver actor)
        {
            this.actor = actor;

            LoadFacial();


            nextPlayTime = Time.fixedTime + 1.0f;

            LoadSharedExpressions();

            Debug.Log("-------------");

            GameObject personalityGO = GameObject.Find("Personality Dropdown");

            if (personalityGO != null)
            {
                personalityDropdown = personalityGO.GetComponent<Dropdown>();
                personalityDropdown.ClearOptions();
            }

            localExpressionFiles = Directory.GetFiles(SexDriver.SAVEPATH + "/personalities/", "expression_*.json");
            List<string> nameOptions = new List<string>();
            foreach(string filePath in localExpressionFiles)
            {
                if (File.Exists(filePath))
                {
                    string name = FirstLetterToUpper(Path.GetFileNameWithoutExtension(filePath).Replace("expression_",""));
                    nameOptions.Add(name);
                    Debug.Log(name);
                }
            }

            if (personalityDropdown != null)
            {
                personalityDropdown.AddOptions(nameOptions);
                personalityDropdown.transform.localPosition.Set(0, 0, -0.01f);
            }

            personalityDropdown.onValueChanged.RemoveAllListeners();
            personalityDropdown.onValueChanged.AddListener(ChangePersonality);

            //  avoid the first one...
            if(personalityDropdown.value == 0)
            {
                personalityDropdown.value = 2;
            }

            if (localExpressionFiles.Length > 0) {
                if (personalityDropdown != null)
                {
                    LoadExpressions(localExpressionFiles[personalityDropdown.value]);
                }
                else
                {
                    LoadExpressions(localExpressionFiles[0]);
                }
            }
        }

        ~ExpressionManager()
        {
            Debug.Log("deconstructed");
        }

        private void ChangePersonality(int value)
        {
            Debug.Log("changing to " + value);
            LoadExpressions(localExpressionFiles[value]);
        }

        private void LoadFacial()
        {
            string facialPath = SexDriver.SAVEPATH + facialFile;

            StreamReader jsonFile = new StreamReader(facialPath);
            string jsonString = jsonFile.ReadToEnd();
            jsonFile.Close();
            var loadJson = JSON.Parse(jsonString);

            foreach (KeyValuePair<string, JSONNode> kvp in loadJson.AsObject)
            {
                string key = kvp.Key;
                facial.Add(key, new Facial(loadJson[key]));
            }

            facialKeys = facial.Keys.ToList().ToArray();
        }

        private void LoadExpressions(string localExpressionPath)
        {
            StreamReader jsonFile = new StreamReader(localExpressionPath);
            string jsonString = jsonFile.ReadToEnd();
            jsonFile.Close();
            var loadJson = JSON.Parse(jsonString);

            List<Expression> loadedExpression = new List<Expression>();
            var expressions = loadJson["expressions"];
            for (int i = 0; i < expressions.Count; i++)
            {
                Expression exp = new Expression(expressions[i], facial);
                loadedExpression.Add(exp);
            }
            this.expressions = new Shuffler<Expression>(loadedExpression.ToArray());


            List<Expression> loadedClimaxes = new List<Expression>();
            var climaxes = loadJson["climaxes"];
            for (int i = 0; i < climaxes.Count; i++)
            {
                Expression exp = new Expression(climaxes[i], facial);
                loadedClimaxes.Add(exp);
            }
            this.climaxes = new Shuffler<Expression>(loadedClimaxes.ToArray());





            lastPlayedExpression = null;
        }

        private void LoadSharedExpressions()
        {
            string expressionPath = SexDriver.SAVEPATH + "./SharedAnimations/shared_expressions.json";

            StreamReader jsonFile = new StreamReader(expressionPath);
            string jsonString = jsonFile.ReadToEnd();
            jsonFile.Close();
            var sharedJson = JSON.Parse(jsonString);

            panting = new Expression(sharedJson["panting"], facial);
            breathingIdle = new Expression(sharedJson["breathing idle"], facial);
            breathingActive = new Expression(sharedJson["breathing active"], facial);
        }

        public bool CanPlay()
        {
            return Time.fixedTime >= nextPlayTime;
        }

        public void StartBreathing(BreatheAction breath, float duration)
        {
            if(breath.breath > 0.6f)
            {
                panting.TriggerFacial(duration);
                actor.headAudioSource.Trigger(panting.audioClip);
            }
            else
            if(breath.breath > 0.3f)
            {
                breathingActive.TriggerFacial(duration);
                actor.headAudioSource.Trigger(breathingActive.audioClip);
            }
            else
            {
                breathingIdle.TriggerFacial(duration);
                actor.headAudioSource.Trigger(breathingIdle.audioClip);
            }

            foreach (string key in facialKeys)
            {
                Facial f = facial[key];
                f.shouldFade = true;
            }

        }

        public void StartPanting(float duration)
        {
            panting.TriggerFacial(duration);
            actor.headAudioSource.Trigger(panting.audioClip);

            foreach (string key in facialKeys)
            {
                Facial f = facial[key];
                f.shouldFade = true;
            }
        }

        public void SelectExpression(float intensity, float maxIntensity, float desiredIntensity)
        {
            if (expressions == null)
            {
                return;
            }

            Expression nextExpression = expressions.Next();
            if (nextExpression!=null && nextExpression.ShouldPlay(intensity, maxIntensity, desiredIntensity) && nextExpression != lastPlayedExpression)
            {

                foreach (string key in facialKeys)
                {
                    Facial f = facial[key];
                    f.shouldFade = false;
                }

                //panting.Stop();
                //breathingIdle.Stop();
                //breathingActive.Stop();

                lastPlayedExpression = nextExpression;
                nextExpression.TriggerFacial(0);

                float nextDuration = nextExpression.audioClip.sourceClip.length;
                actor.headAudioSource.Trigger(nextExpression.audioClip);

                actor.jawAdjust.driveXRotationFromAudioSource = !nextExpression.keepMouthClosed;
                actor.jawAdjust.SyncJointPositionXDrive();

                nextPlayTime = Time.fixedTime + Random.Range(nextDuration, nextDuration + 1.0f);
            }

            actor.jawAdjust.driveXRotationFromAudioSourceMultiplier = 115.0f;
            actor.jawAdjust.driveXRotationFromAudioSourceAdditionalAngle = 0.0f;
            actor.jawAdjust.driveXRotationFromAudioSourceMaxAngle = -5.0f;
        }

        public void SelectClimax(float duration)
        {
            foreach (string key in facialKeys)
            {
                Facial f = facial[key];
                f.shouldFade = false;
            }

            Expression climax = climaxes.Next();
            climax.TriggerFacial(duration);
            actor.headAudioSource.Trigger(climax.audioClip);
            nextPlayTime = Time.fixedTime + Mathf.Max(duration, climax.audioClip.sourceClip.length) + Random.Range(2.0f, 4.0f);
        }

        public void Update(Activity activity)
        {
            foreach (string key in facialKeys)
            {
                Facial f = facial[key];
                f.Update();
            }
        }

        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }

}
