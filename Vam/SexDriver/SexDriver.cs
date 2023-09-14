using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAM_ScriptEngine;
using MacGruber;

namespace VAMDeluxe
{
    public class SexDriver : Script
    {
        public bool inserted = false;
        public bool deepInsert = false;

        public Activity activity;
        public HipAction hipAction;
        public BreatheAction breatheAction;
        public GazeController gazeController;

        OMeter meter;

        public AdjustJoints jawAdjust;
        public DAZMeshEyelidControl eyelidControl;

        public OutTriggerAudio headAudioSource;

        public static string SAVEPATH;

        GameObject sexDriver;
        Canvas driverCanvas;

        public override void OnPreLoad()
        {
            RegisterInAction("Insertion", ()=> { inserted = true; } );
            RegisterInAction("Pull Out", ()=> { inserted = false; } );
            RegisterInAction("Deep Insertion", () => { deepInsert = true; } );
            RegisterInAction("Deep Out", () => { deepInsert = false; } );

            RegisterHotkey(KeyCode.Z, () =>
            {
                activity.hipState.Switch(activity.sex);
            });

            RegisterHotkey(KeyCode.X, () =>
            {
                activity.hipState.Switch(activity.climax);
            });

            RegisterHotkey(KeyCode.C, () =>
            {
                activity.hipState.Switch(activity.refractory);
            });
        }

        public override void OnPostLoad()
        {
            SAVEPATH = Application.dataPath + "/../" + Utils.GetSceneDirectory();

            sexDriver = GameObject.Find("SexDriver(Clone)");
            if (sexDriver != null && Utils.IsDesktopMode() == false)
            {
                driverCanvas = sexDriver.GetComponent<Canvas>();
                SuperController.singleton.AddCanvas(driverCanvas);
            }

            SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChanged;
            hipAction = new HipAction(this);
            activity = new Activity(this);
            breatheAction = new BreatheAction(this);
            gazeController = new GazeController(this, "Person");
            gazeController.SetReference("Person", "eyeTargetControl");
            gazeController.SetLookAtPlayer(-0.70f * Vector3.up);
            gazeController.SetGazeDuration(0.2f);


            EnsureAtomsCreated();


            headAudioSource = RegisterOutAudio("Person", "HeadAudioSource", "PlayNow");
            jawAdjust = Utils.GetAtom("Person").GetStorableByID("JawControl") as AdjustJoints;
            jawAdjust.driveXRotationFromAudioSourceMultiplier = 115.0f;
            jawAdjust.driveXRotationFromAudioSourceAdditionalAngle = 0.0f;
            jawAdjust.driveXRotationFromAudioSourceMaxAngle = -5.0f;

            eyelidControl = Utils.GetAtom("Person").GetStorableByID("EyelidControl") as DAZMeshEyelidControl;
            eyelidControl.SyncBlinkEnabled(true);


            meter = new OMeter(activity);

            Vamasutra sutra = new Vamasutra();
        }

        public override void OnUnLoad()
        {
            GameObject sexDriver = GameObject.Find("SexDriver(Clone)");
            if (sexDriver != null && Utils.IsDesktopMode() == false)
            {
                SuperController.singleton.RemoveCanvas(sexDriver.GetComponent<Canvas>());
            }
            SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChanged;
        }

        public void OnAtomUIDsChanged(List<string> atomUIDs)
        {
            SuperController.singleton.StartCoroutine(WaitAndUpdate());
        }

        private IEnumerator WaitAndUpdate()
        {
            yield return new WaitForSeconds(0.1f);
            EnsureAtomsCreated();
        }

        private void EnsureAtomsCreated()
        {
            hipAction.Init();
            breatheAction.Init();
        }

        public override void OnUpdate()
        {
            hipAction.Update(inserted);
            activity.Update(inserted, hipAction);
            breatheAction.Update(activity);
            meter.Update();
        }

        public override void OnFixedUpdate()
        {
            //gazeController.OnFixedUpdate();

        }

        public override void OnLateUpdate()
        {
            if (sexDriver != null && SuperController.singleton !=null)
            {

                if (Utils.IsDesktopMode() == false)
                {
                    driverCanvas.enabled = SuperController.singleton.mainHUD.gameObject.activeSelf && (SuperController.singleton.activeUI == SuperController.ActiveUI.SelectedOptions || SuperController.singleton.activeUI == SuperController.ActiveUI.None) && Utils.GetSelectedAtom()==null;
                    sexDriver.transform.SetPositionAndRotation(SuperController.singleton.mainHUD.transform.position, SuperController.singleton.mainHUD.transform.rotation);
                    sexDriver.transform.Rotate(0, 180, 0);
                    sexDriver.transform.Translate(0, 0.2f, 0.0f);
                }
                else
                {
                    sexDriver.transform.LookAt(Camera.main.transform, Vector3.up);
                    sexDriver.transform.Rotate(0, 180, 0);
                }
            }
        }

        public static void HideController(Atom atom)
        {
            FreeControllerV3 controller = atom.GetStorableByID("control") as FreeControllerV3;
            controller.deselectedMeshScale = 0.000f;

            SphereCollider collider = controller.GetComponent<SphereCollider>();
            collider.radius = 0.0f;
            collider.enabled = false;
        }

    }
}