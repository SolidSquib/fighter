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
            Vector3 outputVector = inputVector;

            if (!targetCamera.orthographic)
            {
                Vector3 forwardDirection = GetProjectedForwardVector(targetCamera, referenceTransform);

                Vector3 lateral = targetCamera.transform.right * inputVector.x;
                Vector3 forward = forwardDirection * inputVector.z;
                outputVector = (lateral + forward).normalized;
            }

            return outputVector;
        }

        public static Vector3 GetProjectedForwardVector(Camera targetCamera, Transform referenceTransform)
        {
            Vector3 screenPoint = targetCamera.WorldToScreenPoint(referenceTransform.position);
            Ray ray = targetCamera.ScreenPointToRay(screenPoint);
            Vector3 forwardDirection = ray.direction;
            forwardDirection.y = 0;

            return forwardDirection.normalized;
        }
    }
}