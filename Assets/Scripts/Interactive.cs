using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactive : MonoBehaviour
{
    [SerializeField] TagRequirementsContainer _interactionRequiredTags = new TagRequirementsContainer();
    [SerializeField] SInteract _interaction;
    public TagRequirementsContainer interactionRequiredTags { get { return _interactionRequiredTags; } }

    public bool CanInteract(Interactor interactor)
    {
        return _interaction != null && _interactionRequiredTags.RequirementsMet(interactor.gameplayTags);
    }

    public void Interact(Interactor interactor)
    {
        if (CanInteract(interactor))
        {
            _interaction.Interact(this, interactor);
        }
    }
}
