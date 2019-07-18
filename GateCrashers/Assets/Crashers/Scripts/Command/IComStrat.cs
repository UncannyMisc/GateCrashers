using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IComStrat<T,V>
{
    void JustPressed(float totaltimedelta,T pawn, V values);
    void JustReleased(float totaltimedelta,T pawn, V values);
    void Held(float totaltimedelta,T pawn, V values);
    void NotHeld(float totaltimedelta,T pawn, V values);
}
