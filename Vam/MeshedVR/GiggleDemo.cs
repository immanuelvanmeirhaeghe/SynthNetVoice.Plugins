using UnityEngine;
using VAM_ScriptEngine;

namespace MacGruber
{
    class GiggleDemo : Script
    {
        private GazeController gazeController;
        private OutTriggerAudio personHeadAudio;
        private Shuffler<NamedAudioClip> clipsGiggle;
        private float nextClock = 0.0f;
        private float previousClock = float.MinValue;


        public override void OnPostLoad()
        {
            personHeadAudio = RegisterOutAudio("Person", "HeadAudioSource", "PlayNow");

            NamedAudioClip[] clips = Utils.GetAudioClips(new string[] {
                "./Assets/giggle1.wav",
                "./Assets/giggle2.wav",
                "./Assets/giggle3.wav",
                "./Assets/giggle4.wav",
                "./Assets/giggle5.wav"
            });
            clipsGiggle = new Shuffler<NamedAudioClip>(clips);



            gazeController = new GazeController(this, "Person");
            gazeController.SetReference("Person", "hipControl");
            gazeController.SetLookAtPlayer(-0.10f * Vector3.up); // applying target offset, 10cm down from player center-eye
        }

        public override void OnFixedUpdate()
        {
            gazeController.OnFixedUpdate();            
        }

        public override void OnUpdate()
        {
            nextClock -= Time.deltaTime;
            previousClock += Time.deltaTime;

            // Timer did run out OR the player moved very fast => giggle
            if (nextClock <= 0.0f || (previousClock > 2.5f && gazeController.GetCurrentAngle() > 45.0f))
            {
                NamedAudioClip audioClip = clipsGiggle.Next();
                personHeadAudio.Trigger(audioClip);
                nextClock = Random.Range(5.0f, 10.0f);
                previousClock = 0.0f;
            }
        }
    }
}