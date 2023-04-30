using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScatteringDiagrams
{
    class ComplexNumService
    {
        int DBL_MAX_10_EXP = 308; // max decimal exponent

        public ComplexNum GetConjugation(ComplexNum z)
        {
            return GetComplexNum(z.re, -z.im);
        }

        public ComplexNum Multiply(double x, ComplexNum z)
        {
            ComplexNum c;

            c.re = z.re * x;
            c.im = z.im * x;
            return c;
        }

        public ComplexNum Multiply(ComplexNum z, ComplexNum w)
        {
            ComplexNum c;

            c.re = z.re * w.re - z.im * w.im;
            c.im = z.im * w.re + z.re * w.im;
            return c;
        }

        public ComplexNum Divide(double x, ComplexNum w)
        {
            ComplexNum c;
            double r, factor;

            if ((w.re == 0) && (w.im == 0))
                throw new Exception("Attempt to divide scalar by 0+0i");

            if (Math.Abs(w.re) >= Math.Abs(w.im))
            {
                r = w.im / w.re;
                factor = x / (w.re + r * w.im);
                c.re = factor;
                c.im = -r * factor;
            }
            else
            {
                r = w.re / w.im;
                factor = x / (w.im + r * w.re);
                c.im = -factor;
                c.re = r * factor;
            }
            return c;
        }        
        
        public ComplexNum Divide(ComplexNum z, ComplexNum w)
        {
            ComplexNum c;
            double r, denom;

            if ((w.re == 0) && (w.im == 0))
                throw new Exception("Attempt to divide by 0+0i");

            if (Math.Abs(w.re) >= Math.Abs(w.im))
            {
                r = w.im / w.re;
                denom = w.re + r * w.im;
                c.re = (z.re + r * z.im) / denom;
                c.im = (z.im - r * z.re) / denom;
            }
            else
            {
                r = w.re / w.im;
                denom = w.im + r * w.re;
                c.re = (z.re * r + z.im) / denom;
                c.im = (z.im * r - z.re) / denom;
            }
            return c;
        }

        public double Abs(ComplexNum z)
        {
            double x, y, temp;

            x = Math.Abs(z.re);
            y = Math.Abs(z.im);
            if (x == 0.0)
                return y;
            if (y == 0.0)
                return x;

            if (x > y)
            {
                temp = y / x;
                return (x * Math.Sqrt(1.0 + temp * temp));
            }
            temp = x / y;
            return (y * Math.Sqrt(1.0 + temp * temp));
        }

        public ComplexNum Sqr(ComplexNum z)
        {
            return Multiply(z, z);
        }

        public ComplexNum Add(ComplexNum z, ComplexNum w)
        {
            ComplexNum c;

            c.im = z.im + w.im;
            c.re = z.re + w.re;
            return c;
        }

        public ComplexNum Invert(ComplexNum w)
        {
            double r, d;

            if ((w.re == 0) && (w.im == 0))
                throw new Exception("Attempt to invert 0+0i");

            if (Math.Abs(w.re) >= Math.Abs(w.im))
            {
                r = w.im / w.re;
                d = 1 / (w.re + r * w.im);
                return GetComplexNum(d, -r * d);
            }
            r = w.re / w.im;
            d = 1 / (w.im + r * w.re);
            return GetComplexNum(r * d, -d);
        }
        public ComplexNum Tan(ComplexNum z)
        {
            double t, x, y;

            if (z.im == 0)
                return GetComplexNum(Math.Tan(z.re), 0.0);
            if (z.im > DBL_MAX_10_EXP)
                return GetComplexNum(0.0, 1.0);
            if (z.im < -DBL_MAX_10_EXP)
                return GetComplexNum(0.0, -1.0);

            x = 2 * z.re;
            y = 2 * z.im;
            t = Math.Cos(x) + Math.Cosh(y);
            if (t == 0)
                throw new Exception("Complex tangent is infinite");

            return GetComplexNum(Math.Sin(x) / t, Math.Sinh(y) / t);
        }
        public ComplexNum GetComplexNum(double a, double b)
        {
            ComplexNum c;

            c.re = a;
            c.im = b;
            return c;
        }

        public ComplexNum Substract(ComplexNum z, ComplexNum w)
        {
            ComplexNum c;

            c.im = z.im - w.im;
            c.re = z.re - w.re;
            return c;
        }

        public double GetNorm(ComplexNum z)
        {
            return (z.re * z.re + z.im * z.im);
        }        
    }
}
