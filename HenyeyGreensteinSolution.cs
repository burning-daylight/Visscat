using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ScatteringDiagrams
{
    public struct PhaseFunction
    {
        public double angle;
        public double intens;
    };

    public class HenyeyGreensteinSolution : INotifyPropertyChanged
    {
        double g = 0.5;
        public double AnisotropyFactor
        {
            get { return g; }
            set
            {
                if (value >= -1 && value <= 1)
                    g = value;
                InvokePropertyChanged(new PropertyChangedEventArgs("AnisotropyFactor"));
            }
        }

        uint anglesAmount = 360;
        public uint AnglesAmount
        {
            get { return anglesAmount; }
            set
            {
                if (value > 0)                
                    anglesAmount = value;                                    
                InvokePropertyChanged(new PropertyChangedEventArgs("AnglesAmount"));
            }
        }

        public PhaseFunction[] CalculateHGScatteringDiagram()
        {
            PhaseFunction[] scattDiag = new PhaseFunction[anglesAmount];
            double angle = 0;

            for (uint j = 0; j < anglesAmount; j++)
            {                
                angle = 2 * Math.PI / anglesAmount * j; // in radians
                scattDiag[j].angle = j / (double)anglesAmount * 360; // in degrees
                scattDiag[j].intens = (1 - g * g) / Math.Pow(1 + g * g - 2 * g * Math.Cos(angle), 1.5);
            }
            NormalizeArray(scattDiag);
            return scattDiag;
        }

        public PhaseFunction[] CalculateHGModifiedScatteringDiagram()
        {
            PhaseFunction[] scattDiag = new PhaseFunction[anglesAmount];
            double angle = 0;

            for (uint j = 0; j < anglesAmount; j++)
            {
                angle = 2 * Math.PI / anglesAmount * j; // in radians
                scattDiag[j].angle = j / (double)anglesAmount * 360; // in degrees
                scattDiag[j].intens = 1.5 * (1 - g * g) / (2 + g * g) * (1 + Math.Cos(angle) * Math.Cos(angle)) / Math.Pow(1 + g * g - 2 * g * Math.Cos(angle), 1.5);
            }
            //NormalizeArray(scattDiag);
            return scattDiag;
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}
