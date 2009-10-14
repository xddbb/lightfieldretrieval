using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DescriptionExtractor
{
    public class Polynomial
    {
        protected double[] coefficients;

        public Polynomial(double[] coeff)
        {
            int n = coeff.Length;
            coefficients = new double[n];
            for (int i = 0; i < n; i++)
            {
                coefficients[i] = coeff[i];
                //coefficients[i] = coeff[n-i-1];
            }
        }

        public double Value(double x)
        {
            double sum = coefficients[0];
            for (int i = 1; i < coefficients.Length; i++)
            {
                sum = sum * x + coefficients[i];
            }

            return sum;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int n = coefficients.Length;
            for (int i = 0; i < n; i++)
            {
                double c = coefficients[i];                
                if(c > 0.0)
                    sb.Append("+" + coefficients[i] + "x^" + (n-i-1));
                else if(c < 0.0)
                    sb.Append(coefficients[i] + "x^" + (n-i-1));
            }

            return sb.ToString();
        }
    }
}
