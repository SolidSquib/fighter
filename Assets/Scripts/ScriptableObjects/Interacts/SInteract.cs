using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class SInteract : ScriptableObject
{
        public abstract void Interact(Interactive interactive, Interactor interactor);
}
