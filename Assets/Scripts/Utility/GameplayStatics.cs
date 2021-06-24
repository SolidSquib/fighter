using UnityEngine;

namespace Fighter
{
    public static class GameplayStatics
    {
        /* This accounts for perspective camera projections and ensures that vertical joystick movement always
         * moves the character directly forwards and backwards instead of being influenced by the camera's 
         * perspective. It feels much nicer that the default behaviour. */
        public static Vector3 ProjectInputVectorToCamera(Camera targetCamera, Transform referenceTransform, Vector3 inputVector)
        {
            if (!targetCamera.orthographic)
            {
                Vector3 screenPoint = targetCamera.WorldToScreenPoint(referenceTransform.position);
                Ray ray = targetCamera.ScreenPointToRay(screenPoint);
                Vector3 forwardDirection = ray.direction;
                forwardDirection.y = 0;

                Vector3 forwardMovement = forwardDirection * inputVector.z;
                inputVector.z = 0;
                inputVector += forwardMovement;
            }

            return inputVector;
        }

        public static bool TraceForGroundUnderneath(PlayerMovement playerMovement, int groundLayerMask)
        {
            if (playerMovement == null)
            {
                Debug.LogWarning($"Unable to trace for ground underneath a null PlayerMovement object.");
                return false;
            }

            const float traceRadius = 0.1f;
            float capsuleHalfHeight = Mathf.Max(playerMovement.playerCapsule.radius, playerMovement.playerCapsule.height * 0.5f);
            Vector3 traceStart = playerMovement.transform.position;
            traceStart.y -= capsuleHalfHeight + traceRadius;
            Vector3 traceEnd = new Vector3(traceStart.x, traceStart.y - traceRadius, traceStart.z);

            Debug.DrawLine(traceStart, traceEnd, Color.red, 0);

            if (groundLayerMask != 0)
            {
                return Physics.CheckSphere(traceStart, traceRadius, groundLayerMask);
            }
            else
            {
                return Physics.CheckSphere(traceStart, traceRadius);
            }
        }
    }
}