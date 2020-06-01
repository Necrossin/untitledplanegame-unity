using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    void proceedDamage(float dmg, bool noSnd = false);
}
