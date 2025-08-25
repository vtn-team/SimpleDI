using UnityEngine;

public interface IDestructable
{
    void SkillDamageOut();
    bool IsActive { get; }
}