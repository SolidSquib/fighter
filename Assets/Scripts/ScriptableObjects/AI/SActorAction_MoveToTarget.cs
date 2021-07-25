using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI Actor/Actions/MoveToTarget")]
public class SActorAction_MoveToTarget : SActorAction
{
    public override void Act(AIController controller)
    {
        controller.navMeshAgent.destination = controller.target.transform.position;
        controller.navMeshAgent.isStopped = false;

        if (controller.navMeshAgent.path.corners.Length > 1)
        {
            Vector3 moveDirection = (controller.navMeshAgent.path.corners[1] - controller.transform.position).normalized;
            controller.GetComponent<CharacterMovement>().inputVector = moveDirection;
        }
    }
}
