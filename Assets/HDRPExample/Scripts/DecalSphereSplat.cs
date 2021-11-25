using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalSphereSplat : MonoBehaviour {
    public LayerMask hitMask;
    public Material projector;
    public Color color;
    private static Collider[] colliders = new Collider[32];
    private static List<Renderer> staticRenderers = new List<Renderer>();
    private static List<Renderer> staticTempRenderers = new List<Renderer>();
    private void GetComponentsInChildrenNoAlloc<T>(Transform t, List<T> temp, List<T> result) {
        t.GetComponents<T>(temp);
        result.AddRange(temp);
        for(int i=0;i<t.childCount;i++) {
            GetComponentsInChildrenNoAlloc<T>(t.GetChild(i), temp, result);
		}
	}
    void Start() {
        projector = Material.Instantiate(projector);
        projector.color = color;
    }
    void Update() {
        float scale = transform.localScale.x;
        int hits = Physics.OverlapSphereNonAlloc(transform.position, scale, colliders, hitMask, QueryTriggerInteraction.UseGlobal);
        for(int i=0;i<hits;i++) {
            Collider c = colliders[i];

            staticRenderers.Clear();
            GetComponentsInChildrenNoAlloc<Renderer>(c.transform.root, staticTempRenderers, staticRenderers);
            foreach(Renderer r in staticRenderers) {
                SkinnedMeshDecals.PaintDecal.instance.RenderDecal(r, projector, transform.position-Vector3.forward*scale*0.5f, Quaternion.identity, Vector2.one*scale*0.5f, scale);
            }
        }
    }
}
