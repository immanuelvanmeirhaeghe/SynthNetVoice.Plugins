using UnityEngine;
using VAM_ScriptEngine;

namespace MacGruber
{
    class HandjobDemo : Script
    {
        private static StateMachine stateMachine;
        private static BlendQueue handjobQueue;

        public static OutTriggerFloat lHandSpeed;
        public static OutTriggerFloat lHandMoveSpeed;
        public static OutTriggerAction lHandMovePlay;
        public static OutTriggerFloat rHandSpeed;
        public static OutTriggerFloat rHandPosition;
        public static OutTriggerAction rHandPlay;
        public static OutTriggerAction rHandPause;

        public override void OnPreLoad()
        {
            stateMachine = new StateMachine();
            handjobQueue = new BlendQueue();

            RegisterInString("Interrupt", OnInterrupt);            
        }

        public override void OnPostLoad()
        {
            lHandSpeed = RegisterOutFloat("AP#LHand", "AnimationPattern", "speed");
            lHandMoveSpeed = RegisterOutFloat("AP#LHandMove", "AnimationPattern", "speed");
            lHandMovePlay = RegisterOutAction("AP#LHandMove", "AnimationPattern", "Play");

            rHandSpeed    = RegisterOutFloat("AP#RHand", "AnimationPattern", "speed");
            rHandPosition = RegisterOutFloat("AP#RHand", "AnimationPattern", "currentTime");
            rHandPlay     = RegisterOutAction("AP#RHand", "AnimationPattern", "Play");
            rHandPause    = RegisterOutAction("AP#RHand", "AnimationPattern", "Pause");


            stateMachine.Switch(jerkStart);
        }

        public override void OnFixedUpdate()
        {
            stateMachine.OnUpdate();
            handjobQueue.OnUpdate();
        }

        private void OnInterrupt(string parameter)
        {
            stateMachine.OnInterrupt(parameter);
        }

        #region States
        private static State jerkStart = new JerkStart();
        private static State jerkStop = new JerkStop();
        private static State jerkSlowConst = new JerkSlowConst();
        private static State jerkMediumConst = new JerkMediumConst();
        private static State jerkFastConst = new JerkFastConst();
        private static State teaseStart = new TeaseStart();
        private static State teaseStop = new TeaseStop();
        private static State teaseSlowConst = new TeaseSlowConst();
        private static State teaseFastConst = new TeaseFastConst();


        private class JerkStart : State
        {
            public override void OnEnter()
            {
                rHandPlay.Trigger();
                rHandSpeed.Trigger(1.0f);

                stateMachine.SwitchRandom(new State[] {
                    jerkSlowConst,
                    jerkMediumConst
                });
            }
        }

        private class JerkStop : State
        {
            public override void OnEnter()
            {
                rHandPause.Trigger();
                float speed = rHandSpeed.ReadValue();
                float position = rHandPosition.ReadValue();
                float duration = (2.0f - position) / Mathf.Max(speed, 0.1f);
                handjobQueue.QueueInstant(rHandPosition, 2.0f, duration);

                Duration = Random.Range(5.0f, 10.0f);
            }

            public override void OnTimeout()
            {
                stateMachine.SwitchRandom(new State[] {
                    jerkStart,
                    teaseStart
                });
            }
        }


        private class JerkSlowConst : State
        {
            public override void OnEnter()
            {
                float speed = Random.Range(0.5f, 1.5f);
                handjobQueue.QueueInstant(rHandSpeed, speed, 2.0f);
                Duration = Random.Range(5.0f, 20.0f);
            }

            public override void OnTimeout()
            {
                stateMachine.SwitchRandom(new State[] {
                    jerkMediumConst,
                    jerkFastConst,
                    jerkStop
                });
            }
        }

        private class JerkMediumConst : State
        {
            public override void OnEnter()
            {
                float speed = Random.Range(1.5f, 3.0f);
                handjobQueue.QueueInstant(rHandSpeed, speed, 1.5f);
                Duration = Random.Range(5.0f, 20.0f);
            }

            public override void OnTimeout()
            {
                stateMachine.SwitchRandom(new State[] {
                    jerkSlowConst,
                    jerkFastConst
                });
            }
        }

        private class JerkFastConst : State
        {
            public override void OnEnter()
            {
                float speed = Random.Range(3.0f, 5.0f);
                handjobQueue.QueueInstant(rHandSpeed, speed, 1.0f);
                Duration = Random.Range(5.0f, 10.0f);
            }

            public override void OnTimeout()
            {
                stateMachine.SwitchRandom(new State[] {
                    jerkSlowConst,
                    jerkMediumConst,
                    jerkStop
                });
            }
        }

        private class TeaseStart : State
        {
            public override void OnEnter()
            {
                lHandMoveSpeed.Trigger(1.0f);
                lHandMovePlay.Trigger();

                stateMachine.SwitchRandom(new State[] {
                    teaseSlowConst,
                    teaseFastConst,
                    teaseStop
                });
            }
        }

        private class TeaseStop : State
        {
            public override void OnEnter()
            {
                lHandMoveSpeed.Trigger(-1.0f);
                lHandMovePlay.Trigger();

                Duration = Random.Range(1.5f, 5.0f);
            }

            public override void OnTimeout()
            {
                stateMachine.SwitchRandom(new State[] {
                    jerkStart,
                    teaseStart
                });
            }
        }

        private class TeaseSlowConst : State
        {
            public override void OnEnter()
            {
                float speed = Random.Range(0.5f, 1.5f);
                handjobQueue.QueueInstant(lHandSpeed, speed, 2.0f);
                Duration = Random.Range(5.0f, 20.0f);
            }

            public override void OnTimeout()
            {
                stateMachine.SwitchRandom(new State[] {
                    teaseSlowConst,
                    teaseFastConst,
                    teaseStop
                });
            }
        }

        private class TeaseFastConst : State
        {
            public override void OnEnter()
            {
                float speed = Random.Range(1.5f, 3.0f);
                handjobQueue.QueueInstant(lHandSpeed, speed, 1.0f);
                Duration = Random.Range(3.0f, 10.0f);
            }

            public override void OnTimeout()
            {
                stateMachine.SwitchRandom(new State[] {
                    teaseSlowConst,
                    teaseFastConst,
                    teaseStop
                });
            }
        }
        #endregion
    }
}
