using System;

using UnityEngine;

namespace BareBones.Game
{
    public class CollisionScript : MonoBehaviour
    {
        public float damage;

        public OnEndOfLifeAction _onImpactAction = OnEndOfLifeAction.None;

        public float bounceReverseForce = 0;
        public float impactCooldown = 0.0f;

        public string[] bounceTags;

        private float _lastImpact = float.MinValue;

        private Rigidbody _body;

        public void Start()
        {
            _body = GetComponent<Rigidbody>();
        }

        void OnCollisionEnter(Collision collision)
        {
            if (impactCooldown <= 0 || (Time.time - _lastImpact) > impactCooldown)
            {
                var hitpointsOther = collision.gameObject.GetComponent<Hitpoints>();

                if (hitpointsOther != null)
                {
                    hitpointsOther.OnHit(damage);
                }

                if (_onImpactAction != OnEndOfLifeAction.None)
                {
                    switch (_onImpactAction)
                    {
                        case OnEndOfLifeAction.Disable:
                            // xxx assumption here is this object is pooled and will be cleaned up 
                            // the objectpool's sweep
                            gameObject.SetActive(false);
                            break;
                        case OnEndOfLifeAction.Destroy:
                            GameObject.Destroy(gameObject);
                            break;
                        case OnEndOfLifeAction.Release:
                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (bounceReverseForce > 0 && _body != null)
                {
                    if (CanBounceAgainst(collision.gameObject, bounceTags))
                    {
                        _body.AddForce(-bounceReverseForce * _body.velocity.normalized);
                    }
                }

                _lastImpact = Time.time;
            }
        }

        private bool CanBounceAgainst(GameObject other, string[] tags)
        {
            for (var i = 0; i < tags.Length; i++)
            {
                if (other.CompareTag(tags[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}