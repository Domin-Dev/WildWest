using UnityEngine;

public interface ILifePoints    
{
    void Hit(int damage, Vector2 dir);
    void Kill();
}

