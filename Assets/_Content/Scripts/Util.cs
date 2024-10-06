using UnityEngine;

namespace vegeo
{
    public static class Util
    {
        static Plane XZPlane = new Plane(Vector3.up, Vector3.zero);

        public static Vector3 GetMousePositionOnXZPlane(Camera camera)
        {
            float distance;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (XZPlane.Raycast(ray, out distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                //Just double check to ensure the y position is exactly zero
                hitPoint.y = 0;
                return hitPoint;
            }
            return Vector3.zero;
        }
    }
}