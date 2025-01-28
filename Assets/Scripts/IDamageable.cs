using System;
using UnityEngine;

public interface IDamageable {
    public bool Debounce { get; set; }
    public float DamageInvulPeriod { get; }
    
    public abstract void TakeDamage(int damage);

    public abstract Tuple<int,int> GetHealthStats();

    public abstract void SetDebounce(bool val);

}