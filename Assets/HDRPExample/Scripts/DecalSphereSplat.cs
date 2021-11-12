using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalSphereSplat : MonoBehaviour {
    public LayerMask hitMask;
    public Material projector;
    public Color color;
    private static Collider[] colliders = new Collider[32];
    void Start() {
        projector = Material.Instantiate(projector);
        projector.color = color;
    }
    void Update() {
        float scale = transform.localScale.x;
        int hits = Physics.OverlapSphereNonAlloc(transform.position, scale, colliders, hitMask, QueryTriggerInteraction.UseGlobal);
        for(int i=0;i<hits;i++) {
            Collider c = colliders[i];
            foreach(Renderer r in c.transform.root.GetComponentsInChildren<Renderer>()) {
                PaintDecal.instance.RenderDecal(r, projector, transform.position-Vector3.forward*scale*0.5f, Quaternion.identity, color, Vector2.one*scale*0.5f, scale);
            }
        }
    }
}
