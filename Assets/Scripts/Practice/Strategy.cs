using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IStrategy      //스킬 시스템 사용할때
{
    public bool Skillplay();
}

/*public class Strategy : IStrategy
{
    public bool Skillplay()
    {
        return true;
    }
}*/

public abstract class Strategy
{
    public abstract void Skill();
}

public class Nearing : Strategy
{
    public override void Skill()
    {
    }
}
