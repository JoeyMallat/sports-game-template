using UnityEngine;

[System.Serializable]
public class Contract
{
    [SerializeField] int _yearsOnContract;
    [SerializeField] int _yearlySalary;

    public Contract(int rating, int age)
    {
        _yearsOnContract = UnityEngine.Random.Range(1, 6);
        _yearlySalary = rating * age * UnityEngine.Random.Range(3000, 5000);
    }

    public Contract(int pick)
    {
        if (pick >= 30) pick = 30;

        _yearlySalary = Mathf.RoundToInt(10000000 * ConfigManager.Instance.GetCurrentConfig().RookieSalaryScale.Evaluate(pick / 30f));
        _yearsOnContract = 2;
    }

    public int GetYearsOnContract()
    {
        return _yearsOnContract;
    }

    public int GetYearlySalary()
    {
        return _yearlySalary;
    }

    public int GetTotalContractValue()
    {
        return _yearsOnContract * _yearlySalary;
    }

    public void SetNewContract(int newSalary, int newLength)
    {
        _yearlySalary = newSalary;
        _yearsOnContract = newLength;
    }

    public void DecreaseYearsOnContract()
    {
        _yearsOnContract--;
    }
}
