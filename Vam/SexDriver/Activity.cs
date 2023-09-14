using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VAM_ScriptEngine;
using MacGruber;

namespace VAMDeluxe
{
    public class Activity
    {
        public StateMachine hipState;
        public Idle idle;
        public Sex sex;
        public Climax climax;
        public Refractory refractory;

        public SexDriver actor;

        public float intensity = 0.0f;
        public float desiredIntensity = 0.0f;
        public float maxIntensity = 1.0f;
        public float intensityDecay = 0.1f;

        public ExpressionManager expressionManager;

        public Activity(SexDriver actor)
        {
            this.actor = actor;

            expressionManager = new ExpressionManager(actor);

            idle = new Idle(this);
            sex = new Sex(this);
            climax = new Climax(this);
            refractory = new Refractory(this);

            hipState = new StateMachine();
            hipState.Switch(idle);
        }

        public void Update(bool inserted, HipAction hipAction)
        {
            hipState.OnUpdate();
            expressionManager.Update(this);
        }

        #region States

        #region ActivityState
        public class ActivityState : State
        {
            protected Activity activity;
            public ActivityState(Activity activity) : base()
            {
                this.activity = activity;
            }
        }
        #endregion

        #region Idle
        public class Idle : ActivityState
        {
            public Idle(Activity activity) : base(activity) { }
            public override void OnEnter()
            {
                Duration = 2.6f;
                activity.expressionManager.StartBreathing(activity.actor.breatheAction, Duration);
                activity.actor.eyelidControl.SyncBlinkEnabled(true);
            }
            public override void OnUpdate()
            {
                activity.actor.eyelidControl.Update();
                activity.actor.eyelidControl.UpdateBlink();
                if ( activity.actor.hipAction.GetHipIntensity() > 0.01f)
                {
                    stateMachine.Switch(activity.sex);
                }

                activity.intensity -= activity.intensityDecay * 0.4f * 1.0f / (1.4f - activity.intensity) * Time.deltaTime;
                activity.intensity = Mathf.Clamp(activity.intensity, 0, activity.maxIntensity);
            }
            public override void OnTimeout()
            {
                stateMachine.Switch(activity.idle);
            }

        }
        #endregion

        #region Sex
        public class Sex : ActivityState
        {
            public Sex(Activity activity) : base(activity) { }


            public override void OnEnter()
            {
                activity.actor.gazeController.SetLookAtPlayer(-1.4f * Vector3.up);
                activity.actor.gazeController.SetGazeDuration(0.2f);
                ExpressionManager em = activity.expressionManager;
                em.panting.Stop();
                em.breathingActive.Stop();
                em.breathingIdle.Stop();
                activity.actor.eyelidControl.SyncBlinkEnabled(false);
            }

            public override void OnUpdate()
            {
                UpdateIntensity(activity.actor.inserted, activity.actor.hipAction);

                ExpressionManager em = activity.expressionManager;
                if (em.CanPlay())
                {
                    em.SelectExpression(activity.intensity, activity.maxIntensity, activity.desiredIntensity);
                }

                if (activity.intensity >= (activity.maxIntensity-0.005f))
                {
                    stateMachine.Switch(activity.climax);
                }

                if(activity.intensity <= 0.8f && activity.actor.hipAction.GetHipIntensity() < 0.01f)
                {
                    stateMachine.Switch(activity.idle);
                }

                bool headSelected = false;
                Atom person = Utils.GetAtom("Person");
                if (person != null)
                {
                    FreeControllerV3 control = person.GetStorableByID("headControl") as FreeControllerV3;
                    if (control != null && control.currentPositionState == FreeControllerV3.PositionState.ParentLink)
                    {
                        headSelected = true;
                    }
                }


                if (headSelected == false)
                {
                    activity.actor.gazeController.SetReference("Person", "eyeTargetControl");
                    activity.actor.gazeController.SetLookAtPlayer(-1.5f * Vector3.up);
                    activity.actor.gazeController.SetGazeDuration(0.3f);
                }
            }

            private void UpdateIntensity(bool inserted, HipAction hipAction)
            {
                float hipIntensity = hipAction.GetHipIntensity() * 0.08f;

                float addIntensity = 0.0f;

                if (hipIntensity > 0)
                {
                    addIntensity += hipIntensity;
                    addIntensity -= activity.intensityDecay * 0.2f * 1.0f / (1.4f - activity.intensity);
                }
                else
                {
                    addIntensity -= activity.intensityDecay * 1.5f;
                }


                activity.desiredIntensity = hipIntensity * 10.0f;

                activity.intensity += addIntensity * Time.deltaTime;
                activity.intensity = Mathf.Clamp(activity.intensity, 0, activity.maxIntensity);
            }

        }
        #endregion

        #region Climax
        public class Climax : ActivityState
        {
            public float climaxDuration = 10.0f;
            public Climax(Activity activity) : base(activity) { }
            public override void OnEnter()
            {
                Duration = climaxDuration;
                activity.expressionManager.SelectClimax(climaxDuration);
                //activity.actor.gazeController.SetLookAtPlayer(2.5f * Vector3.up);
                //activity.actor.gazeController.SetGazeDuration(4.0f);

            }

            public override void OnTimeout()
            {
                stateMachine.Switch(activity.refractory);
            }
        }
        #endregion

        #region Refractory
        public class Refractory : ActivityState
        {
            public float refractoryDuration = 6.0f;
            public Refractory(Activity activity) : base(activity) {}

            public override void OnEnter()
            {
                Duration = refractoryDuration;
                activity.expressionManager.StartPanting(refractoryDuration);
                activity.actor.gazeController.SetLookAtPlayer(-6.5f * Vector3.up);
                activity.actor.gazeController.SetGazeDuration(1.0f);
            }

            public override void OnUpdate()
            {
                activity.intensity -= 0.1f * Time.deltaTime;
                activity.intensity = Mathf.Clamp(activity.intensity, 0, activity.maxIntensity);
            }

            public override void OnTimeout()
            {
                stateMachine.Switch(activity.sex);
            }
        }
        #endregion

        #endregion

    }
}
