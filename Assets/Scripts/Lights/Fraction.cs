using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fraction
{
    public int numerator;
    public int denominator;

    public Fraction(int numerator, int denominator)
    {
        this.numerator = numerator;
        this.denominator = denominator;
    }

    public Fraction(Fraction fraction)
    {
        numerator = fraction.numerator;
        denominator = fraction.denominator;
    }

    public override bool Equals(object obj)
    {
        Fraction other = obj as Fraction;
        return (numerator == other.numerator && denominator == other.denominator);
    }

    public static bool operator ==(Fraction f1, Fraction f2)
    {
        return f1.Equals(f2);
    }

    public static bool operator !=(Fraction f1, Fraction f2)
    {
        return !(f1 == f2);
    }

    public override int GetHashCode()
    {
        return numerator * denominator;
    }

    public override string ToString()
    {
        return numerator + "/" + denominator;
    }

    //Helper function, simplifies a fraction.
    public Fraction Simplify()
    {
        for (int divideBy = denominator; divideBy > 0; divideBy--)
        {
            bool divisible = true;

            if ((int)(numerator / divideBy) * divideBy != numerator)
            {
                divisible = false;
            }
            else if ((int)(denominator / divideBy) * divideBy != denominator)
            {
                divisible = false;
            }
            else if (divisible)
            {
                numerator /= divideBy;
                denominator /= divideBy;
            }
        }
        return this;
    }
}
