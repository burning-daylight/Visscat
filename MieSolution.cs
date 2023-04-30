using System;
using System.Linq;
using System.ComponentModel;

namespace ScatteringDiagrams
{
    public class MieSolution : INotifyPropertyChanged
    {
        ComplexNum n_particle = new ComplexNum(1.33, 0.00); // refractive index of particle
        public ComplexNum RefractiveIndexParticles
        {
            get { return n_particle; }
            set
            {
                if (value.re >= 1.0 && value.im <= 0)
                {
                    n_particle = value;
                    n_relative.re = n_particle.re / n_medium;
                    n_relative.im = n_particle.im / n_medium;
                }
                InvokePropertyChanged(new PropertyChangedEventArgs("RefractiveIndexParticleReal"));
            }
        }
        public double RefractiveIndexParticleReal
        {
            get { return n_particle.re; }
            set
            {
                if (value >= 1.0)
                {
                    ComplexNum c;
                    c.re = value;
                    c.im = 0;
                    n_particle = c;

                    n_relative.re = n_particle.re / n_medium;
                    n_relative.im = n_particle.im / n_medium;
                }
                InvokePropertyChanged(new PropertyChangedEventArgs("RefractiveIndexParticleReal"));
            }
        }

        double n_medium = 1.0; // refractive index of medium
        public double RefractiveIndexMedium
        {
            get { return n_medium; }
            set
            {
                if (value >= 1.0)
                    if (CountSizeParameter(radius, value, wavelength_vac) <= sizeParameterLimit)
                    {
                        n_medium = value;
                        wavelength_relative = wavelength_vac / n_medium;
                        n_relative.re = n_particle.re / n_medium;
                        n_relative.im = n_particle.im / n_medium;
                    }

                InvokePropertyChanged(new PropertyChangedEventArgs("RefractiveIndexMedium"));
            }
        }

        ComplexNum n_relative; // n_particle / n_medium

        double sizeParameter; // 2 * pi * radius * n_medium / wavelength
        public double SizeParameter
        {
            get { return sizeParameter; }
        }

        double radius = 0.5; // particle's radius
        public double ParticleRadius
        {
            get { return radius; }
            set
            {
                if (value > 0)
                    if (CountSizeParameter(value, n_medium, wavelength_vac) <= sizeParameterLimit)
                    {
                        radius = value;
                        area = pi * radius * radius;
                    }                
                InvokePropertyChanged(new PropertyChangedEventArgs("ParticleRadius"));
            }
        }

        double wavelength_vac = 0.8; // wavelength in vaccuum
        public double Wavelength
        {
            get { return wavelength_vac; }
            set
            {
                if (value > 0)
                    if (CountSizeParameter(radius, n_medium, value) <= sizeParameterLimit)
                    {
                        wavelength_vac = value;
                        wavelength_relative = wavelength_vac / n_medium;
                    }

                InvokePropertyChanged(new PropertyChangedEventArgs("Wavelength"));
            }
        }
        double wavelength_relative; // relative scattering wavelength

        long anglesAmount = 3600;
        public long AnglesAmount
        {
            get { return anglesAmount; }
            set
            {
                if (value > 0)
                {
                    anglesAmount = value;

                    angles = new double[anglesAmount];
                    for (i = 0; i < anglesAmount; i++)
                        angles[i] = Math.Cos(2 * pi / anglesAmount * i);
                }
                InvokePropertyChanged(new PropertyChangedEventArgs("AnglesAmount"));
            }
        }
        double[] angles;

        double concentration; // spheres per cubic micron
        public double Concentration
        {
            get { return concentration; }
            set
            {
                if (value > 0)
                {
                    if (value <= CountMaxConcentration())
                        concentration = value;
                }
                InvokePropertyChanged(new PropertyChangedEventArgs("Concentration"));
            }
        }

        bool isDistanceSizeRatioUsed = true;
        double particleDistanceSizeRatio = 3; // = distance/radius

        ComplexNum[] s1;
        ComplexNum[] s2;
        double[] intens_parallel;
        double[] intens_perpen;
        double[] intens_natural;        

        // Efficiency factors [without dimension] = microscopic cross-section [cm^2] / geometric cross-section [cm^2]
        double qext; // extinction efficiency
        public double ExtinctionEfficiency
        {
            get { return qext; }
            private set
            {
                qext = value;
            }
        }

        double qsca; // scattering efficiency
        public double ScatteringEfficiency
        {
            get { return qsca; }
            private set
            {
                qsca = value;
            }
        }

        double qback; // backscattering efficiency
        public double BackScatteringEfficiency
        {
            get { return qback; }
            private set
            {
                qback = value;
            }
        }

        double g; // anisotropy factor (average cosine of scattering)
        public double AnisotropyFactor
        {
            set
            {
                g = value;
                if (onAnisotropyFactorChanged != null)
                    onAnisotropyFactorChanged(value, new EventArgs());
            }
            get { return g; }
        }
       
        // Macroscopic cross-sections [mm^-1, cm^-1]
        double mut; // extinction coefficient
        public double ExtinctionCoeff
        {
            set
            {
                mut = value;
                if (onExtinctionCoeffChanged != null)
                    onExtinctionCoeffChanged(value, new EventArgs());
            }
            get { return mut; }
        }

        double mus; // scattering coefficient
        public double ScatteringCoeff
        {
            set
            {
                mus = value;
                if (onScatteringCoeffChanged != null)
                    onScatteringCoeffChanged(value, new EventArgs());
            }
            get { return mus; }
        }

        MieTable[] miePhaseFuncTable;
        public MieTable[] MiePhaseFuncTable
        {
            get
            { 
                return miePhaseFuncTable; 
            }
        }

        double musp; // reduced scattering coefficient
                
        ComplexNumService complex;
        double pi;
        double area;
        long i = 0;

        double sizeParameterLimit = 20000;

        public static event EventHandler onScatteringCoeffChanged;    
        public static event EventHandler onExtinctionCoeffChanged;        
        public static event EventHandler onAnisotropyFactorChanged;

        public MieSolution()
        {            
            Concentration = CountMaxConcentration();
            MieSolutionInit();            
        }

        public MieSolution(double radius, double concentration, double refrIndMedium, ComplexNum refrIndParticle, double wavelength, bool isDistanceSizeRatioUsed, double distanceSizeRatio)
        {
            SetParameters(wavelength, radius, concentration, refrIndMedium, refrIndParticle, distanceSizeRatio, isDistanceSizeRatioUsed);                                 
            MieSolutionInit();
        }

        private void MieSolutionInit()
        {
            pi = Math.PI;            
            sizeParameter = CountSizeParameter(radius, n_medium, wavelength_vac);            

            n_relative.re = n_particle.re / n_medium;
            n_relative.im = n_particle.im / n_medium;
            area = pi * radius * radius;
            wavelength_relative = wavelength_vac / n_medium;

            angles = new double[anglesAmount];
            for (i = 0; i < anglesAmount; i++)
                angles[i] = Math.Cos(2 * pi / anglesAmount * i);

            complex = new ComplexNumService();
        }

        public void SetParameters(double Wavelength, double Radius, double Concentration,
            double RefrIndMedium, ComplexNum RefrIndParticle, double DistSizeRatio, bool IsDistSizeRatioUsed)
        {
            this.particleDistanceSizeRatio = DistSizeRatio;
            this.isDistanceSizeRatioUsed = IsDistSizeRatioUsed;
            this.RefractiveIndexMedium = RefrIndMedium;
            this.RefractiveIndexParticles = RefrIndParticle;            
            this.Wavelength = Wavelength;
            this.ParticleRadius = Radius;
            this.Concentration = Concentration;                    
        }
        
        private double CountMaxConcentration()
        {
            if (isDistanceSizeRatioUsed)
                return 1 / (4.0 / 3.0 * Math.PI * Math.Pow(radius + particleDistanceSizeRatio / 2, 3));
            else
                return 1 / (4.0 / 3.0 * Math.PI * Math.Pow(radius, 3));
        }

        private double CountSizeParameter(double radius, double n_medium, double wavelength_vac)
        {
            sizeParameter = 2 * pi * radius * n_medium / wavelength_vac;
            return sizeParameter;
        }

        private void CountCoeffs()
        {
            ExtinctionCoeff = concentration * qext * area; // mut
            ScatteringCoeff = concentration * qsca * area; // mus
            musp = mus * (1.0 - g);
        }

        public ScattDiag[] CalculateScatteringDiagram()
        {
            ScattDiag[] diagramArr = new ScattDiag[anglesAmount];
            miePhaseFuncTable = new MieTable[anglesAmount / 2 + 1];

            intens_parallel = new double[anglesAmount];
            intens_perpen = new double[anglesAmount];
            intens_natural = new double[anglesAmount];
            s1 = new ComplexNum[anglesAmount];
            s2 = new ComplexNum[anglesAmount];

            if ((n_particle.re == 0) && (sizeParameter < 0.1))            
                Small_conducting_Mie();   
            else             
            if ((n_particle.re > 0.0) && (complex.Abs(n_particle) * sizeParameter < 0.1))            
                Small_Mie();
            else            
                Mie();
            
            CountCoeffs();

            int j;
            double max_natural, max_perpen, max_parallel;

            for (i = 0; i < anglesAmount; ++i)
            {
                intens_parallel[i] = complex.GetNorm(s2[i]) / (sizeParameter * sizeParameter * qsca) / pi;
                intens_perpen[i] = complex.GetNorm(s1[i]) / (sizeParameter * sizeParameter * qsca) / pi;
                intens_natural[i] = (intens_parallel[i] + intens_perpen[i]) / 2.0;
            }

            max_natural = intens_natural.Max();
            max_perpen = intens_perpen.Max();
            max_parallel = intens_parallel.Max();

            double sum_natural = intens_natural.Sum();
            double sum_perpen = intens_perpen.Sum();
            double sum_parallel = intens_parallel.Sum();

            for (j = 0; j < anglesAmount; j++)
            {
                double angle, d, polar, s33, s34;
                ComplexNum t;

                i = j + anglesAmount / 2 + 1;
                if (i >= anglesAmount)
                    i -= anglesAmount;

                if (i <= anglesAmount / 2)
                    angle = 180.0 / pi * Math.Acos(angles[i]);
                else
                    angle = -180.0 / pi * Math.Acos(angles[i]);

                t = complex.Multiply(s2[i], complex.GetConjugation(s1[i]));
                d = (complex.GetNorm(s1[i]) + complex.GetNorm(s2[i])) / 2.0;
                polar = (complex.GetNorm(s1[i]) - complex.GetNorm(s2[i])) / 2.0 / d;
                s33 = (t.re) / d;
                s34 = -(t.im) / d;

                diagramArr[j].angle = angle;
                /*diagramArr[j].intensNatural_norm = intens_natural[i] / max_natural;
                diagramArr[j].intensPerpen_norm = intens_perpen[i] / max_perpen;
                diagramArr[j].intensParallel_norm = intens_parallel[i] / max_parallel;*/
                diagramArr[j].intensNatural_norm = intens_natural[i] / sum_natural;
                diagramArr[j].intensPerpen_norm = intens_perpen[i] / sum_perpen;
                diagramArr[j].intensParallel_norm = intens_parallel[i] / sum_parallel;

                diagramArr[j].intensNatural = intens_natural[i];
                diagramArr[j].intensPerpen = intens_perpen[i];
                diagramArr[j].intensParallel = intens_parallel[i];
                diagramArr[j].intensNatural_LogNorm = Math.Log10(diagramArr[j].intensNatural_norm / 0.001);
                diagramArr[j].intensPerpen_LogNorm = Math.Log10(diagramArr[j].intensPerpen_norm / 0.001);
                diagramArr[j].intensParallel_LogNorm = Math.Log10(diagramArr[j].intensParallel_norm / 0.001);
            }
            
            /*double sum = 0;
            double max = -1; int ind = 0;
            for (j = 0; j < anglesAmount; j++)
            {
                if (max < diagramArr[j].intensNatural_norm)
                {
                    max = diagramArr[j].intensNatural_norm;
                    ind = j;
                }
                sum += diagramArr[j].intensNatural_norm;
            }*/
            FillMieTable(diagramArr);
            return diagramArr;
        }

        private void FillMieTable(ScattDiag[] diag)
        {
            if (miePhaseFuncTable != null) // !!!!!!!!!!!!!!!!!!!!! плохое условие! Не всегда может эта таблица обновляться при изменении параметров пользователем
            {
                int length = miePhaseFuncTable.Length;
                double sum = 0; // содержит сумму интенсивностей по всем рассчитываемым углам
                // начальное заполнение таблицы углами и значения интенсивности
                for (int i = 0; i < length; i++)
                {
                    miePhaseFuncTable[i].angle = Math.Round(diag[length - 2 + i].angle, 3);
                    miePhaseFuncTable[i].sum = diag[length - 2 + i].intensNatural;// _norm;
                    sum += miePhaseFuncTable[i].sum;
                }

                for (int i = 0; i < length; i++)                
                    miePhaseFuncTable[i].sum /= sum;                                    
                
                // сортировка по возрастанию по значениям суммы
                MieTable temp = new MieTable();
                for (int i = 0; i < length; i++)
                    for (int j = i; j < length; j++)
                    {
                        if (miePhaseFuncTable[j].sum < miePhaseFuncTable[i].sum)
                        {
                            temp = miePhaseFuncTable[i];
                            miePhaseFuncTable[i] = miePhaseFuncTable[j];
                            miePhaseFuncTable[j] = temp;
                        }
                    }
                
                for (int i = 1; i < length; i++)                
                    miePhaseFuncTable[i].sum += miePhaseFuncTable[i-1].sum;               
            }
        }
        /*
        private void NormalizeArray(PhaseFunction[] points)
        {
            if (points != null)
            {
                double max = points[0].intens;
                for (uint j = 0; j < points.Length; j++)
                    if (points[j].intens > max)
                        max = points[j].intens;
                for (uint j = 0; j < points.Length; j++)
                    points[j].intens /= max;
            }
        }
        */
        private void Mie()
        {
            ComplexNum[] D;
            ComplexNum z1, an, bn, bnm1, anm1, qbcalc;
            double[] pi0, pi1, tau;
            ComplexNum xi, xi0, xi1;
            double psi, psi0, psi1;
            double alpha, beta, factor;
            long n, k, nstop, sign;

            qext = -1;
            qsca = -1;
            qback = -1;
            g = -1;

            nstop = (long)Math.Floor(sizeParameter + 4.05 * Math.Pow(sizeParameter, 0.33333) + 2.0);

            pi0 = new double[anglesAmount];
            pi1 = new double[anglesAmount];
            tau = new double[anglesAmount];
            FillArray(ref pi0, 0.0);
            FillArray(ref tau, 0.0);
            FillArray(ref pi1, 1.0);

            D = new ComplexNum[nstop + 1];
            if (n_relative.re > 0) 
            {
                ComplexNum z;

                z = complex.Multiply(sizeParameter, n_relative);
                
                if (Math.Abs(n_relative.im * sizeParameter) < ((13.78 * n_relative.re - 10.8) * n_relative.re + 3.9))
                    Dn_up(z, nstop, D);
                else
                    Dn_down(z, nstop, D);
            }

            psi0 = Math.Sin(sizeParameter);
            psi1 = psi0 / sizeParameter - Math.Cos(sizeParameter);
            xi0 = complex.GetComplexNum(psi0, Math.Cos(sizeParameter));
            xi1 = complex.GetComplexNum(psi1, Math.Cos(sizeParameter) / sizeParameter + Math.Sin(sizeParameter));
            qsca = 0.0;
            g = 0.0;
            qext = 0.0;
            sign = 1;
            qbcalc = complex.GetComplexNum(0.0, 0.0);
            anm1 = complex.GetComplexNum(0.0, 0.0);
            bnm1 = complex.GetComplexNum(0.0, 0.0);

            for (n = 1; n <= nstop; n++)
            {

                if (n_relative.re == 0.0)
                {
                    an = complex.Divide(n * psi1 / sizeParameter - psi0, complex.Substract(complex.Multiply(n / sizeParameter, xi1), xi0));
                    bn = complex.Divide(psi1, xi1);
                }
                else if (n_relative.im == 0.0)
                {
                    z1.re = D[n].re / n_relative.re + n / sizeParameter;
                    an = complex.Divide(z1.re * psi1 - psi0, complex.Substract(complex.Multiply(z1.re, xi1), xi0));

                    z1.re = D[n].re * n_relative.re + n / sizeParameter;
                    bn = complex.Divide(z1.re * psi1 - psi0, complex.Substract(complex.Multiply(z1.re, xi1), xi0));
                }
                else
                {
                    z1 = complex.Divide(D[n], n_relative);
                    z1.re += n / sizeParameter;
                    an = complex.Divide(complex.GetComplexNum(z1.re * psi1 - psi0, z1.im * psi1), complex.Substract(complex.Multiply(z1, xi1), xi0));

                    z1 = complex.Multiply(D[n], n_relative);
                    z1.re += n / sizeParameter;
                    bn = complex.Divide(complex.GetComplexNum(z1.re * psi1 - psi0, z1.im * psi1), complex.Substract(complex.Multiply(z1, xi1), xi0));
                }



                for (k = 0; k < anglesAmount; k++)
                {
                    factor = (2.0 * n + 1.0) / (n + 1.0) / n;
                    tau[k] = n * angles[k] * pi1[k] - (n + 1) * pi0[k];
                    alpha = factor * pi1[k];
                    beta = factor * tau[k];
                    s1[k].re += alpha * an.re + beta * bn.re;
                    s1[k].im += alpha * an.im + beta * bn.im;
                    s2[k].re += alpha * bn.re + beta * an.re;
                    s2[k].im += alpha * bn.im + beta * an.im;
                }

                for (k = 0; k < anglesAmount; k++)
                {
                    factor = pi1[k];
                    pi1[k] = ((2.0 * n + 1.0) * angles[k] * pi1[k] - (n + 1.0) * pi0[k]) / n;
                    pi0[k] = factor;
                }



                factor = 2.0 * n + 1.0;
                g += (n - 1.0 / n) * (anm1.re * an.re + anm1.im * an.im + bnm1.re * bn.re + bnm1.im * bn.im);
                g += factor / n / (n + 1.0) * (an.re * bn.re + an.im * bn.im);
                qsca += factor * (complex.GetNorm(an) + complex.GetNorm(bn));
                qext += factor * (an.re + bn.re);
                sign *= -1;
                qbcalc.re += sign * factor * (an.re - bn.re);
                qbcalc.im += sign * factor * (an.im - bn.im);



                factor = (2.0 * n + 1.0) / sizeParameter;
                xi = complex.Substract(complex.Multiply(factor, xi1), xi0);
                xi0 = xi1;
                xi1 = xi;

                psi = factor * psi1 - psi0;
                psi0 = psi1;
                psi1 = xi1.re;

                anm1 = an;
                bnm1 = bn;
            }


            ScatteringEfficiency *= 2 / (sizeParameter * sizeParameter);
            qext *= 2 / (sizeParameter * sizeParameter);
            AnisotropyFactor *= 4 / (qsca) / (sizeParameter * sizeParameter);
            qback = complex.GetNorm(qbcalc) / (sizeParameter * sizeParameter);            
        }

        private void Small_Mie()
        {
            ComplexNum ahat1, ahat2, bhat1;
            ComplexNum z0, m2, m4;
            double x2, x3, x4;

            if ((s1 == null) || (s2 == null))
                anglesAmount = 0;

            m2 = complex.Sqr(n_relative);
            m4 = complex.Sqr(m2);
            x2 = sizeParameter * sizeParameter;
            x3 = x2 * sizeParameter;
            x4 = x2 * x2;
            z0.re = -m2.im;
            z0.im = m2.re - 1;
            {
                ComplexNum z1, z2, z3, z4, D;

                z1 = complex.Multiply(2.0 / 3.0, z0);
                z2.re = 1.0 - 0.1 * x2 + (4.0 * m2.re + 5.0) * x4 / 1400.0;
                z2.im = 4.0 * x4 * m2.im / 1400.0;
                z3 = complex.Multiply(z1, z2);

                z4 = complex.Multiply(x3 * (1.0 - 0.1 * x2), z1);
                D.re = 2.0 + m2.re + (1 - 0.7 * m2.re) * x2 - (8.0 * m4.re - 385.0 * m2.re + 350.0) / 1400.0 * x4 + z4.re;
                D.im = m2.im + (-0.7 * m2.im) * x2 - (8.0 * m4.im - 385.0 * m2.im) / 1400.0 * x4 + z4.im;

                ahat1 = complex.Divide(z3, D);
            }
            {
                ComplexNum z2, z6, z7;

                z2 = complex.Multiply(x2 / 45.0, z0);
                z6.re = 1.0 + (2.0 * m2.re - 5.0) * x2 / 70.0;
                z6.im = m2.im * x2 / 35.0;
                z7.re = 1.0 - (2.0 * m2.re - 5.0) * x2 / 30.0;
                z7.im = -m2.im * x2 / 15.0;
                bhat1 = complex.Multiply(z2, complex.Divide(z6, z7));
            }
            {
                ComplexNum z3, z8;

                z3 = complex.Multiply((1.0 - x2 / 14.0) * x2 / 15.0, z0);
                z8.re = 2.0 * m2.re + 3.0 - (m2.re / 7.0 - 0.5) * x2;
                z8.im = 2.0 * m2.im - m2.im / 7.0 * x2;
                ahat2 = complex.Divide(z3, z8);
            }
            {
                ComplexNum ss1;
                double T;

                T = complex.GetNorm(ahat1) + complex.GetNorm(bhat1) + (5.0 / 3.0) * complex.GetNorm(ahat2);
                qsca = 6.0 * x4 * T;
                qext = 6.0 * sizeParameter * (ahat1.re + bhat1.re + (5.0 / 3.0) * ahat2.re);
                g = (ahat1.re * (ahat2.re + bhat1.re) + ahat1.im * (ahat2.im + bhat1.im)) / T;
                ss1.re = 1.5 * x2 * (ahat1.re - bhat1.re - (5.0 / 3.0) * ahat2.re);
                ss1.im = 1.5 * x2 * (ahat1.im - bhat1.im - (5.0 / 3.0) * ahat2.im);
                qback = 4 * complex.GetNorm(ss1);
            }
            {
                double muj, angle;
                long j;

                x3 *= 1.5;
                ahat1.re *= x3;
                ahat1.im *= x3;
                bhat1.re *= x3;
                bhat1.im *= x3;
                ahat2.re *= x3 * (5.0 / 3.0);
                ahat2.im *= x3 * (5.0 / 3.0);
                for (j = 0; j < anglesAmount; j++)
                {
                    muj = angles[j];
                    angle = 2.0 * muj * muj - 1.0;
                    s1[j].re = ahat1.re + (bhat1.re + ahat2.re) * muj;
                    s1[j].im = ahat1.im + (bhat1.im + ahat2.im) * muj;
                    s2[j].re = bhat1.re + ahat1.re * muj + ahat2.re * angle;
                    s2[j].im = bhat1.im + ahat1.im * muj + ahat2.im * angle;
                }
            }
        }

        private void Small_conducting_Mie()
        {
            ComplexNum ahat1, ahat2, bhat1, bhat2;
            ComplexNum ss1;
            double x2, x3, x4, muj, angle;
            long j;

            if ((s1 == null) || (s2 == null))
                anglesAmount = 0;

            n_relative.re += 0.0;
            x2 = sizeParameter * sizeParameter;
            x3 = x2 * sizeParameter;
            x4 = x2 * x2;

            ahat1 = complex.Divide(complex.GetComplexNum(0.0, 2.0 / 3.0 * (1.0 - 0.2 * x2)), complex.GetComplexNum(1.0 - 0.5 * x2, 2.0 / 3.0 * x3));
            bhat1 = complex.Divide(complex.GetComplexNum(0.0, (x2 - 10.0) / 30.0), complex.GetComplexNum(1 + 0.5 * x2, -x3 / 3.0));
            ahat2 = complex.GetComplexNum(0.0, x2 / 30.0);
            bhat2 = complex.GetComplexNum(0.0, -x2 / 45.0);

            qsca = 6.0 * x4 * (complex.GetNorm(ahat1) + complex.GetNorm(bhat1) +
                    (5.0 / 3.0) * (complex.GetNorm(ahat2) + complex.GetNorm(bhat2)));
            qext = qsca;
            g = 6.0 * x4 * (ahat1.im * (ahat2.im + bhat1.im) +
                     bhat2.im * (5.0 / 9.0 * ahat2.im + bhat1.im) +
                     ahat1.re * bhat1.re) / (qsca);

            ss1.re = 1.5 * x2 * (ahat1.re - bhat1.re);
            ss1.im = 1.5 * x2 * (ahat1.im - bhat1.im - (5.0 / 3.0) * (ahat2.im + bhat2.im));
            qback = 4 * complex.GetNorm(ss1);

            x3 *= 1.5;
            ahat1.re *= x3;
            ahat1.im *= x3;
            bhat1.re *= x3;
            bhat1.im *= x3;
            ahat2.im *= x3 * (5.0 / 3.0);
            bhat2.im *= x3 * (5.0 / 3.0);
            for (j = 0; j < anglesAmount; j++)
            {
                muj = angles[j];
                angle = 2.0 * muj * muj - 1.0;
                s1[j].re = ahat1.re + (bhat1.re) * muj;
                s1[j].im = ahat1.im + (bhat1.im + ahat2.im) * muj + bhat2.im * angle; ;
                s2[j].re = bhat1.re + (ahat1.re) * muj;
                s2[j].im = bhat1.im + (ahat1.im + bhat2.im) * muj + ahat2.im * angle;
            }
        }


        private void FillArray(ref double[] destArr, double filler)
        {
            for (int i = 0; i < destArr.Length; i++)
                destArr[i] = filler;
        }

        private void FillArray(ref ComplexNum[] destArr, ComplexNum filler)
        {
            for (int i = 0; i < destArr.Length; i++)
                destArr[i] = filler;
        }

        private void Dn_up(ComplexNum z, long nstop, ComplexNum[] D)
        {
            ComplexNum zinv, k_over_z;
            long k;

            D[0] = complex.Invert(complex.Tan(z));
            zinv = complex.Invert(z);

            for (k = 1; k < nstop; k++)
            {
                k_over_z = complex.Multiply((double)k, zinv);
                D[k] = complex.Substract(complex.Invert(complex.Substract(k_over_z, D[k - 1])), k_over_z);
            }
        }

        private void Dn_down(ComplexNum z, long nstop, ComplexNum[] D)
        {
            long k;
            ComplexNum zinv, k_over_z;

            D[nstop - 1] = Lentz_Dn(z, nstop);
            zinv = complex.Invert(z);

            for (k = nstop - 1; k >= 1; k--)
            {
                k_over_z = complex.Multiply((double)k, zinv);
                D[k - 1] = complex.Substract(k_over_z, complex.Invert(complex.Add(D[k], k_over_z)));
            }
        }

        ComplexNum Lentz_Dn(ComplexNum z, long n)
        {
            ComplexNum alpha_j1, alpha_j2, zinv, aj;
            ComplexNum alpha, result, ratio, runratio;

            zinv = complex.Divide(2.0, z);
            alpha = complex.Multiply(n + 0.5, zinv);
            aj = complex.Multiply(-n - 1.5, zinv);
            alpha_j1 = complex.Add(aj, complex.Invert(alpha));
            alpha_j2 = aj;
            ratio = complex.Divide(alpha_j1, alpha_j2);
            runratio = complex.Multiply(alpha, ratio);

            do
            {
                aj.re = zinv.re - aj.re;
                aj.im = zinv.im - aj.im;
                alpha_j1 = complex.Add(complex.Invert(alpha_j1), aj);
                alpha_j2 = complex.Add(complex.Invert(alpha_j2), aj);
                ratio = complex.Divide(alpha_j1, alpha_j2);
                zinv.re *= -1;
                zinv.im *= -1;
                runratio = complex.Multiply(ratio, runratio);
            }
            while (Math.Abs(complex.Abs(ratio) - 1.0) > 1e-12);

            result = complex.Add(complex.Divide((double)-n, z), runratio);
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}
