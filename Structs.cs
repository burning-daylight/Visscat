using System;

namespace ScatteringDiagrams
{

    public struct MieTable
    {
        public double angle;
        public double sum; // сумма вероятностей. Для angle = 0 sum = p1, для angle = 1 sum = p1+p2 и т.д.
    };
public struct ScattDiag
{
    public double angle;
    public double intensNatural;
    public double intensPerpen;
    public double intensParallel;

    public double intensNatural_norm;
    public double intensPerpen_norm;
    public double intensParallel_norm;

    public double intensNatural_LogNorm;
    public double intensPerpen_LogNorm;
    public double intensParallel_LogNorm;
};

public struct ComplexNum
{
    public double re;
    public double im;

    public ComplexNum(double Re, double Im)
    {
        this.re = Re;
        this.im = Im;
    }

    public override string ToString()
    {
        return re.ToString();
    }
    public static ComplexNum operator *(double realNum, ComplexNum complNum)
    {
        ComplexNum newComplNum;
        newComplNum.re = complNum.re * realNum;
        newComplNum.im = complNum.im * realNum;
        return newComplNum;
    }

    public static ComplexNum operator *(ComplexNum complNumL, ComplexNum complNumR)
    {
        ComplexNum newComplNum;

        newComplNum.re = complNumL.re * complNumR.re - complNumL.im * complNumR.im;
        newComplNum.im = complNumL.im * complNumR.re + complNumL.re * complNumR.im;
        return newComplNum;
    }
};

}