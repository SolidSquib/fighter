using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Concrete class for a wrapped list of Tags.  Includes events for adding and removing tags.
/// The provided functions allow containers to be checked for matching tags.
/// </summary>
[System.Serializable]
public class CountingTagContainer : TagContainer
{
    public event CountingTagEventHandler TagCountChanged;

    Dictionary<Tag, int> _tagCounts = new Dictionary<Tag, int>();

    public CountingTagContainer() { }
    public CountingTagContainer(List<Tag> tags)
    {
        foreach (Tag tag in tags)
        {
            AddTag(tag);
        }
    }


    public void TriggerTagCountChanged(Tag tag, int newCount)
    {
        if (TagCountChanged != null && tag != null) { TagCountChanged(this, new CountingTagEventArgs(tag, newCount)); }
    }
   
    #region MANAGEMENT_FUNCTIONS
    public override void AddTag(Tag tag)
    {
        int count;
        if (_tagCounts.TryGetValue(tag, out count))
        {
            _tagCounts[tag] = (count + 1);
        }
        else
        {
            count = 1;
            _tagCounts.Add(tag, count);
            base.AddTag(tag);
        }

        TriggerTagCountChanged(tag, count);        
    }
    public override void RemoveTag(Tag tag)
    {
        int count;
        if (_tagCounts.TryGetValue(tag, out count))
        {
            count -= 1;
            _tagCounts[tag] = count;

            if (count <= 0)
            {
                _tagCounts.Remove(tag);
                base.RemoveTag(tag);
            }
            
            TriggerTagCountChanged(tag, count);            
        }
    }
    public override void ClearTags()
    {
        _tagCounts.Clear();
        base.ClearTags();
    }

    public override void AddTags(TagContainer oContainer)
    {
        for (int i = 0; i < oContainer.list.Count; ++i)
        {
            AddTag(oContainer.list[i]);
        }
    }
    public override void RemoveTags(TagContainer oContainer)
    {
        for (int i = 0; i < oContainer.list.Count; ++i)
        {
            RemoveTag(oContainer.list[i]);
        }
    }
    #endregion
}
