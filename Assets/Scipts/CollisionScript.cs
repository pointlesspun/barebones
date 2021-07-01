using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    public float damage;

    public bool destroyOnImpact = true;

    public float bounceReverseForce = 0;
    public float impactCooldown = 0.0f;

    public string[] bounceTags;

    private float _lastImpact = float.MinValue;

    private Rigidbody _body;

    private PoolObject _meta;

    private void Start()
    {
        _body = GetComponent<Rigidbody>();
        _meta = GetComponent<PoolObject>();
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

            if (destroyOnImpact)
            {
                if (_meta != null)
                {
                    ObjectPoolCollection.instance.Release(_meta);
                }
                else
                {
                    Destroy(gameObject);
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
