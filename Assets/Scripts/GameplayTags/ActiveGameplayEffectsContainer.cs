using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveGameplayEffectsContainer
{
    private int _currentId = 0;
    private AbilitySystem abilitySystem { get; set; }
    private Dictionary<ActiveEffectHandle, GameplayEffectSpec> activeEffectSpecs { get; set; } = new Dictionary<ActiveEffectHandle, GameplayEffectSpec>(); // Infinite and Duration based effects that are currently applied to this ability system.
    private Dictionary<Tag, List<ActiveEffectHandle>> tagChangeListeners { get; set; } = new Dictionary<Tag, List<ActiveEffectHandle>>();
    public ActiveGameplayEffectsContainer(AbilitySystem abilitySystem)
    {
        this.abilitySystem = abilitySystem;

        this.abilitySystem.dynamicOwnedTags.TagAdded += OnTagsChanged;
        this.abilitySystem.dynamicOwnedTags.TagRemoved += OnTagsChanged;
    }

    private void OnTagsChanged(object sender, TagEventArgs args)
    {
        List<ActiveEffectHandle> listeningEffects;
        if (tagChangeListeners.TryGetValue(args.tag, out listeningEffects))
        {
            foreach (var handle in listeningEffects)
            {
                GameplayEffectSpec spec;
                if (TryGetSpecFromHandle(handle, out spec))
                {
                    spec.effectTemplate.removalTagRequirements.RequirementsMet(abilitySystem.dynamicOwnedTags);
                }
            }
        }
    }

    public void RemoveEffectsWithTags(TagContainer tags)
    {
        if (tags != null)
        {
            foreach (var pair in activeEffectSpecs)
            {
                GameplayEffectSpec spec = pair.Value;
                if (spec.effectTemplate.effectTags.AnyTagsMatch(tags))
                {
                    // TODO how to handle effect removal?
                }
            }
        }
    }

    public bool TryGetSpecFromHandle(ActiveEffectHandle handle, out GameplayEffectSpec spec)
    {
        spec = null;
        if (activeEffectSpecs.ContainsKey(handle))
        {
            spec = activeEffectSpecs[handle];
            return true;
        }
        return false;
    }

    protected ActiveEffectHandle CreateNewActiveSpecHandle(GameplayEffectSpec spec)
    {
        ActiveEffectHandle handle = new ActiveEffectHandle();
        handle.id = _currentId++;
        handle.source = spec.source;
        handle.target = spec.target;
        return handle;
    }

    protected void AddTagListener(Tag tag, ActiveEffectHandle handle)
    {
        List<ActiveEffectHandle> handles;
        if (tagChangeListeners.TryGetValue(tag, out handles))
        {
            handles.Add(handle);
        }
        else
        {
            handles = new List<ActiveEffectHandle>();
            handles.Add(handle);
            tagChangeListeners.Add(tag, handles);
        }
    }

    public ActiveEffectHandle AddActiveGameplayEffect(GameplayEffectSpec spec)
    {
        if (spec == null)
        {
            return ActiveEffectHandle.Invalid;
        }

        spec.applicationTime = Time.time;
        ActiveEffectHandle handle = CreateNewActiveSpecHandle(spec);
        activeEffectSpecs.Add(handle, spec);

        foreach (Tag tag in spec.effectTemplate.removalTagRequirements.required.list)
        {
            AddTagListener(tag, handle);
        }

        foreach (Tag tag in spec.effectTemplate.removalTagRequirements.ignored.list)
        {
            AddTagListener(tag, handle);
        }

        foreach (Tag tag in spec.effectTemplate.ongoingTagRequirements.required.list)
        {
            AddTagListener(tag, handle);
        }

        foreach (Tag tag in spec.effectTemplate.ongoingTagRequirements.ignored.list)
        {
            AddTagListener(tag, handle);
        }

        return handle;
    }

    public void RemoveExpiredGameplayEffects()
    {

    }

    protected void OnTagCountChanged(Tag tag, int count)
    {

    }
}