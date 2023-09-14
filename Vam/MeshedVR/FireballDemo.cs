using UnityEngine;
using VAM_ScriptEngine;

namespace MacGruber
{
    class FireballDemo : Script
    {
        private ParticleSystem particleSystem;

        public override void OnPreLoad()
        {
            RegisterInAction("Boom", OnBoom);
        }

        public override void OnPostLoad()
        {
            GameObject go = GameObject.Find("Fireball(Clone)");
            if (go == null)
            {
                LogError("Fireball not found!");
                return;
            }
            particleSystem = go.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                LogError("ParticleSystem not found!");
                return;
            }
        }

        public void OnBoom()
        {
            if (particleSystem != null)
                particleSystem.Play();
        }
    }
}
