using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour, IGameplayTagOwner
{
    [RequireInterface(typeof(IGameplayTagOwner))]
    [SerializeField] Object _parentTagOwner;

    public TagContainer gameplayTags 
    {
        get 
        {
            IGameplayTagOwner tagOwner = (IGameplayTagOwner)_parentTagOwner;
            return tagOwner != null ? tagOwner.gameplayTags : null;
        }
    }
}
