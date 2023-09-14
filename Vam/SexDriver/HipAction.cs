using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VAM_ScriptEngine;

namespace VAMDeluxe
{
    public class HipAction
    {

        public float maxThrust = 300.0f;
        public float maxGrind = 60.0f;
        private float maxThrustQuickness = 40.0f;

        Slider thrustSlider;
        Slider grindSlider;

        CycleForceProducerV2 cycleForce;

        SexDriver actor;

        Shuffler<NamedAudioClip> skinSFX;
        Shuffler<NamedAudioClip> insertSFX;
        float nextSkinSFXTime = 0.0f;
        AudioSourceControl skinAudioSource;

        public HipAction(SexDriver actor)
        {
            GameObject sexDriver = GameObject.Find("SexDriver(Clone)");
            if (sexDriver != null)
            {
                GameObject thrustSliderGO = GameObject.Find("Thrust Slider");
                if (thrustSliderGO != null)
                {
                    thrustSlider = thrustSliderGO.GetComponent<Slider>();
                }

                GameObject grindSliderGO = GameObject.Find("Grind Slider");
                if (grindSliderGO != null)
                {
                    grindSlider = grindSliderGO.GetComponent<Slider>();
                }
            }

            this.actor = actor;

            List<NamedAudioClip> skinfxArray = new List<NamedAudioClip>();
            for(int i=1; i<=5; i++)
            {
                skinfxArray.Add(Expression.LoadAudio("./Audio/sfx_skin/skin_" + i + ".wav"));
            }
            skinSFX = new Shuffler<NamedAudioClip>(skinfxArray.ToArray());

            List<NamedAudioClip> insertfxArray = new List<NamedAudioClip>();
            for (int i = 1; i <= 3; i++)
            {
                insertfxArray.Add(Expression.LoadAudio("./Audio/sfx_skin/insert_" + i + ".wav"));
            }
            insertSFX = new Shuffler<NamedAudioClip>(insertfxArray.ToArray());
        }

        public void Init()
        {
            CheckCreateCycleForce("CF_Thrust", out cycleForce);
            CreateSFXSource("AS_Skin");
        }

        private void CheckCreateCycleForce(string name, out CycleForceProducerV2 cf)
        {
            Atom atom = Utils.GetAtom(name);
            if (atom == null)
            {
                SuperController.singleton.StartCoroutine(SuperController.singleton.AddAtomByType("CycleForce", name, true));
                cf = null;
            }
            else
            {
                cf = atom.GetStorableByID("ForceProducer") as CycleForceProducerV2;
                if (cf != null)
                {
                    cf.SetForceReceiver("Person:hip");

                    FreeControllerV3 hipControl = Utils.GetStorable<FreeControllerV3>(Utils.GetAtom("Person"), "hipControl");
                    if (hipControl != null)
                    {
                        FreeControllerV3 cfControl = Utils.GetStorable<FreeControllerV3>(atom, "control");
                        if (cfControl != null)
                        {
                            cfControl.transform.SetPositionAndRotation(hipControl.transform.position, hipControl.transform.rotation);

                            cfControl.transform.LookAt(Utils.GetStorable<FreeControllerV3>(Utils.GetAtom("Person"), "headControl").transform);
                            cfControl.transform.Rotate(0,180,0,Space.Self);
                            cfControl.transform.Rotate(new Vector3(0, 0, 1), 90, Space.Self);
                            cfControl.transform.Rotate(new Vector3(1, 0, 0), -90, Space.Self);
                            cfControl.transform.Rotate(new Vector3(0, 0, 1), 90, Space.Self);

                            cfControl.currentPositionState = FreeControllerV3.PositionState.ParentLink;
                            cfControl.currentRotationState = FreeControllerV3.RotationState.ParentLink;

                            Rigidbody rb = SuperController.singleton.RigidbodyNameToRigidbody("Person:hipControl");
                            cfControl.SelectLinkToRigidbody(rb, FreeControllerV3.SelectLinkState.PositionAndRotation);



                            SexDriver.HideController(cfControl.containingAtom);
                        }
                    }
                }
            }
        }

        private void CreateSFXSource(string name)
        {
            Atom atom = Utils.GetAtom(name);
            if (atom == null)
            {
                SuperController.singleton.StartCoroutine(SuperController.singleton.AddAtomByType("AudioSource", name, true));
                skinAudioSource = null;
            }
            else
            {
                skinAudioSource = atom.GetStorableByID("AudioSource") as AudioSourceControl;
                if (skinAudioSource != null)
                {
                    FreeControllerV3 hipControl = Utils.GetStorable<FreeControllerV3>(Utils.GetAtom("Person"), "hipControl");
                    if (hipControl != null)
                    {
                        FreeControllerV3 sfxControl = Utils.GetStorable<FreeControllerV3>(atom, "control");
                        sfxControl.transform.SetPositionAndRotation(hipControl.transform.position, hipControl.transform.rotation);
                        sfxControl.currentPositionState = FreeControllerV3.PositionState.ParentLink;
                        sfxControl.currentRotationState = FreeControllerV3.RotationState.ParentLink;
                        Rigidbody rb = SuperController.singleton.RigidbodyNameToRigidbody("Person:hipControl");
                        sfxControl.SelectLinkToRigidbody(rb, FreeControllerV3.SelectLinkState.PositionAndRotation);

                        skinAudioSource.volume = 0.5f;
                        SexDriver.HideController(sfxControl.containingAtom);
                    }
                }
            }
        }

        public void Update(bool inserted)
        {
            if (thrustSlider != null && grindSlider != null && cycleForce != null)
            {

                if (actor.activity.hipState.CurrentState == actor.activity.refractory)
                {
                    cycleForce.forceFactor = 0.0f;
                    cycleForce.torqueFactor = 0.0f;
                    return;
                }

                if (inserted)
                {
                    //cycleForce.period = Remap(thrustSlider.value, 0.0f, 1.0f, 1.0f, 0.5f);
                    //cycleForce.forceFactor = thrustSlider.value * maxThrust;
                    //cycleForce.forceQuickness = thrustSlider.value * maxThrustQuickness;
                    //cycleForce.torqueFactor = grindSlider.value * maxGrind;
                }
                else
                {
                    //cycleForce.forceFactor = 0.0f;
                    //cycleForce.torqueFactor = 0.0f;
                }

                if (thrustSlider.value > 0)
                {
                    cycleForce.forceQuickness = Remap(thrustSlider.value, 0.0f, 1.0f, 0.7f, 6.0f);
                    cycleForce.forceFactor = Remap(thrustSlider.value, 0.0f, 1.0f, 500, 300);
                    cycleForce.forceDuration = Remap(thrustSlider.value, 0.0f, 1.0f, 0.9f, 1.0f);
                }
                else
                {
                    cycleForce.forceQuickness = 0.0f;
                }


                if(grindSlider.value > 0)
                {
                    cycleForce.torqueFactor = Remap(grindSlider.value, 0.0f, 1.0f, 0.0f, 120.0f);
                    cycleForce.torqueQuickness = Remap(grindSlider.value, 0.0f, 1.0f, 0.7f, 6.0f);
                }
                else
                {
                    cycleForce.torqueQuickness = 0.0f;
                }

                cycleForce.period = Remap(Mathf.Clamp01(thrustSlider.value + grindSlider.value), 0.0f, 1.0f, 2.2f, 0.3f);



                if (Time.fixedTime > nextSkinSFXTime && (grindSlider.value > 0 || thrustSlider.value > 0))
                {
                    if(cycleForce.forceQuickness > 4.4f && cycleForce.appliedForce.magnitude >= 200.0f)
                    {
                        NamedAudioClip clip = skinSFX.Next();
                        skinAudioSource.PlayNow(clip);
                    }
                    else
                    if(cycleForce.forceQuickness > 0.5f || (cycleForce.torqueFactor > 1.0f && cycleForce.appliedTorque.magnitude>=10.0f))
                    {
                        NamedAudioClip clip = insertSFX.Next();
                        skinAudioSource.PlayNow(clip);
                    }
                    nextSkinSFXTime = Time.fixedTime + cycleForce.period;
                }
            }
        }

        public float GetHipIntensity()
        {
            if (thrustSlider == null || grindSlider == null)
            {
                return 0.0f;
            }
            return Mathf.Max(thrustSlider.value, grindSlider.value);
        }

        private float Remap(float x, float x1, float x2, float y1, float y2)
        {
            var m = (y2 - y1) / (x2 - x1);
            var c = y1 - m * x1; // point of interest: c is also equal to y2 - m * x2, though float math might lead to slightly different results.

            return m * x + c;
        }
    }
}
