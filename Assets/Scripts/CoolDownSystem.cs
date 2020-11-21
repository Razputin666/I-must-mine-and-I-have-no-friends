using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolDownSystem : MonoBehaviour
{
    private readonly List<CoolDownData> coolDowns = new List<CoolDownData>();

    private void Update() => ProcessCoolDowns();

    public bool IsOnCoolDown(int id)
    {
        foreach (CoolDownData coolDown in coolDowns)
        {
            if(coolDown.Id == id)
            {
                return true;
            }
        }
        return false;
    }

    public float GetRemainingDuration(int id)
    {
        foreach (CoolDownData coolDown in coolDowns)
        {
            if (coolDown.Id != id)
            {
                continue;
            }

            return coolDown.RemainingTime;
        }
        return 0f;
    }

    public void PutOnCoolDown(HasCoolDownInterFace coolDown)
    {
        coolDowns.Add(new CoolDownData(coolDown));
    }
    private void ProcessCoolDowns()
    {
        float deltaTime = Time.deltaTime;

        for (int i = coolDowns.Count - 1; i >= 0; i--)
        {
            if(coolDowns[i].DecrementCoolDown(deltaTime))
            {
                coolDowns.RemoveAt(i);
            }
        }
    }

}

public class CoolDownData
{
    public CoolDownData(HasCoolDownInterFace coolDown)

    {
        Id = coolDown.Id;
        RemainingTime = coolDown.CoolDownDuration;
    }

    public int Id { get; }
    public float RemainingTime { get; private set; }

    public bool DecrementCoolDown(float deltaTime)
    {
        RemainingTime = Mathf.Max(RemainingTime - deltaTime, 0f);

        return RemainingTime == 0f;
    }
}
