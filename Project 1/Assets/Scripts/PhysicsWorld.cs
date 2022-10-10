using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Collision manager that contains references to all physics objects in the scene and
/// computes any collisions between them.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class PhysicsWorld : MonoBehaviour
{
    /// <summary>
    /// The minimum (bottom left) corner of the map boundaries
    /// </summary>
    public Vector2 mapBoundsMin;

    /// <summary>
    /// The maximum (top right) corner of the map boundaries
    /// </summary>
    public Vector2 mapBoundsMax;

    /// <summary>
    /// List of all of the PhysicsObjects currently in the physics simulation
    /// </summary>
    private List<PhysicsObject> physicsObjects = new List<PhysicsObject>();

    /// <summary>
    /// Registers a PhysicsObject as part of the physics simulation, enabling collision
    /// </summary>
    /// <param name="obj">PhysicsObject to add to the simulation</param>
    public void AddObject(PhysicsObject obj)
    {
        physicsObjects.Add(obj);
        obj.physicsWorld = this;
    }

    /// <summary>
    /// Removes a PhysicsObject from the physics simulation, disabling collision
    /// </summary>
    /// <param name="obj">PhysicsObject to remove from the simulation</param>
    public void RemoveObject(PhysicsObject obj)
    {
        physicsObjects.Remove(obj);
        obj.physicsWorld = null;
    }

    /// <summary>
    /// On LateUpdate (after all PhysicsObjects have been moved), check each registered PhysicsObject
    /// for collisions between the map boundaries (walls) and other PhysicsObjects
    /// </summary>
    private void LateUpdate()
    {
        for (int i = physicsObjects.Count - 1; i >= 0; i--)
        {
            PhysicsObject obj = physicsObjects[i];

            Vector2 circleCenter = obj.worldCircleCenter;
            float circleRadius = obj.worldCircleRadius;

            // Left wall
            if (circleCenter.x - circleRadius < mapBoundsMin.x)
            {
                obj.OnWallCollision(new Vector2(mapBoundsMin.x, circleCenter.y), Vector2.right);
            }

            // Right wall
            if (circleCenter.x + circleRadius > mapBoundsMax.x)
            {
                obj.OnWallCollision(new Vector2(mapBoundsMax.x, circleCenter.y), Vector2.left);
            }

            // Lower wall
            if (circleCenter.y - circleRadius < mapBoundsMin.y)
            {
                obj.OnWallCollision(new Vector2(circleCenter.x, mapBoundsMin.y), Vector2.up);
            }

            // Upper wall
            if (circleCenter.y + circleRadius > mapBoundsMax.y)
            {
                obj.OnWallCollision(new Vector2(circleCenter.x, mapBoundsMax.y), Vector2.down);
            }

            // Other PhysicsObject collisions
            for (int j = i - 1; j >= 0; j--)
            {
                PhysicsObject otherObj = physicsObjects[j];

                Vector2 otherCircleCenter = otherObj.worldCircleCenter;
                float otherCircleRadius = otherObj.worldCircleRadius;

                if ((otherCircleCenter - circleCenter).sqrMagnitude < (otherCircleRadius + circleRadius) * (otherCircleRadius + circleRadius))
                {
                    Vector2 normal = (otherCircleCenter - circleCenter).normalized;
                    obj.OnObjectCollision(otherObj, circleCenter + normal * circleRadius, normal);
                    otherObj.OnObjectCollision(obj, otherCircleCenter - normal * otherCircleRadius, -normal);
                }
            }
        }
    }
}