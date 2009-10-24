using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DescriptionExtractor;

namespace FeatureComparator
{
	public class Comparator
	{		
		protected LightFieldDescriptor lfdscA;
		protected LightFieldDescriptor lfdscB;
		protected double alpha;
		protected double beta;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lfdA"></param>
		/// <param name="lfdB"></param>
		/// <param name="alpha">Scaling for the zernike vector</param>
		/// <param name="beta">Scaling for the fourier vector</param>
		public Comparator(LightFieldDescriptor lfdA, LightFieldDescriptor lfdB, double alpha, double beta)
		{
			lfdscA = lfdA;
			lfdscB = lfdB;
			this.alpha = alpha;
			this.beta = beta;
		}

		/// <summary>
		/// Calucuates the euclidean distance for two vectors, assuming they have the same dimension
		/// </summary>
		/// <param name="vecA">The first vector</param>
		/// <param name="vecB">The second vector</param>
		public static double EuclideanDistance(double[] vecA, double[] vecB)
		{
			double sum = 0;
			for (int i = 0; i < vecA.Length; i++)
			{
				double t = (vecA[i] - vecB[i]);
				sum += t*t;
			}
			return Math.Sqrt(sum);
		}

		/// <summary>
		/// Compares arrays of image features, one by one
		/// </summary>
		/// <param name="aligmentA">First array of image features</param>
		/// <param name="aligmentB">Second array of image features</param>
		/// <returns>The distance</returns>
		protected double Compare(int[] aligmentA, int[] aligmentB)
		{
			double sum = 0.0;
			//
			double zsum = 0.0;
			double fsum = 0.0;
			//
			for (int i = 0; i < 20; i++)
			{
				FeatureVector fvA = lfdscA.GetImageFeatures(aligmentA[i]);
				FeatureVector fvB = lfdscB.GetImageFeatures(aligmentB[i]);
				//
				double z = alpha * EuclideanDistance(fvA.zernike, fvB.zernike);
				double f = beta * EuclideanDistance(fvA.fourier, fvB.fourier);				
				//
				sum += (f + z);
				//
				fsum += f;
				zsum += z;
			}
			//Console.Write("z=" + zsum + ", f=" + fsum); 
			//Console.Write("z/f=" + zsum / fsum); 
			//
			return sum;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public double Compare()
		{
			DodecahedronAlign ddcal = new DodecahedronAlign();

			//////////////////////////////////////////////////////////////////////
			// Create the combinations by picking a vertiex on the dodecahedron,
			// then apply the three rotations and compare each with the unrotated
			// dodecahedron. Use the rotation with minimal distance.
			//////////////////////////////////////////////////////////////////////			
			int[] ajdA = ddcal.DodecahedronGraph.GetAdjacent(0);
			int[] basealg = ddcal.GetAligment(0, ajdA[0], ajdA[1], ajdA[2]);
			//
			double min = Double.PositiveInfinity;
			for (int i = 0; i < 20; i++)
			{				
				//////////////////////////////////////////////////////////////////////
				// Do the three rotations for the choosen vertex combination
				//////////////////////////////////////////////////////////////////////
				// Get the three adjecent vertices to the 					
				int[] ajdB = ddcal.DodecahedronGraph.GetAdjacent(i);
				// Generate the three rotations
				for (int j = 0; j < 3; j++)
				{
					int[] aligment = ddcal.GetAligment(i, ajdB[j % 3], ajdB[(j + 1) % 3], ajdB[(j + 2) % 3]);
					double dist = Compare(basealg, aligment);
					if (dist < min)
						min = dist;
				}			
			}

			return min;
		}
	}
}
