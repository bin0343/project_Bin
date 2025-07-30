using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : IStrategy
{
    public bool Skillplay()
    {
        return true;
    }
}

public class SkillProjectile : Projectile
{

}
