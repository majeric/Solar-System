using System.Collections.Generic;
using UnityEngine;

public class EndlessManager : MonoBehaviour {

    public float distanceThreshold = 1000;
    private List<Transform> physicsObjects;
    private Ship ship;
    private PlayerController player;
    private Camera playerCamera;

    public event System.Action PostFloatingOriginUpdate;

    private void Awake () {
        var ship = FindObjectOfType<Ship> ();
        var player = FindObjectOfType<PlayerController> ();
        var bodies = FindObjectsOfType<CelestialBody> ();

        physicsObjects = new List<Transform> ();
        physicsObjects.Add (ship.transform);
        physicsObjects.Add (player.transform);
        foreach (var c in bodies) {
            physicsObjects.Add (c.transform);
        }

        playerCamera = Camera.main;
    }

    private void LateUpdate () {
        UpdateFloatingOrigin ();
        if (PostFloatingOriginUpdate != null) {
            PostFloatingOriginUpdate ();
        }
    }

    private void UpdateFloatingOrigin () {
        Vector3 originOffset = playerCamera.transform.position;
        float dstFromOrigin = originOffset.magnitude;

        if (dstFromOrigin > distanceThreshold) {
            foreach (Transform t in physicsObjects) {
                t.position -= originOffset;
            }
        }
    }

}