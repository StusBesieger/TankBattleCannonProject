using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using UnityEngine;
using Modding;
using Modding.Serialization;
using Modding.Modules;
using Modding.Blocks;
using skpCustomModule;
using Vector3 = UnityEngine.Vector3;

namespace TBCStusSpace
{
    public class AdAmmunitionStorage : BlockScript
    {
        public float Range = 10f;
        public LayerMask layermask = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 25) | (1 << 26);
        private BlockBehaviour bb;
        private FireTag fireTag;
        private Rigidbody rigidbody;
        private TBCAddProjectileBehaviour[] ChildObjects;
        private Transform GOparent;
        private int num;
        private bool explodeOK = true;
        public AudioSource audiosource;
        public AudioClip sound;
        public GameObject EffectPrefab;
        public GameObject EffectObject;
        public ParticleSystem particleSystem;
        public override void OnBlockPlaced()
        {
            base.OnBlockPlaced();
        }
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            EffectPrefab = Mod.modAssetBundle.LoadAsset<GameObject>("HEexplosion");
            EffectObject = (GameObject)Instantiate(EffectPrefab, transform);
            particleSystem = EffectObject.GetComponent<ParticleSystem>();
            particleSystem.Stop();

            audiosource = gameObject.AddComponent<AudioSource>();
            audiosource.spatialBlend = 1.0f;
            audiosource.volume = 1.5f;
            sound = ModResource.GetAudioClip("HEHit");

            GOparent = this.transform;
            if(bb == null)
            {
                bb = this.gameObject.GetComponent<BlockBehaviour>();
            }
            num = 0;
            while(GOparent.gameObject.name != "Simulation Machine")
            {
                GOparent = GOparent.transform.parent;
            }

            ChildObjects = GOparent.GetComponentsInChildren<TBCAddProjectileBehaviour>();
            num = ChildObjects.Length;
            foreach (TBCAddProjectileBehaviour childobject in ChildObjects)
            {
                childobject.TBCAmmoStock += 10/num;
                childobject.start = true;
            }
        }
        public override void SimulateFixedUpdateAlways()
        {
            base.SimulateFixedUpdateAlways();
            if(explodeOK)
            {
                if (bb.BlockHealth.health == 0f)
                {
                    Explode();
                    audiosource.PlayOneShot(sound);
                    EffectObject.transform.position = this.transform.position;

                    particleSystem.Play();
                    explodeOK = false;
                }
                
            }
            
        }
        public void Explode()
        {
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, 4.0f, layermask);
            foreach(Collider collider in colliders)
            {
                rigidbody = collider.GetComponent<Rigidbody>();

                if(rigidbody == null)
                {
                    rigidbody = collider.transform.parent.GetComponent<Rigidbody>();
                    if (rigidbody == null)
                    {
                        rigidbody = collider.transform.parent.parent.GetComponent<Rigidbody>();
                    }
                }
                if(rigidbody)
                {
                    fireTag = rigidbody.gameObject.GetComponent<FireTag>();
                    if(fireTag != null)
                    {
                        fireTag.Ignite();
                    }
                    rigidbody.AddExplosionForce(25, this.transform.position, 2.0f, 5.0f, ForceMode.Impulse);
                }
            }
        }
    }
}
