using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IComStrat<T,V>
{
    void execute(float totaltimedelta,T pawn, V values);
}
